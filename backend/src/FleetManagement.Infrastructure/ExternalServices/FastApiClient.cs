using FleetManagement.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace FleetManagement.Infrastructure.ExternalServices;

public class FastApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<FastApiClient> _logger;

    public FastApiClient(HttpClient http, ILogger<FastApiClient> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<string> PredictMaintenanceAsync(Guid vehicleId, decimal odometer,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("FastAPI maintenance prediction started for vehicle {Id}", vehicleId);

            var payload  = new { vehicle_id = vehicleId, current_odometer = odometer };
            var response = await _http.PostAsJsonAsync("/api/predict-maintenance", payload, ct);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("FastAPI maintenance prediction succeeded for vehicle {Id}", vehicleId);
            return await response.Content.ReadAsStringAsync(ct);
        }
        catch (TaskCanceledException ex)
        {
            // HttpClient.Timeout surfaces as TaskCanceledException, NOT TimeoutException
            _logger.LogWarning(ex, "FastAPI request timed out for vehicle {Id}", vehicleId);
            throw new ExternalServiceException(
                "Analytics service is temporarily unavailable. Please try again later.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "FastAPI unreachable for vehicle {Id}", vehicleId);
            throw new ExternalServiceException(
                "Analytics service is temporarily unavailable. Please try again later.");
        }
    }

    public async Task<string> ForecastCostAsync(List<decimal> historicalCosts, int months,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("FastAPI cost forecast started");
            var payload  = new { historical_costs = historicalCosts, months_to_forecast = months };
            var response = await _http.PostAsJsonAsync("/api/forecast-cost", payload, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(ct);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "FastAPI cost forecast timed out");
            throw new ExternalServiceException("Analytics service is temporarily unavailable.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "FastAPI cost forecast unreachable");
            throw new ExternalServiceException("Analytics service is temporarily unavailable.");
        }
    }
}
