using System.Net;
using System.Text.Json;
using EventNest.Shared.Application.DTOs;
using EventNest.Shared.Domain.Exceptions;

namespace EventNest.EventService.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                validationEx.Message,
                validationEx.Errors),
            UnauthorizedException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                unauthorizedEx.Message,
                (Dictionary<string, string[]>?)null),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                notFoundEx.Message,
                (Dictionary<string, string[]>?)null),
            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                conflictEx.Message,
                (Dictionary<string, string[]>?)null),
            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred.",
                (Dictionary<string, string[]>?)null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred.");
        }

        var apiResponse = ApiResponseDto<object>.Fail(
            (int)statusCode,
            message,
            errors);

        response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await response.WriteAsync(JsonSerializer.Serialize(apiResponse, jsonOptions));
    }
}
