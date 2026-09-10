using MailForge.Application.DTOs.Enrichment;
using MailForge.Application.Interfaces;
using MailForge.Application.DTOs.DomainSearch;

namespace MailForge.Infrastructure.Services;

public class PeopleDiscoveryService : IPeopleDiscoveryService
{
    private readonly IDomainSearchService _domainSearchService;

    public PeopleDiscoveryService(IDomainSearchService domainSearchService) =>
        _domainSearchService = domainSearchService;

    public async Task<PeopleDiscoveryResponse> DiscoverAsync(Guid userId, PeopleDiscoveryRequest request, CancellationToken cancellationToken)
    {
        var result = await _domainSearchService.SearchAsync(userId,
            new DomainSearchRequest(request.Domain, request.Company, request.JobTitle), cancellationToken);

        return new PeopleDiscoveryResponse(
            string.IsNullOrWhiteSpace(request.Domain) ? "company" : "domain",
            result.Company,
            result.Domain,
            result.Contacts,
            result.ResolvedFrom);
    }
}
