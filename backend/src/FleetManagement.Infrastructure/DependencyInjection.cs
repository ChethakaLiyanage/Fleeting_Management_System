using System.Text;
using FleetManagement.Application.Interfaces;
using FleetManagement.Infrastructure.Cache;
using FleetManagement.Infrastructure.Data;
using FleetManagement.Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

namespace FleetManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<FleetDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IFleetDbContext>(provider => provider.GetRequiredService<FleetDbContext>());

        // JWT Authentication with fail-fast secret validation
        var jwtSecret = configuration["Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(jwtSecret))
        {
            throw new InvalidOperationException(
                "JWT secret is not configured. Set Jwt:Secret via User Secrets or environment variable.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = configuration["Jwt:Issuer"],
                    ValidAudience            = configuration["Jwt:Audience"],
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ClockSkew                = TimeSpan.Zero
                };
            });

        // FastApiClient named/typed HttpClient with 8-second timeout
        var fastApiBaseUrl = configuration["FastApi:BaseUrl"] ?? "http://localhost:8000";
        services.AddHttpClient<FastApiClient>(client =>
        {
            client.BaseAddress = new Uri(fastApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(8);
        });

        // Memory cache + Dashboard cache invalidator
        services.AddMemoryCache();
        services.AddScoped<IDashboardCache, DashboardCacheInvalidator>();

        // Health checks
        services.AddHealthChecks()
            .AddNpgSql(
                connectionString ?? string.Empty,
                name: "postgresql",
                failureStatus: HealthStatus.Unhealthy)
            .AddUrlGroup(
                new Uri($"{fastApiBaseUrl.TrimEnd('/')}/health"),
                name: "fastapi",
                failureStatus: HealthStatus.Degraded);

        return services;
    }
}
