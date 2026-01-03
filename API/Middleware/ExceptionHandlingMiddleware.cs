using Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace API.Middleware;

/// <summary>
/// Global exception handling middleware
/// Converts domain exceptions to appropriate HTTP responses
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case VideoNotFoundException:
            case CommentNotFoundException:
            case UserProfileNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = exception.Message;
                _logger.LogWarning(exception, "Resource not found: {Message}", exception.Message);
                break;

            case UnauthorizedOperationException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Message = exception.Message;
                _logger.LogWarning(exception, "Unauthorized operation: {Message}", exception.Message);
                break;

            case AuthenticationRequiredException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = exception.Message;
                _logger.LogWarning(exception, "Authentication required: {Message}", exception.Message);
                break;

            case RateLimitExceededException:
                response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                response.Message = exception.Message;
                _logger.LogWarning(exception, "Rate limit exceeded: {Message}", exception.Message);
                break;

            case InvalidCommentStateException:
            case DomainException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = exception.Message;
                _logger.LogWarning(exception, "Domain exception: {Message}", exception.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "An error occurred while processing your request.";
                _logger.LogError(exception, "Unhandled exception occurred");
                break;
        }

        context.Response.StatusCode = response.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

