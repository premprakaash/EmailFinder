using MailForge.Infrastructure;
using MailForge.Infrastructure.Persistence;
using MailForge.Workers;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.WithProperty("Application", "MailForge.Workers")
    .CreateLogger();

builder.Services.AddSerilog();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<BulkImportWorker>();

var host = builder.Build();
await host.RunAsync();
