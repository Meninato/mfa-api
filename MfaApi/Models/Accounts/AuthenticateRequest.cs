﻿using System.ComponentModel.DataAnnotations;

namespace MfaApi.Models.Accounts;

public class AuthenticateRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;
}