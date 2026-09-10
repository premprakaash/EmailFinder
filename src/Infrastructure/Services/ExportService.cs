using System.Globalization;
using System.Text;
using System.Text.Json;
using MailForge.Application.DTOs.Contacts;
using MailForge.Application.DTOs.Exports;
using MailForge.Application.Interfaces;
using MailForge.Domain.Entities;
using MailForge.Domain.Enums;
using MailForge.Infrastructure.Persistence;
using MailForge.Shared.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MailForge.Infrastructure.Services;

public class ExportService : IExportService
{
    private readonly MailForgeDbContext _db;
    private readonly IContactService _contactService;
    private readonly ICreditService _creditService;
    private readonly AppSettings _settings;

    public ExportService(
        MailForgeDbContext db,
        IContactService contactService,
        ICreditService creditService,
        IOptions<AppSettings> settings)
    {
        _db = db;
        _contactService = contactService;
        _creditService = creditService;
        _settings = settings.Value;
    }

    public async Task<ExportDto> CreateExportAsync(Guid userId, ExportRequest request, CancellationToken cancellationToken)
    {
        if (!await _creditService.ConsumeCreditsAsync(userId, CreditOperation.Export, "Contact export", cancellationToken: cancellationToken))
            throw new InvalidOperationException("Insufficient credits.");

        var filter = string.IsNullOrWhiteSpace(request.Filters)
            ? new ContactFilterRequest(PageSize: 10000)
            : JsonSerializer.Deserialize<ContactFilterRequest>(request.Filters) ?? new ContactFilterRequest(PageSize: 10000);

        var contacts = await _contactService.SearchAsync(userId, filter, cancellationToken);
        Directory.CreateDirectory(_settings.ExportStoragePath);

        var exportId = Guid.NewGuid();
        var token = Guid.NewGuid().ToString("N");
        var extension = request.Format.ToLowerInvariant() == "json" ? "json" : "csv";
        var fileName = $"export_{exportId:N}.{extension}";
        var filePath = Path.Combine(_settings.ExportStoragePath, fileName);

        if (extension == "json")
        {
            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(contacts.Items, new JsonSerializerOptions { WriteIndented = true }), cancellationToken);
        }
        else
        {
            await WriteCsvAsync(filePath, contacts.Items, cancellationToken);
        }

        var export = new Export
        {
            Id = exportId,
            UserId = userId,
            Format = extension,
            ContactCount = contacts.Items.Count,
            CreditsConsumed = 1,
            FilePath = filePath,
            DownloadToken = token,
            ExpiresAt = DateTime.UtcNow.AddHours(_settings.ExportExpirationHours),
            Filters = request.Filters
        };

        _db.Exports.Add(export);
        await _db.SaveChangesAsync(cancellationToken);

        return new ExportDto(export.Id, export.Format, export.ContactCount, export.CreditsConsumed,
            export.CreatedAt, export.ExpiresAt, $"/api/exports/{export.Id}/download?token={token}");
    }

    public async Task<IReadOnlyList<ExportHistoryDto>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _db.Exports.AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new ExportHistoryDto(e.Id, e.Format, e.ContactCount, e.CreatedAt, e.ExpiresAt, e.ExpiresAt < DateTime.UtcNow))
            .ToListAsync(cancellationToken);
    }

    public async Task<(Stream FileStream, string ContentType, string FileName)?> DownloadAsync(Guid userId, Guid exportId, string token, CancellationToken cancellationToken)
    {
        var export = await _db.Exports.FirstOrDefaultAsync(e => e.Id == exportId && e.UserId == userId, cancellationToken);
        if (export == null || export.DownloadToken != token || export.ExpiresAt < DateTime.UtcNow)
            return null;

        if (!File.Exists(export.FilePath))
            return null;

        export.DownloadedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var stream = new FileStream(export.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var contentType = export.Format == "json" ? "application/json" : "text/csv";
        return (stream, contentType, Path.GetFileName(export.FilePath));
    }

    private static async Task WriteCsvAsync(string path, IReadOnlyList<ContactDto> contacts, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("first_name,last_name,email,job_title,company,domain,status,confidence,source");
        foreach (var c in contacts)
        {
            sb.AppendLine(string.Join(",",
                Escape(c.FirstName), Escape(c.LastName), Escape(c.Email),
                Escape(c.JobTitle ?? ""), Escape(c.Company ?? ""), Escape(c.Domain ?? ""),
                c.Status.ToString(), c.ConfidenceScore.ToString(CultureInfo.InvariantCulture),
                Escape(c.Source ?? "")));
        }
        await File.WriteAllTextAsync(path, sb.ToString(), cancellationToken);
    }

    private static string Escape(string value) =>
        value.Contains(',') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}
