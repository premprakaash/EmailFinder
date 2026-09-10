namespace MailForge.Application.Interfaces;

public interface IEmailPatternEngine
{
    IReadOnlyList<string> GeneratePatterns(string firstName, string lastName, string domain, int maxCandidates = 12);
    (string First, string Last) NormalizeName(string firstName, string lastName);
}
