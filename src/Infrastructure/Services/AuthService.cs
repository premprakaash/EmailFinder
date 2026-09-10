using MailForge.Application.DTOs.Auth;
using MailForge.Application.Interfaces;
using MailForge.Domain.Entities;
using MailForge.Domain.Enums;
using MailForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MailForge.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly MailForgeDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(MailForgeDbContext db, ITokenService tokenService, ILogger<AuthService> logger)
    {
        _db = db;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email, cancellationToken))
            throw new InvalidOperationException("Email already registered.");

        var defaultPlan = await _db.Plans.FirstOrDefaultAsync(p => p.IsDefault, cancellationToken)
            ?? throw new InvalidOperationException("No default plan configured.");

        var user = new User
        {
            Email = email,
            PasswordHash = PasswordHasher.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = UserRole.User,
            EmailVerificationToken = Guid.NewGuid().ToString("N"),
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddDays(1)
        };

        user.CreditBalance = new CreditBalance { UserId = user.Id, Balance = defaultPlan.MonthlyCredits, TotalPurchased = defaultPlan.MonthlyCredits };
        user.Subscription = new Subscription { UserId = user.Id, PlanId = defaultPlan.Id, StartsAt = DateTime.UtcNow, IsActive = true };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User registered: {Email}", email);
        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.Include(u => u.CreditBalance)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (user.IsSuspended) throw new UnauthorizedAccessException("Account suspended.");
        if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = _tokenService.HashToken(refreshToken);
        var stored = await _db.RefreshTokens.Include(r => r.User).ThenInclude(u => u.CreditBalance)
            .FirstOrDefaultAsync(r => r.TokenHash == hash && r.RevokedAt == null, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (stored.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired.");

        stored.RevokedAt = DateTime.UtcNow;
        return await CreateAuthResponseAsync(stored.User, cancellationToken, stored.TokenHash);
    }

    public async Task LogoutAsync(Guid userId, string refreshToken, CancellationToken cancellationToken)
    {
        var hash = _tokenService.HashToken(refreshToken);
        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.UserId == userId && r.TokenHash == hash, cancellationToken);
        if (stored != null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLowerInvariant(), cancellationToken);
        if (user == null) return;

        user.PasswordResetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Password reset token generated for {Email}", user.Email);
    }

    public async Task ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token, cancellationToken)
            ?? throw new InvalidOperationException("Invalid reset token.");

        if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("Reset token expired.");

        user.PasswordHash = PasswordHasher.Hash(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task VerifyEmailAsync(string token, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token, cancellationToken)
            ?? throw new InvalidOperationException("Invalid verification token.");

        if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("Verification token expired.");

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(User user, CancellationToken cancellationToken, string? replacedTokenHash = null)
    {
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshHash = _tokenService.HashToken(refreshToken);

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            ReplacedByTokenHash = replacedTokenHash
        });
        await _db.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var balance = user.CreditBalance?.Balance ?? 0;

        return new AuthResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(15),
            new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Role.ToString(), user.IsEmailVerified, balance));
    }
}
