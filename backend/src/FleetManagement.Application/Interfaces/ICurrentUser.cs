namespace FleetManagement.Application.Interfaces;

public interface ICurrentUser
{
    Guid UserId { get; }
    string Email { get; }
    string Role { get; }
    bool IsInRole(string role);
    bool IsAuthenticated { get; }
}
