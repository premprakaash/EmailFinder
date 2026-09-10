using MailForge.Application.DTOs.DomainSearch;

namespace MailForge.Application.Interfaces.Providers;

public interface IDomainSearchProvider
{
    string Name { get; }
    int Priority { get; }
    Task<IReadOnlyList<DomainContactDto>> SearchAsync(string domain, CancellationToken cancellationToken);
}
