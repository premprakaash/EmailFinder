namespace MailForge.Shared.Settings;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "MailForge";
    public string Audience { get; set; } = "MailForge";
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

public class AppSettings
{
    public const string SectionName = "App";
    public string Name { get; set; } = "MailForge";
    public string FrontendUrl { get; set; } = "http://localhost:3000";
    public string ApiUrl { get; set; } = "http://localhost:5000";
    public string ExportStoragePath { get; set; } = "/app/exports";
    public string BulkUploadPath { get; set; } = "/app/uploads";
    public int ExportExpirationHours { get; set; } = 24;
}

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMQ";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
}

public class RedisSettings
{
    public const string SectionName = "Redis";
    public string ConnectionString { get; set; } = "localhost:6379";
}
