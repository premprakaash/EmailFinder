using MailForge.Domain.Common;
using MailForge.Domain.Enums;

namespace MailForge.Domain.Entities;

public class CreditRule : BaseEntity
{
    public CreditOperation Operation { get; set; }
    public long CreditCost { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
