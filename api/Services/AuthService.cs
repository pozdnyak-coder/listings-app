using ListingsApi.Auth;
using ListingsApi.Contracts;
using ListingsApi.Data;
using ListingsApi.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ListingsApi.Services;

public class AuthService(AppDbContext db, IPasswordHasher<User> hasher, TokenService tokens,
                         ILogger<AuthService> logger)
{
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();

    public async Task<AuthResponse> RegisterAsync(RegisterRequest r)
    {
        var user = new User { Email = Normalize(r.Email), CreatedAt = DateTime.UtcNow };
        user.PasswordHash = hasher.HashPassword(user, r.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        logger.LogInformation("Зарегистрирован пользователь {UserId}", user.Id);   // не email: это персональные данные
        return Issue(user);                         // сразу вошёл
    }

    // null = неверный email или пароль (не говорим, что именно)
    public async Task<AuthResponse?> LoginAsync(LoginRequest r)
    {
        var email = Normalize(r.Email);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, r.Password)
                            == PasswordVerificationResult.Failed)
        {
            logger.LogWarning("Неудачная попытка входа");
            return null;
        }
        return Issue(user);
    }

    private AuthResponse Issue(User user)
    {
        var (token, expires) = tokens.Create(user);
        return new AuthResponse(token, expires, user.Id, user.Email);
    }
}
