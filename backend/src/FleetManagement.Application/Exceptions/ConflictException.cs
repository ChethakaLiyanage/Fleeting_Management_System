namespace FleetManagement.Application.Exceptions;

/// <summary>Thrown when an operation would violate a uniqueness constraint. Maps to HTTP 409.</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
