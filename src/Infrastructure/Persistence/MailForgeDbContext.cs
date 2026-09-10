using MailForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MailForge.Infrastructure.Persistence;

public class MailForgeDbContext : DbContext
{
    public MailForgeDbContext(DbContextOptions<MailForgeDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<CreditBalance> CreditBalances => Set<CreditBalance>();
    public DbSet<CreditTransaction> CreditTransactions => Set<CreditTransaction>();
    public DbSet<CreditRule> CreditRules => Set<CreditRule>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<DomainEntity> Domains => Set<DomainEntity>();
    public DbSet<EmailAddress> EmailAddresses => Set<EmailAddress>();
    public DbSet<VerificationResult> VerificationResults => Set<VerificationResult>();
    public DbSet<FinderResult> FinderResults => Set<FinderResult>();
    public DbSet<SearchJob> SearchJobs => Set<SearchJob>();
    public DbSet<VerificationJob> VerificationJobs => Set<VerificationJob>();
    public DbSet<BulkJob> BulkJobs => Set<BulkJob>();
    public DbSet<BulkJobResult> BulkJobResults => Set<BulkJobResult>();
    public DbSet<Export> Exports => Set<Export>();
    public DbSet<UsageRecord> UsageRecords => Set<UsageRecord>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ContactTag> ContactTags => Set<ContactTag>();
    public DbSet<ContactList> ContactLists => Set<ContactList>();
    public DbSet<ContactListItem> ContactListItems => Set<ContactListItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SuppressionEntry> SuppressionEntries => Set<SuppressionEntry>();
    public DbSet<RateLimitConfig> RateLimitConfigs => Set<RateLimitConfig>();
    public DbSet<ProviderConfig> ProviderConfigs => Set<ProviderConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MailForgeDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
