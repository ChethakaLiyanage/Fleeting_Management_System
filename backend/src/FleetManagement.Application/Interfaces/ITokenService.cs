using FleetManagement.Domain.Entities;
namespace FleetManagement.Application.Interfaces;
public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
}
