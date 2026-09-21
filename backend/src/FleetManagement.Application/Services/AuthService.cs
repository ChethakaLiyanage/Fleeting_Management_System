using FleetManagement.Application.Interfaces;
using FleetManagement.Application.DTOs.Auth;
using Microsoft.EntityFrameworkCore;
using FleetManagement.Domain.Entities;

namespace FleetManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly IFleetDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(IFleetDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.PasswordHash == request.Password); // Simplified for now

        if (user == null || !user.IsActive)
            throw new Exception("Invalid credentials");

        var roles = user.UserRoles.Select(ur => ur.Role!.Name);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshTokens.Add(new RefreshToken { TokenHash = refreshToken, ExpiresAt = DateTime.UtcNow.AddDays(7) });
        await _context.SaveChangesAsync();

        return new AuthResponse { AccessToken = accessToken, RefreshToken = refreshToken };
    }
}
