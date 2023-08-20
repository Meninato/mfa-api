using System.ComponentModel.DataAnnotations;

namespace MfaApi.Models.Accounts;

public class ValidateResetTokenRequest
{
    [Required]
    public string Token { get; set; }
}
