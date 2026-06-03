using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SecureQrMvc.Data;
using SecureQrMvc.Hubs;
using SecureQrMvc.Models;
using SecureQrMvc.Services;
using System.Security.Claims;

namespace SecureQrMvc.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly IUserSecurityService _security;
    private readonly IQrLoginService _qr;
    private readonly IHubContext<QrLoginHub> _hub;
    private readonly IConfiguration _config;

    public AccountController(AppDbContext db, IUserSecurityService security, IQrLoginService qr, IHubContext<QrLoginHub> hub, IConfiguration config)
    {
        _db = db;
        _security = security;
        _qr = qr;
        _hub = hub;
        _config = config;
    }

    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost, EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == model.Email.ToLower());
        if (user == null || IsLocked(user) || !_security.VerifyPassword(user, model.Password))
        {
            if (user != null) await RegisterFailedAttempt(user);
            ModelState.AddModelError(string.Empty, "Invalid email/password or account locked.");
            return View(model);
        }
        await ResetLockoutAndSignIn(user, model.RememberMe);
        return RedirectToAction("Index", "Dashboard");
    }

    public async Task<IActionResult> QrLogin()
    {
        var (session, _, qrBase64) = await _qr.CreateAsync(Request);
        return View(new QrLoginStartViewModel { SessionId = session.SessionId, QrImageBase64 = qrBase64, ExpiresAtUtc = session.ExpiresAtUtc });
    }

    [HttpPost]
    public async Task<IActionResult> QrStatus(string sessionId)
    {
        var session = await _qr.ConsumeIfApprovedAsync(sessionId);
        if (session == null) return Json(new { ok = false });
        var user = await _db.Users.FindAsync(session.ApprovedUserId);
        if (user == null) return Json(new { ok = false });
        await ResetLockoutAndSignIn(user, false);
        return Json(new { ok = true, redirect = Url.Action("Index", "Dashboard") });
    }

    [HttpGet]
    public IActionResult Scan(string sessionId, string token)
    {
        return View(new ScanApproveViewModel { SessionId = sessionId, Token = token });
    }

    [HttpPost, EnableRateLimiting("login")]
    public async Task<IActionResult> Scan(ScanApproveViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == model.Email.ToLower());
        if (user == null || IsLocked(user) || !_security.VerifyPassword(user, model.Password))
        {
            if (user != null) await RegisterFailedAttempt(user);
            ModelState.AddModelError(string.Empty, "Invalid approval credentials.");
            return View(model);
        }
        var approved = await _qr.ApproveAsync(model.SessionId, model.Token, user);
        if (!approved)
        {
            ModelState.AddModelError(string.Empty, "QR session expired or already used.");
            return View(model);
        }
        await _hub.Clients.Group(model.SessionId).SendAsync("QrApproved");
        return View("ScanSuccess");
    }

    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, EnableRateLimiting("login")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var email = model.Email.ToLower();
        if (await _db.Users.AnyAsync(x => x.Email == email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email already registered.");
            return View(model);
        }
        var user = new AppUser { FullName = model.FullName, Email = email };
        _security.SetPassword(user, model.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        await ResetLockoutAndSignIn(user, false);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();

    private bool IsLocked(AppUser user) => user.LockoutEndUtc.HasValue && user.LockoutEndUtc > DateTime.UtcNow;

    private async Task RegisterFailedAttempt(AppUser user)
    {
        user.FailedLoginAttempts++;
        if (user.FailedLoginAttempts >= _config.GetValue("Security:MaxFailedLoginAttempts", 5))
            user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(_config.GetValue("Security:LockoutMinutes", 10));
        await _db.SaveChangesAsync();
    }

    private async Task ResetLockoutAndSignIn(AppUser user, bool rememberMe)
    {
        user.FailedLoginAttempts = 0;
        user.LockoutEndUtc = null;
        user.LastLoginUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = rememberMe });
    }
}
