using MailForge.Application.DTOs.Contacts;

namespace MailForge.Application.Interfaces;

public interface IContactService
{
    Task<ContactDto> SaveAsync(Guid userId, SaveContactRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, Guid contactId, CancellationToken cancellationToken);
    Task<PagedResult<ContactDto>> SearchAsync(Guid userId, ContactFilterRequest filter, CancellationToken cancellationToken);
    Task<ContactDto?> GetByIdAsync(Guid userId, Guid contactId, CancellationToken cancellationToken);
    Task TagContactAsync(Guid userId, Guid contactId, string tagName, CancellationToken cancellationToken);
    Task<int> RemoveDuplicatesAsync(Guid userId, CancellationToken cancellationToken);
}
