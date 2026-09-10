using MailForge.Domain.Enums;

namespace MailForge.Application.DTOs.Bulk;

public record BulkJobDto(
    Guid Id,
    string FileName,
    JobStatus Status,
    int TotalRows,
    int ProcessedRows,
    int FoundCount,
    int VerifiedCount,
    int InvalidCount,
    int RiskyCount,
    int FailedCount,
    int Remaining,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public record BulkJobCreateResponse(Guid JobId, string Message);
