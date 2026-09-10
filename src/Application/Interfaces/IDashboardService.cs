using MailForge.Application.DTOs.Dashboard;

namespace MailForge.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponse> GetDashboardAsync(Guid userId, CancellationToken cancellationToken);
}
