using FleetManagement.Application.DTOs.Auth;
namespace FleetManagement.Application.Interfaces;
public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
