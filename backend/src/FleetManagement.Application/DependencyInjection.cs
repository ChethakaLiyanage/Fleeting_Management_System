using FleetManagement.Application.Interfaces;
using FleetManagement.Application.Services;
using FleetManagement.Application.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace FleetManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // FluentValidation — auto-validation hook + assembly scanning for validators
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateVehicleValidator>();

        // Domain & Application Services
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IDriverService, DriverService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<ITripService, TripService>();
        services.AddScoped<IInspectionService, InspectionService>();
        services.AddScoped<IIncidentService, IncidentService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFuelService, FuelService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}
