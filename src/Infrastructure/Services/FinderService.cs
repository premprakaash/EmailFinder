using MailForge.Application.Interfaces;
using MailForge.Application.Interfaces.Providers;
using MailForge.Application.DTOs.Finder;
using MailForge.Domain.Entities;
using MailForge.Domain.Enums;
using MailForge.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace MailForge.Infrastructure.Services;

public class FinderService : IFinderService
{
    private readonly IEnumerable<IEmailFinderProvider> _providers;
    private readonly IEnumerable<IEmailVerificationProvider> _verificationProviders;
    private readonly IEmailPatternEngine _patternEngine;
    private readonly ICreditService _creditService;
    private readonly MailForgeDbContext _db;
    private readonly ILogger<FinderService> _logger;

    public FinderService(
        IEnumerable<IEmailFinderProvider> providers,
        IEnumerable<IEmailVerificationProvider> verificationProviders,
        IEmailPatternEngine patternEngine,
        ICreditService creditService,
        MailForgeDbContext db,
        ILogger<FinderService> logger)
    {
        _providers = providers.OrderBy(p => p.Priority);
        _verificationProviders = verificationProviders.OrderBy(p => p.Priority);
        _patternEngine = patternEngine;
        _creditService = creditService;
        _db = db;
        _logger = logger;
    }

    public async Task<FinderResponse> FindAsync(Guid userId, FinderRequest request, CancellationToken cancellationToken)
    {
        if (!await _creditService.ConsumeCreditsAsync(userId, CreditOperation.EmailFinder, "Email finder", cancellationToken: cancellationToken))
            throw new InvalidOperationException("Insufficient credits.");

        var domain = request.Domain.Trim().ToLowerInvariant().TrimStart('@');
        var candidates = new List<FinderCandidateDto>();

        foreach (var provider in _providers)
        {
            try
            {
                var results = await provider.FindAsync(new ProviderFinderRequest(request.FirstName, request.LastName, domain, request.Company, request.JobTitle), cancellationToken);
                foreach (var r in results)
                {
                    candidates.Add(new FinderCandidateDto(r.Email, r.ConfidenceScore, EmailStatus.Unknown, r.Pattern));
                }
                if (candidates.Count > 0) break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Finder provider {Provider} failed", provider.Name);
            }
        }

        if (candidates.Count == 0)
        {
            var patterns = _patternEngine.GeneratePatterns(request.FirstName, request.LastName, domain);
            foreach (var email in patterns)
            {
                candidates.Add(new FinderCandidateDto(email, 50, EmailStatus.Unknown, "pattern"));
            }
        }

        var verifiedCandidates = new List<FinderCandidateDto>();
        foreach (var candidate in candidates.Take(6))
        {
            var verification = await VerifyCandidateAsync(candidate.Email, cancellationToken);
            verifiedCandidates.Add(candidate with { Confidence = verification.Score, Status = verification.Status });

            _db.FinderResults.Add(new FinderResult
            {
                UserId = userId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Domain = domain,
                Company = request.Company,
                JobTitle = request.JobTitle,
                Email = candidate.Email,
                ConfidenceScore = verification.Score,
                Status = verification.Status,
                Provider = verification.ProviderName
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        var best = verifiedCandidates.OrderByDescending(c => c.Confidence).FirstOrDefault();
        return new FinderResponse(request.FirstName, request.LastName, domain, verifiedCandidates, best);
    }

    private async Task<ProviderVerificationResult> VerifyCandidateAsync(string email, CancellationToken cancellationToken)
    {
        foreach (var provider in _verificationProviders)
        {
            try
            {
                return await provider.VerifyAsync(email, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Verification provider {Provider} failed for {Email}", provider.Name, email);
            }
        }

        return new ProviderVerificationResult(email, EmailStatus.Unknown, 30, false, false, false, false, false, "None");
    }
}
