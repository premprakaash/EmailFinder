using MailForge.Application.DTOs.Finder;

namespace MailForge.Application.Interfaces.Providers;

public record ProviderFinderRequest(string FirstName, string LastName, string Domain, string? Company = null, string? JobTitle = null);

public record ProviderFinderResult(
    string Email,
    int ConfidenceScore,
    string Pattern,
    string ProviderName,
    bool IsVerified = false);

public interface IEmailFinderProvider
{
    string Name { get; }
    int Priority { get; }
    Task<IReadOnlyList<ProviderFinderResult>> FindAsync(ProviderFinderRequest request, CancellationToken cancellationToken);
}
