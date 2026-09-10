namespace MailForge.Application.Interfaces.Providers;

public record EnrichmentResult(
    string Email,
    string? FirstName,
    string? LastName,
    string? JobTitle,
    string? Company,
    string? Domain,
    string? Country,
    string? Industry,
    string ProviderName);

public interface IEnrichmentProvider
{
    string Name { get; }
    int Priority { get; }
    Task<EnrichmentResult?> EnrichAsync(string email, CancellationToken cancellationToken);
}
