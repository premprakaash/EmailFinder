using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using DnsClient;
using MailForge.Application.Interfaces.Providers;
using MailForge.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MailForge.Infrastructure.Providers;

public partial class InternalVerificationProvider : IEmailVerificationProvider
{
    private readonly LookupClient _dns;
    private readonly ILogger<InternalVerificationProvider> _logger;
    private static readonly HashSet<string> DisposableDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "mailinator.com", "guerrillamail.com", "tempmail.com", "throwaway.email",
        "yopmail.com", "10minutemail.com", "trashmail.com"
    };
    private static readonly HashSet<string> RolePrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin", "info", "support", "sales", "contact", "help", "noreply", "no-reply",
        "billing", "hr", "jobs", "careers", "marketing", "team", "hello", "office"
    };

    public string Name => "Internal";
    public int Priority => 100;

    public InternalVerificationProvider(ILogger<InternalVerificationProvider> logger)
    {
        _logger = logger;
        _dns = new LookupClient(new LookupClientOptions { Timeout = TimeSpan.FromSeconds(5) });
    }

    public async Task<ProviderVerificationResult> VerifyAsync(string email, CancellationToken cancellationToken)
    {
        email = email.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(email))
            return Fail(email, EmailStatus.Invalid, 0, "Invalid email syntax");

        var parts = email.Split('@');
        var local = parts[0];
        var domain = parts[1];

        if (DisposableDomains.Contains(domain))
            return new ProviderVerificationResult(email, EmailStatus.Disposable, 10, false, false, false, true, IsRoleBased(local), Name, "Disposable domain");

        if (IsRoleBased(local))
            return new ProviderVerificationResult(email, EmailStatus.RoleBased, 30, false, false, false, false, true, Name, "Role-based account");

        var mxValid = false;
        var smtpValid = false;
        var catchAll = false;
        string? mxHost = null;

        try
        {
            var mxResult = await _dns.QueryAsync(domain, QueryType.MX, cancellationToken: cancellationToken);
            var mxRecords = mxResult.Answers.MxRecords().OrderBy(r => r.Preference).ToList();
            mxValid = mxRecords.Count > 0;
            mxHost = mxRecords.FirstOrDefault()?.Exchange.Value?.TrimEnd('.');

            if (!mxValid)
            {
                var aResult = await _dns.QueryAsync(domain, QueryType.A, cancellationToken: cancellationToken);
                mxValid = aResult.Answers.ARecords().Any();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DNS lookup failed for {Domain}", domain);
            return new ProviderVerificationResult(email, EmailStatus.Unknown, 20, false, false, false, false, false, Name, "DNS lookup failed");
        }

        if (!mxValid)
            return Fail(email, EmailStatus.Invalid, 15, "No MX or A records");

        if (mxHost != null)
        {
            try
            {
                var (smtpResult, isCatchAll) = await CheckSmtpAsync(mxHost, email, domain, cancellationToken);
                smtpValid = smtpResult;
                catchAll = isCatchAll;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SMTP check failed for {Email}", email);
            }
        }

        var score = CalculateScore(mxValid, smtpValid, catchAll, false, IsRoleBased(local));
        var status = DetermineStatus(mxValid, smtpValid, catchAll, false, IsRoleBased(local), score);

        return new ProviderVerificationResult(email, status, score, mxValid, smtpValid, catchAll, false, IsRoleBased(local), Name);
    }

    private static async Task<(bool Valid, bool CatchAll)> CheckSmtpAsync(string mxHost, string email, string domain, CancellationToken cancellationToken)
    {
        using var client = new TcpClient();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(8));

        await client.ConnectAsync(mxHost, 25, cts.Token);
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream) { AutoFlush = true, NewLine = "\r\n" };

        await ReadResponseAsync(reader);
        await SendCommandAsync(writer, reader, $"EHLO mailforge.local");
        await SendCommandAsync(writer, reader, $"MAIL FROM:<verify@mailforge.local>");
        var rcptCode = await SendCommandAsync(writer, reader, $"RCPT TO:<{email}>");

        var randomEmail = $"nonexistent{Guid.NewGuid():N}@{domain}";
        var catchAllCode = await SendCommandAsync(writer, reader, $"RCPT TO:<{randomEmail}>");

        await SendCommandAsync(writer, reader, "QUIT");

        var valid = rcptCode >= 250 && rcptCode < 260;
        var catchAll = catchAllCode >= 250 && catchAllCode < 260;
        return (valid, catchAll);
    }

    private static async Task<int> SendCommandAsync(StreamWriter writer, StreamReader reader, string command)
    {
        await writer.WriteLineAsync(command);
        var response = await ReadResponseAsync(reader);
        return int.TryParse(response[..3], out var code) ? code : 500;
    }

    private static async Task<string> ReadResponseAsync(StreamReader reader)
    {
        var line = await reader.ReadLineAsync() ?? "";
        return line;
    }

    private static bool IsRoleBased(string local) =>
        RolePrefixes.Any(p => local.Equals(p, StringComparison.OrdinalIgnoreCase) || local.StartsWith(p + ".", StringComparison.OrdinalIgnoreCase));

    private static int CalculateScore(bool mx, bool smtp, bool catchAll, bool disposable, bool roleBased)
    {
        if (disposable) return 10;
        if (roleBased) return 30;
        var score = 40;
        if (mx) score += 30;
        if (smtp) score += 25;
        if (catchAll) score -= 20;
        return Math.Clamp(score, 0, 100);
    }

    private static EmailStatus DetermineStatus(bool mx, bool smtp, bool catchAll, bool disposable, bool roleBased, int score)
    {
        if (disposable) return EmailStatus.Disposable;
        if (roleBased) return EmailStatus.RoleBased;
        if (catchAll) return EmailStatus.CatchAll;
        if (!mx) return EmailStatus.Invalid;
        if (smtp && score >= 70) return EmailStatus.Valid;
        if (score >= 50) return EmailStatus.Risky;
        if (!smtp) return EmailStatus.Unknown;
        return EmailStatus.Invalid;
    }

    private static ProviderVerificationResult Fail(string email, EmailStatus status, int score, string details) =>
        new(email, status, score, false, false, false, false, false, "Internal", details);

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
