using MailForge.Application.Interfaces;
using System.Text.RegularExpressions;

namespace MailForge.Infrastructure.Services;

public partial class CompanyResolver : ICompanyResolver
{
    private static readonly Dictionary<string, (string Company, string Domain)> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["example corp"] = ("Example Corp", "example.com"),
        ["example corporation"] = ("Example Corp", "example.com"),
        ["example"] = ("Example Corp", "example.com"),
        ["example.com"] = ("Example Corp", "example.com"),
        ["company inc"] = ("Company Inc", "company.com"),
        ["company"] = ("Company Inc", "company.com"),
        ["company.com"] = ("Company Inc", "company.com"),
        ["startup.io"] = ("Startup IO", "startup.io"),
        ["startup"] = ("Startup IO", "startup.io"),
    };

    public Task<CompanyResolveResult?> ResolveAsync(string query, CancellationToken cancellationToken = default)
    {
        query = query.Trim();
        if (string.IsNullOrWhiteSpace(query))
            return Task.FromResult<CompanyResolveResult?>(null);

        if (TryParseDomain(query, out var domainFromUrl))
            return Task.FromResult<CompanyResolveResult?>(new CompanyResolveResult(domainFromUrl, domainFromUrl, "url"));

        if (Aliases.TryGetValue(query, out var alias))
            return Task.FromResult<CompanyResolveResult?>(new CompanyResolveResult(alias.Company, alias.Domain, "directory"));

        var normalized = query.ToLowerInvariant();
        var match = Aliases.FirstOrDefault(a => normalized.Contains(a.Key, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(match.Key))
            return Task.FromResult<CompanyResolveResult?>(new CompanyResolveResult(match.Value.Company, match.Value.Domain, "directory"));

        if (DomainRegex().IsMatch(query))
            return Task.FromResult<CompanyResolveResult?>(new CompanyResolveResult(query, query.ToLowerInvariant(), "domain"));

        return Task.FromResult<CompanyResolveResult?>(null);
    }

    private static bool TryParseDomain(string input, out string domain)
    {
        domain = string.Empty;
        var value = input.Trim().ToLowerInvariant();
        if (value.StartsWith("http://", StringComparison.Ordinal) || value.StartsWith("https://", StringComparison.Ordinal))
        {
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.Host))
            {
                domain = uri.Host.StartsWith("www.", StringComparison.Ordinal) ? uri.Host[4..] : uri.Host;
                return true;
            }
        }
        if (DomainRegex().IsMatch(value))
        {
            domain = value.TrimStart('@');
            return true;
        }
        return false;
    }

    [GeneratedRegex(@"^[a-z0-9]([a-z0-9\-]{0,61}[a-z0-9])?(\.[a-z0-9]([a-z0-9\-]{0,61}[a-z0-9])?)+$")]
    private static partial Regex DomainRegex();
}
