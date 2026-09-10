using MailForge.Domain.Common;

namespace MailForge.Domain.Entities;

public class DomainEntity : BaseEntity
{
    public string Domain { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? MxRecords { get; set; }
    public string? MailProvider { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? Country { get; set; }
    public int? EmployeeCount { get; set; }
    public DateTime? LastCheckedAt { get; set; }
    public Guid? CompanyId { get; set; }

    public Company? Company { get; set; }
    public ICollection<Contact> Contacts { get; set; } = [];
}
