using FleetManagement.Application.Interfaces;
using FleetManagement.Application.DTOs.Users;
using FleetManagement.Domain.Entities;

namespace FleetManagement.Application.Services;

public class UserService : IUserService
{
    private readonly IFleetDbContext _context;

    public UserService(IFleetDbContext context)
    {
        _context = context;
    }

    public Task<UserDto> GetUserByIdAsync(Guid id) => throw new NotImplementedException();
    public Task<UserDto> CreateUserAsync(CreateUserRequest request) => throw new NotImplementedException();
}
