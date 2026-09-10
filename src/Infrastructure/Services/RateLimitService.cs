using MailForge.Application.Interfaces;
using MailForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace MailForge.Infrastructure.Services;

public class RateLimitService : IRateLimitService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly MailForgeDbContext _db;

    public RateLimitService(IConnectionMultiplexer redis, MailForgeDbContext db)
    {
        _redis = redis;
        _db = db;
    }

    public async Task<bool> IsAllowedAsync(string key, string scope, string? endpoint = null, CancellationToken cancellationToken = default)
    {
        var config = await _db.RateLimitConfigs.AsNoTracking()
            .Where(r => r.IsActive && r.Scope == scope)
            .Where(r => r.Endpoint == null || r.Endpoint == endpoint)
            .OrderByDescending(r => r.Endpoint != null)
            .FirstOrDefaultAsync(cancellationToken);

        var perMinute = config?.RequestsPerMinute ?? 600;
        var perDay = config?.RequestsPerDay ?? 10000;

        var db = _redis.GetDatabase();
        var minuteKey = $"rl:{scope}:{key}:{endpoint ?? "all"}:m:{DateTime.UtcNow:yyyyMMddHHmm}";
        var dayKey = $"rl:{scope}:{key}:{endpoint ?? "all"}:d:{DateTime.UtcNow:yyyyMMdd}";

        var minuteCount = await db.StringIncrementAsync(minuteKey);
        if (minuteCount == 1) await db.KeyExpireAsync(minuteKey, TimeSpan.FromMinutes(2));

        if (minuteCount > perMinute) return false;

        var dayCount = await db.StringIncrementAsync(dayKey);
        if (dayCount == 1) await db.KeyExpireAsync(dayKey, TimeSpan.FromDays(2));

        return dayCount <= perDay;
    }
}
