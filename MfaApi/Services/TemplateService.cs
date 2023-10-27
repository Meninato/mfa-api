using MfaApi.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Scriban;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace MfaApi.Services;

public interface ITemplateService
{
    Task<string> GetForgotPasswordAsync(Account account, string url);
    string GetForgotPassword(Account account, string url);
}

public class TemplateService : ITemplateService
{
    private readonly string _forgotPasswordTemplate = "forgot-password.html";
    private readonly IWebHostEnvironment _webHostEnvironment;

    public TemplateService(IWebHostEnvironment env)
    {
        _webHostEnvironment = env;
    }

    public async Task<string> GetForgotPasswordAsync(Account account, string url)
    {
        string templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Templates", _forgotPasswordTemplate);
        using StreamReader reader = new StreamReader(templatePath);
        string html = reader.ReadToEnd();
        var template = Template.Parse(html);

        return await template.RenderAsync(new { Name = $"{account.FirstName} {account.LastName}", Email = account.Email, Url = url });
    }

    public string GetForgotPassword(Account account, string url)
    {
        string templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Templates", _forgotPasswordTemplate);
        using StreamReader reader = new StreamReader(templatePath);
        string html = reader.ReadToEnd();
        var template = Template.Parse(html);

        return template.Render(new { Name = $"{account.FirstName} {account.LastName}", Email = account.Email, Url = url });
    }
}
