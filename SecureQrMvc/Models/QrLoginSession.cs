using System.ComponentModel.DataAnnotations;

namespace SecureQrMvc.Models;

public class QrLoginSession
{
    public int Id { get; set; }
    [MaxLength(64)] public string SessionId { get; set; } = string.Empty;
    [MaxLength(128)] public string TokenHash { get; set; } = string.Empty;
    [MaxLength(45)] public string? RequestedIp { get; set; }
    [MaxLength(260)] public string? UserAgent { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public int? ApprovedUserId { get; set; }
    public bool Consumed { get; set; }
}
