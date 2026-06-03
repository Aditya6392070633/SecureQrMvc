namespace SecureQrMvc.Models;

public class QrLoginStartViewModel
{
    public string SessionId { get; set; } = string.Empty;
    public string QrImageBase64 { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
