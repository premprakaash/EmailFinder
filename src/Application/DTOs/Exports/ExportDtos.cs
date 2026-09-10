namespace MailForge.Application.DTOs.Exports;

public record ExportRequest(string Format = "csv", string? Filters = null);
public record ExportDto(Guid Id, string Format, int ContactCount, long CreditsConsumed, DateTime CreatedAt, DateTime ExpiresAt, string DownloadUrl);
public record ExportHistoryDto(Guid Id, string Format, int ContactCount, DateTime CreatedAt, DateTime ExpiresAt, bool IsExpired);
