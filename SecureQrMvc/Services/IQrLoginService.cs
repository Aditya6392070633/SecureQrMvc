using SecureQrMvc.Models;

namespace SecureQrMvc.Services;

public interface IQrLoginService
{
    Task<(QrLoginSession Session, string RawToken, string QrBase64)> CreateAsync(HttpRequest request);
    Task<bool> ApproveAsync(string sessionId, string rawToken, AppUser user);
    Task<QrLoginSession?> ConsumeIfApprovedAsync(string sessionId);
}
