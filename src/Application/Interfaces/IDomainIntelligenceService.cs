namespace MailForge.Application.Interfaces;

public interface IDomainIntelligenceService
{
    Task<DomainInfo?> GetDomainInfoAsync(string domain, CancellationToken cancellationToken);
    Task UpsertDomainInfoAsync(string domain, DomainInfo info, CancellationToken cancellationToken);
}

public record DomainInfo(
    string Domain,
    string? CompanyName,
    string? MxRecords,
    string? MailProvider,
    string? Website,
    string? Industry,
    string? Country,
    int? EmployeeCount,
    DateTime LastCheckedAt);
