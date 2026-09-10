using System.Globalization;
using System.Text;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using MailForge.Application.DTOs.Finder;
using MailForge.Application.Interfaces;
using MailForge.Domain.Entities;
using MailForge.Domain.Enums;
using MailForge.Infrastructure.Persistence;
using MailForge.Infrastructure.Services;
using MailForge.Shared.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MailForge.Workers;

public class BulkImportWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<BulkImportWorker> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public BulkImportWorker(IServiceProvider services, IOptions<RabbitMqSettings> settings, ILogger<BulkImportWorker> logger)
    {
        _services = services;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            Port = _settings.Port,
            UserName = _settings.Username,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare("bulk-import", durable: true, exclusive: false, autoDelete: false);
        _channel.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<BulkJobMessage>(json)!;
                await ProcessJobAsync(message, stoppingToken);
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process bulk job message");
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume("bulk-import", autoAck: false, consumer);
        _logger.LogInformation("Bulk import worker started");
        return Task.CompletedTask;
    }

    private async Task ProcessJobAsync(BulkJobMessage message, CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MailForgeDbContext>();
        var finder = scope.ServiceProvider.GetRequiredService<IFinderService>();
        var credit = scope.ServiceProvider.GetRequiredService<ICreditService>();

        var job = await db.BulkJobs.FirstOrDefaultAsync(j => j.Id == message.JobId, ct);
        if (job == null || job.Status == JobStatus.Cancelled) return;

        job.Status = JobStatus.Processing;
        await db.SaveChangesAsync(ct);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HeaderValidated = null, MissingFieldFound = null };
        var rowNumber = 0;

        await using var stream = File.OpenRead(job.FilePath);
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, config);

        await foreach (var row in csv.GetRecordsAsync<BulkCsvRow>(ct))
        {
            if (await db.BulkJobs.AsNoTracking().Where(j => j.Id == job.Id).Select(j => j.Status).FirstAsync(ct) == JobStatus.Cancelled)
                break;

            rowNumber++;
            try
            {
                if (string.IsNullOrWhiteSpace(row.FirstName) || string.IsNullOrWhiteSpace(row.LastName) || string.IsNullOrWhiteSpace(row.Domain))
                {
                    job.FailedCount++;
                    db.BulkJobResults.Add(new BulkJobResult
                    {
                        BulkJobId = job.Id, RowNumber = rowNumber,
                        FirstName = row.FirstName ?? "", LastName = row.LastName ?? "", Domain = row.Domain ?? "",
                        ErrorMessage = "Missing required fields"
                    });
                }
                else if (await credit.GetBalanceAsync(message.UserId, ct) < 1)
                {
                    job.FailedCount++;
                    db.BulkJobResults.Add(new BulkJobResult
                    {
                        BulkJobId = job.Id, RowNumber = rowNumber,
                        FirstName = row.FirstName, LastName = row.LastName, Domain = row.Domain,
                        ErrorMessage = "Insufficient credits"
                    });
                }
                else
                {
                    var result = await finder.FindAsync(message.UserId, new FinderRequest(row.FirstName, row.LastName, row.Domain), ct);
                    var best = result.BestMatch;
                    if (best != null)
                    {
                        job.FoundCount++;
                        if (best.Status == EmailStatus.Valid) job.VerifiedCount++;
                        else if (best.Status == EmailStatus.Invalid) job.InvalidCount++;
                        else if (best.Status == EmailStatus.Risky) job.RiskyCount++;
                    }

                    db.BulkJobResults.Add(new BulkJobResult
                    {
                        BulkJobId = job.Id, RowNumber = rowNumber,
                        FirstName = row.FirstName, LastName = row.LastName, Domain = row.Domain,
                        Email = best?.Email, ConfidenceScore = best?.Confidence, Status = best?.Status
                    });
                }
            }
            catch (Exception ex)
            {
                job.FailedCount++;
                db.BulkJobResults.Add(new BulkJobResult
                {
                    BulkJobId = job.Id, RowNumber = rowNumber,
                    FirstName = row.FirstName ?? "", LastName = row.LastName ?? "", Domain = row.Domain ?? "",
                    ErrorMessage = ex.Message
                });
            }

            job.ProcessedRows = rowNumber;
            if (rowNumber % 10 == 0) await db.SaveChangesAsync(ct);
        }

        job.Status = JobStatus.Completed;
        job.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Bulk job {JobId} completed: {Processed}/{Total}", job.Id, job.ProcessedRows, job.TotalRows);
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
