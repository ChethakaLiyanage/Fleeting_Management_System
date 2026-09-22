using FleetManagement.Application.DTOs.Users;
namespace FleetManagement.Application.Interfaces;
public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(Guid id);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
}
