using System.ComponentModel.DataAnnotations;
using FinTrack.Models;

namespace WebApp.Models;

public class EditProfileViewModel
{
    [Required]
    [MaxLength(ApplicationUser.MaxFullNameLength)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(ApplicationUser.MaxUserNameLength)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [RegularExpression(ApplicationUser.EmailPattern, ErrorMessage = "Email is not valid")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression(ApplicationUser.PhonePattern, ErrorMessage = "Invalid Ukrainian phone number")]
    public string PhoneNumber { get; set; } = string.Empty;
}