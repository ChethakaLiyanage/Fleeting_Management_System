namespace FleetManagement.Application.Exceptions;

/// <summary>Thrown when an authenticated user lacks permission for a specific resource. Maps to HTTP 403.</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "You do not have permission to access this resource.")
        : base(message) { }
}
