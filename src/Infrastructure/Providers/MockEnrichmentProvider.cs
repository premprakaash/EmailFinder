using MailForge.Application.Interfaces;
using MailForge.Application.Interfaces.Providers;

namespace MailForge.Infrastructure.Providers;

/// <summary>
/// Development enrichment provider. Maps known demo emails to contact profiles.
/// Replace with lawful production enrichment providers.
/// </summary>
public class MockEnrichmentProvider : IEnrichmentProvider
{
    public string Name => ProviderName;
    public int Priority => 100;

    private const string ProviderName = "MockDev";

    private static readonly Dictionary<string, EnrichmentResult> Profiles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["john.smith@example.com"] = new("john.smith@example.com", "John", "Smith", "Senior Software Engineer", "Example Corp", "example.com", "US", "Technology", ProviderName),
        ["jane.doe@example.com"] = new("jane.doe@example.com", "Jane", "Doe", "Product Manager", "Example Corp", "example.com", "US", "Technology", ProviderName),
        ["alex.johnson@example.com"] = new("alex.johnson@example.com", "Alex", "Johnson", "DevOps Engineer", "Example Corp", "example.com", "US", "Technology", ProviderName),
        ["david.wilson@company.com"] = new("david.wilson@company.com", "David", "Wilson", "CTO", "Company Inc", "company.com", "US", "Technology", ProviderName),
        ["info@example.com"] = new("info@example.com", null, null, null, "Example Corp", "example.com", "US", "Technology", ProviderName),
    };

    public Task<EnrichmentResult?> EnrichAsync(string email, CancellationToken cancellationToken)
    {
        email = email.Trim().ToLowerInvariant();
        if (Profiles.TryGetValue(email, out var profile))
            return Task.FromResult<EnrichmentResult?>(profile);

        if (email.EndsWith("@example.com", StringComparison.OrdinalIgnoreCase))
        {
            var local = email.Split('@')[0];
            var parts = local.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                return Task.FromResult<EnrichmentResult?>(new EnrichmentResult(
                    email, Capitalize(parts[0]), Capitalize(parts[1]), "Team Member", "Example Corp", "example.com", "US", "Technology", ProviderName));
            }
        }

        return Task.FromResult<EnrichmentResult?>(null);
    }

    private static string Capitalize(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToUpperInvariant(value[0]) + value[1..];
}
