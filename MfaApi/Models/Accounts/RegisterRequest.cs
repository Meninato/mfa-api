using System.ComponentModel.DataAnnotations;

namespace MfaApi.Models.Accounts;

public class RegisterRequest
{

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = default!;

    [Required]
    [MaxLength(254)]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = default!;

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = default!;

    [Range(typeof(bool), "true", "true")]
    public bool AcceptTerms { get; set; }
}