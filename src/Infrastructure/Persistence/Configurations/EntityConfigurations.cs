using MailForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MailForge.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(100);
        builder.Property(u => u.LastName).HasMaxLength(100);
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}

public class CreditBalanceConfiguration : IEntityTypeConfiguration<CreditBalance>
{
    public void Configure(EntityTypeBuilder<CreditBalance> builder)
    {
        builder.HasIndex(c => c.UserId).IsUnique();
        builder.Property(c => c.RowVersion).IsRowVersion();
    }
}

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.HasIndex(a => a.KeyHash).IsUnique();
        builder.Property(a => a.KeyPrefix).HasMaxLength(12);
    }
}

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.HasIndex(c => new { c.UserId, c.Email });
        builder.Property(c => c.Email).HasMaxLength(256);
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

public class DomainEntityConfiguration : IEntityTypeConfiguration<DomainEntity>
{
    public void Configure(EntityTypeBuilder<DomainEntity> builder)
    {
        builder.HasIndex(d => d.Domain).IsUnique();
    }
}

public class EmailAddressConfiguration : IEntityTypeConfiguration<EmailAddress>
{
    public void Configure(EntityTypeBuilder<EmailAddress> builder)
    {
        builder.HasIndex(e => e.Email).IsUnique();
    }
}

public class ExportConfiguration : IEntityTypeConfiguration<Export>
{
    public void Configure(EntityTypeBuilder<Export> builder)
    {
        builder.HasIndex(e => e.DownloadToken).IsUnique();
    }
}

public class SuppressionEntryConfiguration : IEntityTypeConfiguration<SuppressionEntry>
{
    public void Configure(EntityTypeBuilder<SuppressionEntry> builder)
    {
        builder.HasIndex(s => new { s.UserId, s.Email }).IsUnique();
    }
}

public class ContactTagConfiguration : IEntityTypeConfiguration<ContactTag>
{
    public void Configure(EntityTypeBuilder<ContactTag> builder)
    {
        builder.HasIndex(ct => new { ct.ContactId, ct.TagId }).IsUnique();
    }
}

public class ContactListItemConfiguration : IEntityTypeConfiguration<ContactListItem>
{
    public void Configure(EntityTypeBuilder<ContactListItem> builder)
    {
        builder.HasIndex(i => new { i.ContactListId, i.ContactId }).IsUnique();
    }
}

public class CreditRuleConfiguration : IEntityTypeConfiguration<CreditRule>
{
    public void Configure(EntityTypeBuilder<CreditRule> builder)
    {
        builder.HasIndex(r => r.Operation).IsUnique();
    }
}

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasIndex(t => new { t.UserId, t.Name }).IsUnique();
    }
}
