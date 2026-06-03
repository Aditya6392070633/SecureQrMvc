using Microsoft.EntityFrameworkCore;
using QRCoder;
using SecureQrMvc.Data;
using SecureQrMvc.Models;
using System.Security.Cryptography;

namespace SecureQrMvc.Services;

public class QrLoginService : IQrLoginService
{
    private readonly AppDbContext _db;
    private readonly IUserSecurityService _security;
    private readonly IConfiguration _config;
    private readonly LinkGenerator _links;
    private readonly IHttpContextAccessor _accessor;

    public QrLoginService(AppDbContext db, IUserSecurityService security, IConfiguration config, LinkGenerator links, IHttpContextAccessor accessor)
    {
        _db = db;
        _security = security;
        _config = config;
        _links = links;
        _accessor = accessor;
    }

    public async Task<(QrLoginSession Session, string RawToken, string QrBase64)> CreateAsync(HttpRequest request)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        var session = new QrLoginSession
        {
            SessionId = Guid.NewGuid().ToString("N"),
            TokenHash = _security.Sha256(rawToken),
            RequestedIp = request.HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = request.Headers.UserAgent.ToString()[..Math.Min(request.Headers.UserAgent.ToString().Length, 260)],
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(_config.GetValue("Security:QrSessionMinutes", 3))
        };
        _db.QrLoginSessions.Add(session);
        await _db.SaveChangesAsync();

        var scanUrl = _links.GetUriByAction(_accessor.HttpContext!, "Scan", "Account", new { sessionId = session.SessionId, token = rawToken })
                      ?? $"/Account/Scan?sessionId={session.SessionId}&token={Uri.EscapeDataString(rawToken)}";
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(scanUrl, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data).GetGraphic(12);
        return (session, rawToken, Convert.ToBase64String(png));
    }

    public async Task<bool> ApproveAsync(string sessionId, string rawToken, AppUser user)
    {
        var hash = _security.Sha256(rawToken);
        var session = await _db.QrLoginSessions.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.TokenHash == hash);
        if (session == null || session.Consumed || session.ApprovedAtUtc != null || session.ExpiresAtUtc < DateTime.UtcNow)
            return false;

        session.ApprovedUserId = user.Id;
        session.ApprovedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<QrLoginSession?> ConsumeIfApprovedAsync(string sessionId)
    {
        var session = await _db.QrLoginSessions.FirstOrDefaultAsync(x => x.SessionId == sessionId);
        if (session == null || session.Consumed || session.ApprovedAtUtc == null || session.ApprovedUserId == null || session.ExpiresAtUtc < DateTime.UtcNow)
            return null;

        session.Consumed = true;
        await _db.SaveChangesAsync();
        return session;
    }
}
