namespace FleetManagement.Application.Interfaces;

/// <summary>
/// Abstraction for invalidating the dashboard summary cache.
/// Mutation services depend on this interface, NOT on DashboardService directly,
/// avoiding circular coupling.
/// </summary>
public interface IDashboardCache
{
    void Invalidate();
}
