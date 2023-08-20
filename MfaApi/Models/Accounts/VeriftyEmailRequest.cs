using System.ComponentModel.DataAnnotations;

namespace MfaApi.Models.Accounts;

public class VerifyEmailRequest
{
    [Required]
    public string Token { get; set; }
}
