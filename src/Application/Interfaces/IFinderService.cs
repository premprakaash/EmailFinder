using MailForge.Application.DTOs.Finder;

namespace MailForge.Application.Interfaces;

public interface IFinderService
{
    Task<FinderResponse> FindAsync(Guid userId, FinderRequest request, CancellationToken cancellationToken);
}
