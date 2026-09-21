using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;

namespace FleetManagement.Application.Services;

public class TokenService : ITokenService
{
    public string GenerateAccessToken(User user, IEnumerable<string> roles) => "mock-jwt-token";
    public string GenerateRefreshToken() => Guid.NewGuid().ToString();
}
