using MailForge.Application.DTOs.Bulk;

namespace MailForge.Application.Interfaces;

public interface IBulkJobService
{
    Task<BulkJobCreateResponse> CreateJobAsync(Guid userId, Stream csvStream, string fileName, CancellationToken cancellationToken);
    Task<BulkJobDto?> GetJobAsync(Guid userId, Guid jobId, CancellationToken cancellationToken);
    Task<IReadOnlyList<BulkJobDto>> GetJobsAsync(Guid userId, CancellationToken cancellationToken);
    Task CancelJobAsync(Guid userId, Guid jobId, CancellationToken cancellationToken);
}
