using AutoMapper;
using MfaApi.Authorization;
using MfaApi.Database;
using MfaApi.Entities;
using MfaApi.Helpers;
using MfaApi.Models.Accounts;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using BCryptNet = BCrypt.Net.BCrypt;

namespace MfaApi.Services;

public interface IAccountService
{
    AuthenticateResponse Authenticate(AuthenticateRequest model, string? ipAddress);
    AuthenticateResponse RefreshToken(string? token, string? ipAddress);
    void RevokeToken(string token, string? ipAddress);
    void Register(RegisterRequest model, string? origin);
    void VerifyEmail(string token);
    void ForgotPassword(ForgotPasswordRequest model, string? origin);
    void ValidateResetToken(ValidateResetTokenRequest model);
    void ResetPassword(ResetPasswordRequest model);
    IEnumerable<AccountResponse> GetAll();
    AccountResponse GetById(int id);
    AccountResponse Create(CreateRequest model);
    AccountResponse Update(int id, UpdateRequest model);
    void Delete(int id);
}

public class AccountService : IAccountService
{
    private readonly DataContext _context;
    private readonly IJwtUtils _jwtUtils;
    private readonly IMapper _mapper;
    private readonly AppSettings _appSettings;
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;

    public AccountService(
        DataContext context,
        IJwtUtils jwtUtils,
        IMapper mapper,
        IOptions<AppSettings> appSettings,
        IEmailService emailService,
        ITemplateService templateService)
    {
        _context = context;
        _jwtUtils = jwtUtils;
        _mapper = mapper;
        _appSettings = appSettings.Value;
        _emailService = emailService;
        _templateService = templateService;
    }

    public AuthenticateResponse Authenticate(AuthenticateRequest model, string? ipAddress)
    {
        var account = _context.Accounts.SingleOrDefault(x => x.Email == model.Email);

        // validate
        if (account == null || !account.IsVerified || !BCryptNet.Verify(model.Password, account.PasswordHash))
            throw new AppException("Email ou senha estão incorretos", HttpStatusCode.Unauthorized);

        // authentication successful so generate jwt and refresh tokens
        var jwtToken = _jwtUtils.GenerateJwtToken(account);
        var refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
        account.RefreshTokens.Add(refreshToken);

        // remove old refresh tokens from account
        RemoveOldRefreshTokens(account);

        // save changes to db
        _context.Update(account);
        _context.SaveChanges();

        var response = _mapper.Map<AuthenticateResponse>(account);
        response.JwtToken = jwtToken;
        response.RefreshToken = refreshToken.Token;
        return response;
    }

    public AuthenticateResponse RefreshToken(string? token, string? ipAddress)
    {
        var account = GetAccountByRefreshToken(token);
        var refreshToken = account.RefreshTokens.Single(x => x.Token == token);

        if (refreshToken.IsRevoked)
        {
            // revoke all descendant tokens in case this token has been compromised
            RevokeDescendantRefreshTokens(refreshToken, account, ipAddress, $"Attempted reuse of revoked ancestor token: {token}");
            _context.Update(account);
            _context.SaveChanges();
        }

        if (!refreshToken.IsActive)
            throw new AppException("Token inválido");

        // replace old refresh token with a new one (rotate token)
        var newRefreshToken = RotateRefreshToken(refreshToken, ipAddress);
        account.RefreshTokens.Add(newRefreshToken);

        // remove old refresh tokens from account
        RemoveOldRefreshTokens(account);

        // save changes to db
        _context.Update(account);
        _context.SaveChanges();

        // generate new jwt
        var jwtToken = _jwtUtils.GenerateJwtToken(account);

        // return data in authenticate response object
        var response = _mapper.Map<AuthenticateResponse>(account);
        response.JwtToken = jwtToken;
        response.RefreshToken = newRefreshToken.Token;
        return response;
    }

    public void RevokeToken(string token, string? ipAddress)
    {
        var account = GetAccountByRefreshToken(token);
        var refreshToken = account.RefreshTokens.Single(x => x.Token == token);

        if (!refreshToken.IsActive)
            throw new AppException("Token inválido");

        // revoke token and save
        RevokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
        _context.Update(account);
        _context.SaveChanges();
    }

    public void Register(RegisterRequest model, string? origin)
    {
        // validate
        if (_context.Accounts.Any(x => x.Email == model.Email))
        {
            // send already registered error in email to prevent account enumeration
            SendAlreadyRegisteredEmail(model.Email, origin);
            return;
        }

        // map model to new account object
        var account = _mapper.Map<Account>(model);

        account.Role = Role.User;
        account.Created = DateTime.UtcNow;
        account.VerificationToken = GenerateVerificationToken();

        // hash password
        account.PasswordHash = BCryptNet.HashPassword(model.Password);

        // save account
        _context.Accounts.Add(account);
        _context.SaveChanges();

        // send email
        SendVerificationEmail(account, origin);
    }

    public void VerifyEmail(string token)
    {
        var account = _context.Accounts.SingleOrDefault(x => x.VerificationToken == token);

        if (account == null)
            throw new AppException("Verificação falhou");

        account.Verified = DateTime.UtcNow;
        account.VerificationToken = null;

        _context.Accounts.Update(account);
        _context.SaveChanges();
    }

    public void ForgotPassword(ForgotPasswordRequest model, string? origin)
    {
        var account = _context.Accounts.SingleOrDefault(x => x.Email == model.Email);

        // always return ok response to prevent email enumeration
        if (account == null) return;

        // create reset token that expires after 1 day
        account.ResetToken = GenerateResetToken();
        account.ResetTokenExpires = DateTime.UtcNow.AddDays(1);

        _context.Accounts.Update(account);
        _context.SaveChanges();

        // send email
        SendPasswordResetEmail(account, origin);
    }

    public void ValidateResetToken(ValidateResetTokenRequest model)
    {
        GetAccountByResetToken(model.Token);
    }

    public void ResetPassword(ResetPasswordRequest model)
    {
        var account = GetAccountByResetToken(model.Token);

        // update password and remove reset token
        account.PasswordHash = BCryptNet.HashPassword(model.Password);
        account.PasswordReset = DateTime.UtcNow;
        account.ResetToken = null;
        account.ResetTokenExpires = null;

        _context.Accounts.Update(account);
        _context.SaveChanges();
    }

    public IEnumerable<AccountResponse> GetAll()
    {
        var accounts = _context.Accounts;
        return _mapper.Map<IList<AccountResponse>>(accounts);
    }

    public AccountResponse GetById(int id)
    {
        var account = getAccount(id);
        return _mapper.Map<AccountResponse>(account);
    }

    public AccountResponse Create(CreateRequest model)
    {
        // validate
        if (_context.Accounts.Any(x => x.Email == model.Email))
            throw new AppException($"Email '{model.Email}' is already registered");

        // map model to new account object
        var account = _mapper.Map<Account>(model);
        account.Created = DateTime.UtcNow;
        account.Verified = DateTime.UtcNow;

        // hash password
        account.PasswordHash = BCryptNet.HashPassword(model.Password);

        // save account
        _context.Accounts.Add(account);
        _context.SaveChanges();

        return _mapper.Map<AccountResponse>(account);
    }

    public AccountResponse Update(int id, UpdateRequest model)
    {
        var account = getAccount(id);

        // validate
        if (account.Email != model.Email && _context.Accounts.Any(x => x.Email == model.Email))
            throw new AppException($"Email '{model.Email}' is already registered");

        // hash password if it was entered
        if (!string.IsNullOrEmpty(model.Password))
            account.PasswordHash = BCryptNet.HashPassword(model.Password);

        // copy model to account and save
        _mapper.Map(model, account);
        account.Updated = DateTime.UtcNow;
        _context.Accounts.Update(account);
        _context.SaveChanges();

        return _mapper.Map<AccountResponse>(account);
    }

    public void Delete(int id)
    {
        var account = getAccount(id);
        _context.Accounts.Remove(account);
        _context.SaveChanges();
    }

    // helper methods

    private Account getAccount(int id)
    {
        var account = _context.Accounts.Find(id);
        if (account == null) throw new KeyNotFoundException("Account not found");
        return account;
    }

    private Account GetAccountByRefreshToken(string? token)
    {
        var account = _context.Accounts.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
        if (account == null) throw new AppException("Token inválido");
        return account;
    }

    private Account GetAccountByResetToken(string token)
    {
        var account = _context.Accounts.SingleOrDefault(x =>
            x.ResetToken == token && x.ResetTokenExpires > DateTime.UtcNow);
        if (account == null) throw new AppException("Token inválido");
        return account;
    }

    private string GenerateResetToken()
    {
        // token is a cryptographically strong random sequence of values
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

        // ensure token is unique by checking against db
        var tokenIsUnique = !_context.Accounts.Any(x => x.ResetToken == token);
        if (!tokenIsUnique)
            return GenerateResetToken();

        return token;
    }

    private string GenerateVerificationToken()
    {
        // token is a cryptographically strong random sequence of values
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

        // ensure token is unique by checking against db
        var tokenIsUnique = !_context.Accounts.Any(x => x.VerificationToken == token);
        if (!tokenIsUnique)
            return GenerateVerificationToken();

        return token;
    }

    private RefreshToken RotateRefreshToken(RefreshToken refreshToken, string? ipAddress)
    {
        var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
        RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
        return newRefreshToken;
    }

    private void RemoveOldRefreshTokens(Account account)
    {
        account.RefreshTokens.RemoveAll(x =>
            !x.IsActive &&
            x.Created.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow);
    }

    private void RevokeDescendantRefreshTokens(RefreshToken refreshToken, Account account, string? ipAddress, string reason)
    {
        // recursively traverse the refresh token chain and ensure all descendants are revoked
        if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
        {
            var childToken = account.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
            if(childToken != null)
            {
                if (childToken.IsActive)
                    RevokeRefreshToken(childToken, ipAddress, reason);
                else
                    RevokeDescendantRefreshTokens(childToken, account, ipAddress, reason);
            }
        }
    }

    private void RevokeRefreshToken(RefreshToken token, string? ipAddress, string? reason = null, string? replacedByToken = null)
    {
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = reason;
        token.ReplacedByToken = replacedByToken;
    }

    private void SendVerificationEmail(Account account, string? origin)
    {
        string message;
        if (!string.IsNullOrEmpty(origin))
        {
            // origin exists if request sent from browser single page app (e.g. Angular or React)
            // so send link to verify via single page app
            var verifyUrl = $"{origin}/auth/verify-email?token={account.VerificationToken}";
            message = $@"<p>Please click the below link to verify your email address:</p>
                            <p><a href=""{verifyUrl}"">{verifyUrl}</a></p>";
        }
        else
        {
            // origin missing if request sent directly to api (e.g. from Postman)
            // so send instructions to verify directly with api
            message = $@"<p>Please use the below token to verify your email address with the <code>/api/v1/accounts/verify-email</code> api route:</p>
                            <p><code>{account.VerificationToken}</code></p>";
        }

        _emailService.Send(
            to: account.Email,
            subject: "Sign-up Verification API - Verify Email",
            html: $@"<h4>Verify Email</h4>
                        <p>Thanks for registering!</p>
                        {message}"
        );
    }

    private void SendAlreadyRegisteredEmail(string email, string? origin)
    {
        string message;
        if (!string.IsNullOrEmpty(origin))
            message = $@"<p>Se você não lembra a sua senha, por favor visite a página <a href=""{origin}/auth/forgot-password"">esqueceu a senha?</a>.</p>";
        else
            message = "<p>Se você não lembra a sua senha você pode alterá-la através da api <code>/api/v1/accounts/forgot-password</code>.</p>";

        _emailService.Send(
            to: email,
            subject: "Nova Conta - E-mail Existente",
            html: $@"<h4>Email já utilizado</h4>
                        <p>O seu email <strong>{email}</strong> já foi registrado.</p>
                        {message}"
        );
    }

    private void SendPasswordResetEmail(Account account, string? origin)
    {
        string message;
        string resetUrl = "";
        string html;
        if (!string.IsNullOrEmpty(origin))
        {
            resetUrl = $"{origin}/auth/reset-password?token={account.ResetToken}";
            html = _templateService.GetForgotPassword(account, resetUrl);
        }
        else
        {
            message = $@"<p>Por favor, use o token abaixo para resetar a sua senha com o endpoint <code>/api/v1/accounts/reset-password</code>:</p>
                            <p><code>{account.ResetToken}</code></p>";
            html = $@"<h4>Olá, {account.FirstName} {account.LastName}</h4>
                        {message}";
        }

        _emailService.Send(
            to: account.Email,
            subject: "Defina sua nova senha",
            html: html
        );
    }
}