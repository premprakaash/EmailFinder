using MailForge.Application.DTOs.DomainSearch;
using MailForge.Application.Interfaces.Providers;
using MailForge.Domain.Enums;

namespace MailForge.Infrastructure.Providers;

/// <summary>
/// Development provider returning sample contacts for permitted demo domains.
/// Replace with real lawful data providers in production.
/// </summary>
public class MockDomainSearchProvider : IDomainSearchProvider
{
    public string Name => "MockDev";
    public int Priority => 200;

    private static readonly Dictionary<string, DomainContactDto[]> DemoData = new(StringComparer.OrdinalIgnoreCase)
    {
        ["example.com"] =
        [
            new("John", "Smith", "Senior Software Engineer", "Example Corp", "example.com", "john.smith@example.com", EmailStatus.Valid, 96, "Public Directory", DateTime.UtcNow.AddDays(-1)),
            new("Jane", "Doe", "Product Manager", "Example Corp", "example.com", "jane.doe@example.com", EmailStatus.Valid, 94, "Public Directory", DateTime.UtcNow.AddDays(-2)),
            new("Alex", "Johnson", "DevOps Engineer", "Example Corp", "example.com", "alex.johnson@example.com", EmailStatus.Risky, 72, "Public Directory", DateTime.UtcNow.AddDays(-3))
        ],
        ["company.com"] =
        [
            new("David", "Wilson", "CTO", "Company Inc", "company.com", "david.wilson@company.com", EmailStatus.Valid, 91, "Public Directory", DateTime.UtcNow.AddDays(-1))
        ],
        ["startup.io"] =
        [
            new("Sarah", "Johnson", "Head of Growth", "Startup IO", "startup.io", "sarah.johnson@startup.io", EmailStatus.Valid, 88, "Public Directory", DateTime.UtcNow.AddDays(-1)),
            new("Michael", "Chen", "Founder & CEO", "Startup IO", "startup.io", "michael.chen@startup.io", EmailStatus.Valid, 95, "Public Directory", DateTime.UtcNow.AddDays(-2)),
            new("Emily", "Brown", "Marketing Director", "Startup IO", "startup.io", "emily.brown@startup.io", EmailStatus.Risky, 70, "Public Directory", DateTime.UtcNow.AddDays(-3))
        ]
    };

    public Task<IReadOnlyList<DomainContactDto>> SearchAsync(string domain, CancellationToken cancellationToken)
    {
        if (DemoData.TryGetValue(domain, out var contacts))
            return Task.FromResult<IReadOnlyList<DomainContactDto>>(contacts);

        return Task.FromResult<IReadOnlyList<DomainContactDto>>([]);
    }
}
