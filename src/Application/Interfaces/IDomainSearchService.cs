using MailForge.Application.DTOs.DomainSearch;

namespace MailForge.Application.Interfaces;

public interface IDomainSearchService
{
    Task<DomainSearchResponse> SearchAsync(Guid userId, DomainSearchRequest request, CancellationToken cancellationToken);
}
