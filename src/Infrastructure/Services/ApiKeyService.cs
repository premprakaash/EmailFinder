using System.Security.Cryptography;
using System.Text;
using MailForge.Application.Interfaces;
using MailForge.Domain.Entities;
using MailForge.Infrastructure.Persistence;
using MailForge.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace MailForge.Infrastructure.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly MailForgeDbContext _db;

    public ApiKeyService(MailForgeDbContext db) => _db = db;

    public async Task<(string RawKey, Guid Id)> CreateKeyAsync(Guid userId, string name, CancellationToken cancellationToken)
    {
        var rawKey = ApiKeyPrefix.Prefix + Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
        var hash = HashKey(rawKey);
        var prefix = rawKey[..8];

        var apiKey = new ApiKey
        {
            UserId = userId,
            Name = name,
            KeyHash = hash,
            KeyPrefix = prefix
        };

        _db.ApiKeys.Add(apiKey);
        await _db.SaveChangesAsync(cancellationToken);
        return (rawKey, apiKey.Id);
    }

    public async Task RevokeKeyAsync(Guid userId, Guid keyId, CancellationToken cancellationToken)
    {
        var key = await _db.ApiKeys.FirstOrDefaultAsync(k => k.Id == keyId && k.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("API key not found.");
        key.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid?> ValidateKeyAsync(string rawKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawKey) || !rawKey.StartsWith(ApiKeyPrefix.Prefix, StringComparison.Ordinal))
            return null;

        var hash = HashKey(rawKey);
        var key = await _db.ApiKeys.AsNoTracking()
            .FirstOrDefaultAsync(k => k.KeyHash == hash && k.RevokedAt == null, cancellationToken);

        return key?.UserId;
    }

    public async Task RecordUsageAsync(Guid keyId, CancellationToken cancellationToken)
    {
        var key = await _db.ApiKeys.FirstOrDefaultAsync(k => k.Id == keyId, cancellationToken);
        if (key != null)
        {
            key.LastUsedAt = DateTime.UtcNow;
            key.RequestCount++;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private static string HashKey(string rawKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexString(bytes);
    }
}
