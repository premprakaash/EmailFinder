using MailForge.Application.DTOs.Enrichment;
using MailForge.Application.Interfaces;
using MailForge.Application.Interfaces.Providers;
using MailForge.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MailForge.Infrastructure.Services;

public class EnrichmentService : IEnrichmentService
{
    private readonly IEnumerable<IEnrichmentProvider> _providers;
    private readonly ICreditService _creditService;
    private readonly ILogger<EnrichmentService> _logger;

    public EnrichmentService(
        IEnumerable<IEnrichmentProvider> providers,
        ICreditService creditService,
        ILogger<EnrichmentService> logger)
    {
        _providers = providers.OrderBy(p => p.Priority);
        _creditService = creditService;
        _logger = logger;
    }

    public async Task<EnrichmentResponse?> EnrichAsync(Guid userId, EnrichmentRequest request, CancellationToken cancellationToken)
    {
        if (!await _creditService.ConsumeCreditsAsync(userId, CreditOperation.Enrichment, "Email enrichment", cancellationToken: cancellationToken))
            throw new InvalidOperationException("Insufficient credits.");

        var email = request.Email.Trim().ToLowerInvariant();

        foreach (var provider in _providers)
        {
            try
            {
                var result = await provider.EnrichAsync(email, cancellationToken);
                if (result != null)
                {
                    return new EnrichmentResponse(
                        result.Email, result.FirstName, result.LastName, result.JobTitle,
                        result.Company, result.Domain, result.Country, result.Industry, result.ProviderName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Enrichment provider {Provider} failed", provider.Name);
            }
        }

        return null;
    }
}
