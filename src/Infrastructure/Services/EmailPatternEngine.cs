using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using MailForge.Application.Interfaces;

namespace MailForge.Infrastructure.Services;

public partial class EmailPatternEngine : IEmailPatternEngine
{
    private static readonly string[] Patterns =
    [
        "{first}",
        "{first}.{last}",
        "{f}.{last}",
        "{first}{last}",
        "{last}.{first}",
        "{f}{last}",
        "{first}_{last}",
        "{first}.{l}",
        "{first}{l}",
        "{f}{l}"
    ];

    public (string First, string Last) NormalizeName(string firstName, string lastName)
    {
        var first = NormalizePart(firstName);
        var last = NormalizePart(lastName);
        return (first, last);
    }

    public IReadOnlyList<string> GeneratePatterns(string firstName, string lastName, string domain, int maxCandidates = 12)
    {
        var (first, last) = NormalizeName(firstName, lastName);
        domain = domain.Trim().ToLowerInvariant().TrimStart('@');

        if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last) || string.IsNullOrWhiteSpace(domain))
            return [];

        var f = first.Length > 0 ? first[0].ToString() : "";
        var l = last.Length > 0 ? last[0].ToString() : "";

        var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pattern in Patterns)
        {
            var local = pattern
                .Replace("{first}", first, StringComparison.Ordinal)
                .Replace("{last}", last, StringComparison.Ordinal)
                .Replace("{f}", f, StringComparison.Ordinal)
                .Replace("{l}", l, StringComparison.Ordinal);

            if (IsValidLocalPart(local))
                candidates.Add($"{local}@{domain}");

            if (candidates.Count >= maxCandidates)
                break;
        }

        return candidates.ToList();
    }

    private static string NormalizePart(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var normalized = input.Trim().ToLowerInvariant();
        normalized = normalized.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        normalized = sb.ToString().Normalize(NormalizationForm.FormC);
        normalized = normalized.Replace("'", "", StringComparison.Ordinal)
            .Replace("-", "", StringComparison.Ordinal)
            .Replace(" ", "", StringComparison.Ordinal);
        normalized = NonAlphaNumericRegex().Replace(normalized, "");
        return normalized;
    }

    private static bool IsValidLocalPart(string local)
    {
        if (string.IsNullOrWhiteSpace(local) || local.Length > 64) return false;
        return LocalPartRegex().IsMatch(local);
    }

    [GeneratedRegex(@"^[a-z0-9._%+\-]+$")]
    private static partial Regex LocalPartRegex();

    [GeneratedRegex(@"[^a-z0-9]")]
    private static partial Regex NonAlphaNumericRegex();
}
