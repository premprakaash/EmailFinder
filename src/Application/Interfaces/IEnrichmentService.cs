using MailForge.Application.DTOs.Enrichment;

namespace MailForge.Application.Interfaces;

public interface IEnrichmentService
{
    Task<EnrichmentResponse?> EnrichAsync(Guid userId, EnrichmentRequest request, CancellationToken cancellationToken);
}

public interface IPeopleDiscoveryService
{
    Task<PeopleDiscoveryResponse> DiscoverAsync(Guid userId, PeopleDiscoveryRequest request, CancellationToken cancellationToken);
}
