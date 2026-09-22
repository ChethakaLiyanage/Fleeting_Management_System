using FleetManagement.Application.Interfaces;
using FleetManagement.Application.DTOs.Auth;
using Microsoft.EntityFrameworkCore;
using FleetManagement.Domain.Entities;

namespace FleetManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly IFleetDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;

    public AuthService(IFleetDbContext context, ITokenService tokenService, IPasswordService passwordService)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordService = passwordService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !user.IsActive || !_passwordService.Verify(user.PasswordHash, request.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var roles = user.UserRoles.Select(ur => ur.Role!.Name);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken { UserId = user.Id, TokenHash = refreshToken, ExpiresAt = DateTime.UtcNow.AddDays(7) });
        await _context.SaveChangesAsync();

        return new AuthResponse { AccessToken = accessToken, RefreshToken = refreshToken };
    }
}
