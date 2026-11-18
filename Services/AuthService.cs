using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using SmartStock.Data;
using SmartStock.Models;

namespace SmartStock.Services;

public class AuthService(
    IDbContextFactory<SmartStockDbContext> factory,
    ProtectedLocalStorage storage)
{
    private const string StorageKey = "smartstock_auth_user_id";
    public const string DemoEmail = "admin@smartstock.com";
    public const string DemoPassword = "SmartStock123!";

    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null;

    public event Action? OnAuthStateChanged;

    public static async Task EnsureSeedDataAsync(IDbContextFactory<SmartStockDbContext> dbFactory)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        // Ensure the Users table exists even if smartstock.db was created before User was added
        await db.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""Users"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Users"" PRIMARY KEY AUTOINCREMENT,
                ""FullName"" TEXT NOT NULL,
                ""Email"" TEXT NOT NULL,
                ""PasswordHash"" TEXT NOT NULL,
                ""Role"" TEXT NOT NULL DEFAULT 'Manager',
                ""CreatedAt"" TEXT NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Users_Email"" ON ""Users"" (""Email"");
        ");

        var demoEmailNormalized = DemoEmail.ToLowerInvariant();
        if (!await db.Users.AnyAsync(u => u.Email.ToLower() == demoEmailNormalized))
        {
            db.Users.Add(new User
            {
                FullName = "SmartStock Admin",
                Email = DemoEmail,
                PasswordHash = HashPassword(DemoPassword),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }

    public async Task RestoreSessionAsync()
    {
        if (CurrentUser is not null)
            return;

        try
        {
            var result = await storage.GetAsync<int>(StorageKey);
            if (result.Success && result.Value > 0)
            {
                await using var db = await factory.CreateDbContextAsync();
                var user = await db.Users.FindAsync(result.Value);
                if (user is not null)
                {
                    CurrentUser = user;
                    NotifyStateChanged();
                }
            }
        }
        catch
        {
            // Ignore storage errors during static prerendering
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "Please enter both email and password.");

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var passwordHash = HashPassword(password);

        await using var db = await factory.CreateDbContextAsync();
        var user = await db.Users.FirstOrDefaultAsync(
            u => u.Email.ToLower() == normalizedEmail && u.PasswordHash == passwordHash);

        if (user is null)
            return (false, "Invalid email or password. Try the demo account or register a new account.");

        CurrentUser = user;
        await SaveSessionAsync(user.Id);
        NotifyStateChanged();
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(
        string fullName,
        string email,
        string password,
        string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return (false, "Please enter your full name.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return (false, "Please enter a valid email address.");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (false, "Password must be at least 6 characters long.");

        if (password != confirmPassword)
            return (false, "Passwords do not match.");

        var normalizedEmail = email.Trim().ToLowerInvariant();

        await using var db = await factory.CreateDbContextAsync();
        if (await db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
            return (false, "An account with this email already exists. Please sign in instead.");

        var user = new User
        {
            FullName = fullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = HashPassword(password),
            Role = "Manager",
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        CurrentUser = user;
        await SaveSessionAsync(user.Id);
        NotifyStateChanged();
        return (true, null);
    }

    public async Task LogoutAsync()
    {
        CurrentUser = null;
        try
        {
            await storage.DeleteAsync(StorageKey);
        }
        catch
        {
            // Ignore storage errors if disconnected
        }
        NotifyStateChanged();
    }

    private async Task SaveSessionAsync(int userId)
    {
        try
        {
            await storage.SetAsync(StorageKey, userId);
        }
        catch
        {
            // Ignore if JS interop is unavailable
        }
    }

    private void NotifyStateChanged() => OnAuthStateChanged?.Invoke();

    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"SmartStockSalt::{password}"));
        return Convert.ToHexString(bytes);
    }
}
