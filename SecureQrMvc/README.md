# SecureQrMvc - ASP.NET Core MVC QR Login Website

A ready-to-run ASP.NET Core MVC project with secure QR login, password login, modern UI, EF Core SQLite database, SignalR live login approval, CSRF, CSP, rate limiting, account lockout and PBKDF2 password hashing.

## Demo Login

Email: `admin@secureqr.local`  
Password: `Admin@12345`

## Run Steps

```bash
cd SecureQrMvc
dotnet restore
dotnet run
```

Open:

```text
https://localhost:7107
```

## QR Login Flow

1. Open `/Account/QrLogin` on desktop.
2. Scan the QR using your phone camera.
3. Enter the demo credentials on phone.
4. Desktop browser logs in automatically.

For local testing with a real phone, both phone and computer should be on the same network. You may need to run with your LAN URL instead of localhost.

## Security Features

- QR token is random and one-time use.
- Only SHA-256 token hash is stored in DB.
- QR session expires in 3 minutes.
- SignalR sends live approval event.
- PBKDF2 password hashing with salt.
- Cookie auth with HttpOnly, SameSite and Secure policy.
- CSRF validation enabled globally.
- Rate limiting on login endpoints.
- Lockout after failed password attempts.
- CSP and browser security headers.

## Change to SQL Server

Install SQL Server provider if needed:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

In `Program.cs`, replace `UseSqlite(...)` with `UseSqlServer(...)` and update appsettings connection string.
