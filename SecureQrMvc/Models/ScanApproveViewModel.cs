using System.ComponentModel.DataAnnotations;

namespace SecureQrMvc.Models;

public class ScanApproveViewModel
{
    public string SessionId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
}
