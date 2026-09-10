using MailForge.Application.DTOs.Auth;
using MailForge.Application.DTOs.Finder;
using MailForge.Infrastructure.Services;
using Xunit;

namespace MailForge.IntegrationTests;

public class AuthIntegrationTests
{
    [Fact]
    public void PasswordHasher_Integration_WorksCorrectly()
    {
        var password = "SecurePass123!";
        var hash = PasswordHasher.Hash(password);
        Assert.True(PasswordHasher.Verify(password, hash));
    }

    [Fact]
    public void RegisterRequest_Record_CreatesCorrectly()
    {
        var req = new RegisterRequest("test@example.com", "Pass123!", "John", "Smith");
        Assert.Equal("test@example.com", req.Email);
        Assert.Equal("John", req.FirstName);
    }

    [Fact]
    public void FinderRequest_Record_CreatesCorrectly()
    {
        var req = new FinderRequest("John", "Smith", "example.com");
        Assert.Equal("example.com", req.Domain);
    }
}
