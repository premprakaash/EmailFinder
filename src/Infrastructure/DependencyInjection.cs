using MailForge.Application.Interfaces;
using MailForge.Application.Interfaces.Providers;
using MailForge.Infrastructure.Messaging;
using MailForge.Infrastructure.Persistence;
using MailForge.Infrastructure.Providers;
using MailForge.Infrastructure.Services;
using MailForge.Shared.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace MailForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<AppSettings>(configuration.GetSection(AppSettings.SectionName));
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));
        services.Configure<RedisSettings>(configuration.GetSection(RedisSettings.SectionName));

        services.AddDbContext<MailForgeDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddMemoryCache();
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetSection(RedisSettings.SectionName)["ConnectionString"] ?? "localhost:6379"));

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICreditService, CreditService>();
        services.AddScoped<IRateLimitService, RateLimitService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IEmailPatternEngine, EmailPatternEngine>();
        services.AddScoped<IFinderService, FinderService>();
        services.AddScoped<IVerifierService, VerifierService>();
        services.AddScoped<IDomainSearchService, DomainSearchService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IBulkJobService, BulkJobService>();
        services.AddScoped<IDomainIntelligenceService, DomainIntelligenceService>();
        services.AddSingleton<IMessagePublisher, MessagePublisher>();

        services.AddScoped<IEmailFinderProvider, InternalFinderProvider>();
        services.AddScoped<IEmailVerificationProvider, InternalVerificationProvider>();
        services.AddScoped<IDomainSearchProvider, MockDomainSearchProvider>();

        return services;
    }
}
