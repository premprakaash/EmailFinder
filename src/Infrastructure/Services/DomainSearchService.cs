using MailForge.Application.DTOs.DomainSearch;
using MailForge.Application.Interfaces;
using MailForge.Application.Interfaces.Providers;
using MailForge.Domain.Entities;
using MailForge.Domain.Enums;
using MailForge.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace MailForge.Infrastructure.Services;

public class DomainSearchService : IDomainSearchService
{
    private readonly IEnumerable<IDomainSearchProvider> _providers;
    private readonly ICreditService _creditService;
    private readonly ICompanyResolver _companyResolver;
    private readonly MailForgeDbContext _db;
    private readonly ILogger<DomainSearchService> _logger;

    public DomainSearchService(
        IEnumerable<IDomainSearchProvider> providers,
        ICreditService creditService,
        ICompanyResolver companyResolver,
        MailForgeDbContext db,
        ILogger<DomainSearchService> logger)
    {
        _providers = providers.OrderBy(p => p.Priority);
        _creditService = creditService;
        _companyResolver = companyResolver;
        _db = db;
        _logger = logger;
    }

    public async Task<DomainSearchResponse> SearchAsync(Guid userId, DomainSearchRequest request, CancellationToken cancellationToken)
    {
        if (!await _creditService.ConsumeCreditsAsync(userId, CreditOperation.DomainSearch, "Domain search", cancellationToken: cancellationToken))
            throw new InvalidOperationException("Insufficient credits.");

        string domain;
        string? company = request.Company?.Trim();
        string? resolvedFrom = null;

        if (!string.IsNullOrWhiteSpace(request.Domain))
        {
            domain = request.Domain.Trim().ToLowerInvariant().TrimStart('@');
        }
        else if (!string.IsNullOrWhiteSpace(company))
        {
            var resolved = await _companyResolver.ResolveAsync(company, cancellationToken)
                ?? throw new InvalidOperationException($"Could not resolve company '{company}' to a domain. Try entering the company website domain directly.");
            domain = resolved.Domain;
            company = resolved.CompanyName;
            resolvedFrom = resolved.Source;
        }
        else
        {
            throw new InvalidOperationException("Provide a company name or domain to discover people.");
        }

        IReadOnlyList<DomainContactDto> contacts = [];

        foreach (var provider in _providers)
        {
            try
            {
                contacts = await provider.SearchAsync(domain, cancellationToken);
                if (contacts.Count > 0) break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Domain search provider {Provider} failed", provider.Name);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.JobTitle))
        {
            var filter = request.JobTitle.Trim();
            contacts = contacts.Where(c => c.JobTitle != null &&
                c.JobTitle.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        company ??= contacts.FirstOrDefault()?.Company;

        _db.SearchJobs.Add(new SearchJob
        {
            UserId = userId,
            Domain = domain,
            Status = JobStatus.Completed,
            TotalFound = contacts.Count
        });
        await _db.SaveChangesAsync(cancellationToken);

        return new DomainSearchResponse(domain, company, contacts, resolvedFrom);
    }
}
