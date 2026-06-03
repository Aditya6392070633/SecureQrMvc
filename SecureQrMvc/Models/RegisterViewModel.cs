using System.ComponentModel.DataAnnotations;

namespace SecureQrMvc.Models;

public class RegisterViewModel
{
    [Required, MaxLength(80)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(120)] public string Email { get; set; } = string.Empty;
    [Required, MinLength(8), DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Required, Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = string.Empty;
}
