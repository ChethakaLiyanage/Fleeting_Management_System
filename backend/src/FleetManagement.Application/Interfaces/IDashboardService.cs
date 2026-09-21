using FleetManagement.Application.DTOs.Dashboard;

namespace FleetManagement.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
    Task<IEnumerable<AlertDto>> GetActiveAlertsAsync();
}
