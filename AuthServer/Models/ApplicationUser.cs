// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public const int MaxFullNameLength = 500;
    public const int MaxUserNameLength = 50;
    public const string PhonePattern = @"^\+380\d{9}$";
    public const string EmailPattern = @"^(?:[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}|(?:\[(?:[0-9]{1,3}\.){3}[0-9]{1,3}\]))$";


    [Required]
    [MaxLength(MaxFullNameLength)]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    [Required]
    [RegularExpression(PhonePattern, ErrorMessage = "Invalid Ukrainian phone number")]
    public override string? PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(MaxUserNameLength)]
    public override string? UserName { get => base.UserName; set => base.UserName = value; }

    [Required]
    [RegularExpression(EmailPattern, ErrorMessage = "Email is not valid according to RFC 822")]
    public override string? Email { get; set; } = string.Empty;
}
