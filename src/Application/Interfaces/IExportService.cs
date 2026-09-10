using MailForge.Application.DTOs.Exports;

namespace MailForge.Application.Interfaces;

public interface IExportService
{
    Task<ExportDto> CreateExportAsync(Guid userId, ExportRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<ExportHistoryDto>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken);
    Task<(Stream FileStream, string ContentType, string FileName)?> DownloadAsync(Guid userId, Guid exportId, string token, CancellationToken cancellationToken);
}
