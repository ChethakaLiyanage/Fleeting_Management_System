namespace FleetManagement.Application.Exceptions;

/// <summary>Thrown when an external service (e.g. FastAPI) is unavailable or times out. Maps to HTTP 503.</summary>
public class ExternalServiceException : Exception
{
    public ExternalServiceException(string message) : base(message) { }
}
