using FleetManagement.Application.DTOs.Reports;

namespace FleetManagement.Application.Interfaces;

public interface IReportService
{
    Task<FleetUtilizationReportDto> GetFleetUtilizationAsync(DateTime from, DateTime to);
    Task<CostReportDto> GetCostReportAsync(DateTime from, DateTime to);
    Task<DriverPerformanceReportDto> GetDriverPerformanceAsync(DateTime from, DateTime to);
}
