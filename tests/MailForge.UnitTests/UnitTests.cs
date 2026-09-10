using MailForge.Application.Interfaces;
using MailForge.Infrastructure.Services;

namespace MailForge.UnitTests;

public class EmailPatternEngineTests
{
    private readonly IEmailPatternEngine _engine = new EmailPatternEngine();

    [Fact]
    public void GeneratePatterns_StandardName_ReturnsExpectedPatterns()
    {
        var patterns = _engine.GeneratePatterns("John", "Smith", "example.com");
        Assert.Contains("john@example.com", patterns);
        Assert.Contains("john.smith@example.com", patterns);
        Assert.Contains("j.smith@example.com", patterns);
        Assert.Contains("johnsmith@example.com", patterns);
        Assert.True(patterns.Count <= 12);
    }

    [Fact]
    public void NormalizeName_ApostropheName_StripsSpecialChars()
    {
        var (first, last) = _engine.NormalizeName("John", "O'Connor");
        Assert.Equal("john", first);
        Assert.Equal("oconnor", last);
    }

    [Fact]
    public void GeneratePatterns_EmptyInput_ReturnsEmpty()
    {
        var patterns = _engine.GeneratePatterns("", "Smith", "example.com");
        Assert.Empty(patterns);
    }
}

public class PasswordHasherTests
{
    [Fact]
    public void HashAndVerify_Works()
    {
        var hash = PasswordHasher.Hash("TestPassword123!");
        Assert.True(PasswordHasher.Verify("TestPassword123!", hash));
        Assert.False(PasswordHasher.Verify("WrongPassword", hash));
    }
}

public class CreditCalculationTests
{
    [Theory]
    [InlineData(100, 1, 99)]
    [InlineData(0, 1, -1)]
    public void BalanceCalculation(long start, long cost, long expected)
    {
        Assert.Equal(expected, start - cost);
    }
}

public class DeduplicationTests
{
    [Fact]
    public void RemoveDuplicates_KeepsFirstOccurrence()
    {
        var emails = new[] { "a@test.com", "b@test.com", "a@test.com", "c@test.com" };
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var unique = emails.Where(e => seen.Add(e)).ToList();
        Assert.Equal(3, unique.Count);
        Assert.Equal("a@test.com", unique[0]);
    }
}

public class CompanyResolverTests
{
    private readonly ICompanyResolver _resolver = new CompanyResolver();

    [Theory]
    [InlineData("Example Corp", "example.com")]
    [InlineData("company inc", "company.com")]
    [InlineData("https://www.example.com", "example.com")]
    [InlineData("startup.io", "startup.io")]
    public async Task Resolve_KnownCompany_ReturnsDomain(string query, string expectedDomain)
    {
        var result = await _resolver.ResolveAsync(query);
        Assert.NotNull(result);
        Assert.Equal(expectedDomain, result!.Domain);
    }

    [Fact]
    public async Task Resolve_UnknownCompany_ReturnsNull()
    {
        var result = await _resolver.ResolveAsync("Totally Unknown Corp XYZ");
        Assert.Null(result);
    }
}
