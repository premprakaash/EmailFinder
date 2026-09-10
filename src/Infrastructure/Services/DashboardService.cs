using MailForge.Application.DTOs.Dashboard;
using MailForge.Application.Interfaces;
using MailForge.Domain.Enums;
using MailForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MailForge.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly MailForgeDbContext _db;
    private readonly ICreditService _creditService;

    public DashboardService(MailForgeDbContext db, ICreditService creditService)
    {
        _db = db;
        _creditService = creditService;
    }

    public async Task<DashboardResponse> GetDashboardAsync(Guid userId, CancellationToken cancellationToken)
    {
        var balance = await _creditService.GetBalanceAsync(userId, cancellationToken);
        var creditBalance = await _db.CreditBalances.AsNoTracking().FirstOrDefaultAsync(b => b.UserId == userId, cancellationToken);
        var subscription = await _db.Subscriptions.AsNoTracking().Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive, cancellationToken);

        var contacts = await _db.Contacts.AsNoTracking().Where(c => c.UserId == userId).ToListAsync(cancellationToken);
        var verifications = await _db.VerificationResults.AsNoTracking().Where(v => v.UserId == userId).ToListAsync(cancellationToken);
        var exports = await _db.Exports.AsNoTracking().Where(e => e.UserId == userId).CountAsync(cancellationToken);
        var apiRequests = await _db.UsageRecords.AsNoTracking().Where(u => u.UserId == userId).LongCountAsync(cancellationToken);

        var stats = new DashboardStatsDto(
            balance,
            creditBalance?.TotalConsumed ?? 0,
            contacts.Count,
            verifications.Count,
            verifications.Count(v => v.Status == EmailStatus.Valid),
            verifications.Count(v => v.Status == EmailStatus.Invalid),
            verifications.Count(v => v.Status == EmailStatus.Risky),
            exports,
            apiRequests,
            subscription?.Plan.Name ?? "Free");

        var recentSearches = await _db.SearchJobs.AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Take(5)
            .Select(s => new RecentSearchDto(s.Id, s.Domain, s.Status, s.CreatedAt))
            .ToListAsync(cancellationToken);

        var recentVerifications = await _db.VerificationJobs.AsNoTracking()
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .Take(5)
            .Select(v => new RecentVerificationDto(v.Id, v.Email, v.ResultStatus, v.CreatedAt))
            .ToListAsync(cancellationToken);

        return new DashboardResponse(stats, recentSearches, recentVerifications);
    }
}
