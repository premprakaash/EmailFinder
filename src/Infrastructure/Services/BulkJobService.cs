using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MailForge.Application.DTOs.Bulk;
using MailForge.Application.Interfaces;
using MailForge.Domain.Entities;
using MailForge.Domain.Enums;
using MailForge.Infrastructure.Persistence;
using MailForge.Shared.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MailForge.Infrastructure.Services;

public record BulkCsvRow
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Domain { get; set; }
}

public class BulkJobService : IBulkJobService
{
    private readonly MailForgeDbContext _db;
    private readonly IMessagePublisher _publisher;
    private readonly AppSettings _settings;

    public BulkJobService(MailForgeDbContext db, IMessagePublisher publisher, IOptions<AppSettings> settings)
    {
        _db = db;
        _publisher = publisher;
        _settings = settings.Value;
    }

    public async Task<BulkJobCreateResponse> CreateJobAsync(Guid userId, Stream csvStream, string fileName, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_settings.BulkUploadPath);
        var jobId = Guid.NewGuid();
        var filePath = Path.Combine(_settings.BulkUploadPath, $"{jobId:N}.csv");

        await using (var fileStream = File.Create(filePath))
        {
            await csvStream.CopyToAsync(fileStream, cancellationToken);
        }

        var totalRows = 0;
        using (var reader = new StreamReader(filePath))
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HeaderValidated = null, MissingFieldFound = null };
            using var csv = new CsvReader(reader, config);
            await foreach (var _ in csv.GetRecordsAsync<BulkCsvRow>(cancellationToken))
                totalRows++;
        }

        var job = new BulkJob
        {
            Id = jobId,
            UserId = userId,
            FileName = fileName,
            FilePath = filePath,
            Status = JobStatus.Pending,
            TotalRows = totalRows
        };

        _db.BulkJobs.Add(job);
        await _db.SaveChangesAsync(cancellationToken);

        await _publisher.PublishAsync(Shared.Constants.QueueNames.BulkImport, new BulkJobMessage(jobId, userId), cancellationToken);

        return new BulkJobCreateResponse(jobId, "Bulk job created and queued for processing.");
    }

    public async Task<BulkJobDto?> GetJobAsync(Guid userId, Guid jobId, CancellationToken cancellationToken)
    {
        var job = await _db.BulkJobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == jobId && j.UserId == userId, cancellationToken);
        return job == null ? null : MapToDto(job);
    }

    public async Task<IReadOnlyList<BulkJobDto>> GetJobsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var jobs = await _db.BulkJobs.AsNoTracking()
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
        return jobs.Select(MapToDto).ToList();
    }

    public async Task CancelJobAsync(Guid userId, Guid jobId, CancellationToken cancellationToken)
    {
        var job = await _db.BulkJobs.FirstOrDefaultAsync(j => j.Id == jobId && j.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Job not found.");

        if (job.Status is JobStatus.Pending or JobStatus.Processing)
        {
            job.Status = JobStatus.Cancelled;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private static BulkJobDto MapToDto(BulkJob job) => new(
        job.Id, job.FileName, job.Status, job.TotalRows, job.ProcessedRows,
        job.FoundCount, job.VerifiedCount, job.InvalidCount, job.RiskyCount,
        job.FailedCount, job.TotalRows - job.ProcessedRows, job.CreatedAt, job.CompletedAt);
}
