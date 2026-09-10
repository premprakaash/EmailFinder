using MailForge.Application.DTOs.Contacts;
using MailForge.Application.Interfaces;
using MailForge.Domain.Entities;
using MailForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MailForge.Infrastructure.Services;

public class ContactService : IContactService
{
    private readonly MailForgeDbContext _db;

    public ContactService(MailForgeDbContext db) => _db = db;

    public async Task<ContactDto> SaveAsync(Guid userId, SaveContactRequest request, CancellationToken cancellationToken)
    {
        var existing = await _db.Contacts
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (existing != null)
        {
            existing.FirstName = request.FirstName;
            existing.LastName = request.LastName;
            existing.JobTitle = request.JobTitle;
            existing.Company = request.Company;
            existing.Domain = request.Domain;
            existing.Status = request.Status;
            existing.ConfidenceScore = request.ConfidenceScore;
            existing.Source = request.Source;
            existing.LastVerifiedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return await MapToDto(existing, cancellationToken);
        }

        var contact = new Contact
        {
            UserId = userId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.ToLowerInvariant(),
            JobTitle = request.JobTitle,
            Company = request.Company,
            Domain = request.Domain,
            Status = request.Status,
            ConfidenceScore = request.ConfidenceScore,
            Source = request.Source,
            LastVerifiedAt = DateTime.UtcNow
        };

        _db.Contacts.Add(contact);
        await _db.SaveChangesAsync(cancellationToken);
        return await MapToDto(contact, cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, Guid contactId, CancellationToken cancellationToken)
    {
        var contact = await _db.Contacts.FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Contact not found.");
        contact.IsDeleted = true;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<ContactDto>> SearchAsync(Guid userId, ContactFilterRequest filter, CancellationToken cancellationToken)
    {
        var query = _db.Contacts.AsNoTracking()
            .Include(c => c.ContactTags).ThenInclude(ct => ct.Tag)
            .Where(c => c.UserId == userId);

        if (!string.IsNullOrWhiteSpace(filter.Company))
            query = query.Where(c => c.Company != null && c.Company.Contains(filter.Company));
        if (!string.IsNullOrWhiteSpace(filter.Domain))
            query = query.Where(c => c.Domain != null && c.Domain.Contains(filter.Domain));
        if (!string.IsNullOrWhiteSpace(filter.JobTitle))
            query = query.Where(c => c.JobTitle != null && c.JobTitle.Contains(filter.JobTitle));
        if (filter.Status.HasValue)
            query = query.Where(c => c.Status == filter.Status);
        if (!string.IsNullOrWhiteSpace(filter.Country))
            query = query.Where(c => c.Country != null && c.Country.Contains(filter.Country));
        if (!string.IsNullOrWhiteSpace(filter.Industry))
            query = query.Where(c => c.Industry != null && c.Industry.Contains(filter.Industry));
        if (filter.MinConfidence.HasValue)
            query = query.Where(c => c.ConfidenceScore >= filter.MinConfidence);
        if (filter.CreatedAfter.HasValue)
            query = query.Where(c => c.CreatedAt >= filter.CreatedAfter);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(c => c.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<ContactDto>();
        foreach (var item in items)
            dtos.Add(await MapToDto(item, cancellationToken));

        return new PagedResult<ContactDto>(dtos, total, filter.Page, filter.PageSize);
    }

    public async Task<ContactDto?> GetByIdAsync(Guid userId, Guid contactId, CancellationToken cancellationToken)
    {
        var contact = await _db.Contacts.Include(c => c.ContactTags).ThenInclude(ct => ct.Tag)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId, cancellationToken);
        return contact == null ? null : await MapToDto(contact, cancellationToken);
    }

    public async Task TagContactAsync(Guid userId, Guid contactId, string tagName, CancellationToken cancellationToken)
    {
        var contact = await _db.Contacts.FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Contact not found.");

        var tag = await _db.Tags.FirstOrDefaultAsync(t => t.UserId == userId && t.Name == tagName, cancellationToken);
        if (tag == null)
        {
            tag = new Tag { UserId = userId, Name = tagName };
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync(cancellationToken);
        }

        if (!await _db.ContactTags.AnyAsync(ct => ct.ContactId == contactId && ct.TagId == tag.Id, cancellationToken))
        {
            _db.ContactTags.Add(new ContactTag { ContactId = contactId, TagId = tag.Id });
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> RemoveDuplicatesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var contacts = await _db.Contacts.Where(c => c.UserId == userId).OrderBy(c => c.CreatedAt).ToListAsync(cancellationToken);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var removed = 0;

        foreach (var contact in contacts)
        {
            if (!seen.Add(contact.Email))
            {
                contact.IsDeleted = true;
                removed++;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return removed;
    }

    private static Task<ContactDto> MapToDto(Contact contact, CancellationToken cancellationToken)
    {
        var tags = contact.ContactTags?.Select(ct => ct.Tag.Name).ToList() ?? [];
        return Task.FromResult(new ContactDto(
            contact.Id, contact.FirstName, contact.LastName, contact.Email,
            contact.JobTitle, contact.Company, contact.Domain, contact.Country, contact.Industry,
            contact.Status, contact.ConfidenceScore, contact.Source, contact.LastVerifiedAt,
            contact.CreatedAt, tags));
    }
}
