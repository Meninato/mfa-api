using System.ComponentModel.DataAnnotations;

namespace MfaApi.Models.Accounts;

public class AuthenticateRequest
{
    [Required(ErrorMessage = "Este campo é necessário")]
    [EmailAddress(ErrorMessage = "O formato do e-mail não parece estar correto")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Este campo é necessário")]
    [StringLength(32, MinimumLength = 8, ErrorMessage = "A senha deve ter no mínimo 8 e máximo 32 caracteres.")]
    public string Password { get; set; } = default!;
}