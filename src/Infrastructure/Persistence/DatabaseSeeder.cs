using MailForge.Domain.Entities;
using MailForge.Domain.Enums;
using MailForge.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MailForge.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MailForgeDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MailForgeDbContext>>();

        await db.Database.MigrateAsync();

        if (!await db.Plans.AnyAsync())
        {
            var freePlan = new Plan
            {
                Name = "Starter",
                Slug = "starter",
                Description = "Free starter plan with monthly credits",
                MonthlyPrice = 0,
                MonthlyCredits = 1000,
                RateLimitPerMinute = 60,
                RateLimitPerDay = 1000,
                IsActive = true,
                IsDefault = true
            };
            var proPlan = new Plan
            {
                Name = "Professional",
                Slug = "professional",
                Description = "Professional plan for growing teams",
                MonthlyPrice = 49,
                MonthlyCredits = 50000,
                RateLimitPerMinute = 600,
                RateLimitPerDay = 10000,
                IsActive = true
            };
            db.Plans.AddRange(freePlan, proPlan);
        }

        if (!await db.CreditRules.AnyAsync())
        {
            db.CreditRules.AddRange(
                new CreditRule { Operation = CreditOperation.EmailFinder, CreditCost = 1, Description = "Email finder" },
                new CreditRule { Operation = CreditOperation.EmailVerification, CreditCost = 1, Description = "Email verification" },
                new CreditRule { Operation = CreditOperation.Enrichment, CreditCost = 2, Description = "Enrichment" },
                new CreditRule { Operation = CreditOperation.DomainSearch, CreditCost = 2, Description = "Domain search" },
                new CreditRule { Operation = CreditOperation.ApiRequest, CreditCost = 1, Description = "API request" },
                new CreditRule { Operation = CreditOperation.Export, CreditCost = 1, Description = "Export" }
            );
        }

        if (!await db.RateLimitConfigs.AnyAsync())
        {
            db.RateLimitConfigs.AddRange(
                new RateLimitConfig { Name = "User Default", Scope = "user", RequestsPerMinute = 600, RequestsPerDay = 10000 },
                new RateLimitConfig { Name = "API Key Default", Scope = "apikey", RequestsPerMinute = 600, RequestsPerDay = 10000 },
                new RateLimitConfig { Name = "IP Default", Scope = "ip", RequestsPerMinute = 120, RequestsPerDay = 5000 }
            );
        }

        if (!await db.ProviderConfigs.AnyAsync())
        {
            db.ProviderConfigs.AddRange(
                new ProviderConfig { Name = "Internal", Type = "finder", Priority = 100, IsEnabled = true },
                new ProviderConfig { Name = "Internal", Type = "verification", Priority = 100, IsEnabled = true },
                new ProviderConfig { Name = "MockDev", Type = "domain-search", Priority = 200, IsEnabled = true }
            );
        }

        if (!await db.Users.AnyAsync(u => u.Role == UserRole.Admin))
        {
            var admin = new User
            {
                Email = "admin@mailforge.local",
                PasswordHash = PasswordHasher.Hash("Admin123!"),
                FirstName = "Admin",
                LastName = "User",
                Role = UserRole.Admin,
                IsEmailVerified = true
            };
            var defaultPlan = await db.Plans.FirstAsync(p => p.IsDefault);
            admin.CreditBalance = new CreditBalance { UserId = admin.Id, Balance = 100000, TotalPurchased = 100000 };
            admin.Subscription = new Subscription { UserId = admin.Id, PlanId = defaultPlan.Id, StartsAt = DateTime.UtcNow, IsActive = true };
            db.Users.Add(admin);
            logger.LogInformation("Seeded admin user: admin@mailforge.local / Admin123!");
        }

        await db.SaveChangesAsync();
    }
}
