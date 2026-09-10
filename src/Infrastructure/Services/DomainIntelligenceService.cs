using DnsClient;
using MailForge.Application.Interfaces;
using MailForge.Domain.Entities;
using MailForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MailForge.Infrastructure.Services;

public class DomainIntelligenceService : IDomainIntelligenceService
{
    private readonly MailForgeDbContext _db;
    private readonly LookupClient _dns;
    private readonly IMemoryCache _cache;

    public DomainIntelligenceService(MailForgeDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
        _dns = new LookupClient(new LookupClientOptions { Timeout = TimeSpan.FromSeconds(5) });
    }

    public async Task<DomainInfo?> GetDomainInfoAsync(string domain, CancellationToken cancellationToken)
    {
        domain = domain.Trim().ToLowerInvariant();
        var cacheKey = $"domain:{domain}";

        if (_cache.TryGetValue(cacheKey, out DomainInfo? cached))
            return cached;

        var entity = await _db.Domains.AsNoTracking().FirstOrDefaultAsync(d => d.Domain == domain, cancellationToken);
        if (entity?.LastCheckedAt > DateTime.UtcNow.AddHours(-24))
        {
            var info = MapFromEntity(entity);
            _cache.Set(cacheKey, info, TimeSpan.FromHours(1));
            return info;
        }

        return await RefreshDomainAsync(domain, cancellationToken);
    }

    public async Task UpsertDomainInfoAsync(string domain, DomainInfo info, CancellationToken cancellationToken)
    {
        domain = domain.Trim().ToLowerInvariant();
        var entity = await _db.Domains.FirstOrDefaultAsync(d => d.Domain == domain, cancellationToken);
        if (entity == null)
        {
            entity = new DomainEntity { Domain = domain };
            _db.Domains.Add(entity);
        }

        entity.CompanyName = info.CompanyName;
        entity.MxRecords = info.MxRecords;
        entity.MailProvider = info.MailProvider;
        entity.Website = info.Website;
        entity.Industry = info.Industry;
        entity.Country = info.Country;
        entity.EmployeeCount = info.EmployeeCount;
        entity.LastCheckedAt = info.LastCheckedAt;

        await _db.SaveChangesAsync(cancellationToken);
        _cache.Set($"domain:{domain}", info, TimeSpan.FromHours(1));
    }

    private async Task<DomainInfo?> RefreshDomainAsync(string domain, CancellationToken cancellationToken)
    {
        try
        {
            var mxResult = await _dns.QueryAsync(domain, QueryType.MX, cancellationToken: cancellationToken);
            var mxRecords = mxResult.Answers.MxRecords().Select(r => r.Exchange.Value).ToList();
            var mailProvider = DetectMailProvider(mxRecords);

            var info = new DomainInfo(domain, domain, string.Join(";", mxRecords), mailProvider, $"https://{domain}", null, null, null, DateTime.UtcNow);
            await UpsertDomainInfoAsync(domain, info, cancellationToken);
            return info;
        }
        catch
        {
            return null;
        }
    }

    private static string? DetectMailProvider(List<string> mxRecords)
    {
        var mx = string.Join(" ", mxRecords).ToLowerInvariant();
        if (mx.Contains("google")) return "Google Workspace";
        if (mx.Contains("outlook") || mx.Contains("microsoft")) return "Microsoft 365";
        if (mx.Contains("yahoo")) return "Yahoo";
        if (mx.Contains("zoho")) return "Zoho";
        return mxRecords.FirstOrDefault();
    }

    private static DomainInfo MapFromEntity(DomainEntity entity) => new(
        entity.Domain, entity.CompanyName, entity.MxRecords, entity.MailProvider,
        entity.Website, entity.Industry, entity.Country, entity.EmployeeCount,
        entity.LastCheckedAt ?? DateTime.UtcNow);
}
