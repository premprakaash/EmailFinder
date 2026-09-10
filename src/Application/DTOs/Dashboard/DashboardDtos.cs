using MailForge.Domain.Enums;

namespace MailForge.Application.DTOs.Dashboard;

public record DashboardStatsDto(
    long CreditBalance,
    long CreditsConsumed,
    int ContactsDiscovered,
    int EmailsVerified,
    int ValidEmails,
    int InvalidEmails,
    int RiskyEmails,
    int ExportsUsed,
    long ApiRequests,
    string CurrentPlan);

public record RecentSearchDto(Guid Id, string Domain, JobStatus Status, DateTime CreatedAt);
public record RecentVerificationDto(Guid Id, string Email, EmailStatus? Status, DateTime CreatedAt);

public record DashboardResponse(
    DashboardStatsDto Stats,
    IReadOnlyList<RecentSearchDto> RecentSearches,
    IReadOnlyList<RecentVerificationDto> RecentVerifications);
