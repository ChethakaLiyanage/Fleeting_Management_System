using System.Net;
using System.Text.Json;
using FleetManagement.Application.Common;
using FleetManagement.Application.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FleetManagement.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message, errors) = ex switch
        {
            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                "One or more validation errors occurred.",
                ve.Errors.Select(e => e.ErrorMessage).ToList()),

            InvalidOperationException ioe => (
                StatusCodes.Status400BadRequest,
                ioe.Message,
                (List<string>?)null),

            NotFoundException nfe => (
                StatusCodes.Status404NotFound,
                nfe.Message,
                (List<string>?)null),

            ConflictException ce => (
                StatusCodes.Status409Conflict,
                ce.Message,
                (List<string>?)null),

            ForbiddenException fe => (
                StatusCodes.Status403Forbidden,
                fe.Message,
                (List<string>?)null),

            UnauthorizedAccessException uae => (
                StatusCodes.Status401Unauthorized,
                uae.Message,
                (List<string>?)null),

            ExternalServiceException ese => (
                StatusCodes.Status503ServiceUnavailable,
                ese.Message,
                (List<string>?)null),

            TaskCanceledException tce => (
                StatusCodes.Status503ServiceUnavailable,
                "The request timed out. Please try again.",
                (List<string>?)null),

            OperationCanceledException => (
                StatusCodes.Status503ServiceUnavailable,
                "The operation was cancelled.",
                (List<string>?)null),

            DbUpdateException dbe when IsUniqueViolation(dbe) => (
                StatusCodes.Status409Conflict,
                "A record with the same unique value already exists.",
                (List<string>?)null),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                (List<string>?)null)
        };

        // Log unexpected errors only — don't spam logs with business errors
        if (statusCode >= 500)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }

        var response = new ApiResponse<object>
        {
            Success  = false,
            Message  = message,
            Errors   = errors
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    /// <summary>
    /// Returns true only for PostgreSQL SQLSTATE 23505 (unique_violation).
    /// Other DbUpdateExceptions (FK, invalid data, etc.) map to 500.
    /// </summary>
    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException pg && pg.SqlState == "23505";
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();
}
