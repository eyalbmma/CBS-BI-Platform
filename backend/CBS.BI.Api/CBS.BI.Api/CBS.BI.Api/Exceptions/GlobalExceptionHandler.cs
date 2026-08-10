using CBS.BI.Application.Analytics.Exceptions;
using CBS.BI.Application.Security.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CBS.BI.Api.Exceptions;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is UserNotAuthenticatedException authRequiredException)
        {
            _logger.LogWarning("User authentication required: {Message}", authRequiredException.Message);

            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            httpContext.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Type = "authentication-required",
                Title = "Authentication required",
                Status = StatusCodes.Status401Unauthorized,
                Detail = authRequiredException.Message
            };

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        if (exception is AnalyticsQueryGenerationException generationException)
        {
            _logger.LogWarning("Analytics query generation failed: {Message}", generationException.Message);

            httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            httpContext.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Type = "analytics-query-generation-failed",
                Title = "Analytics query generation failed",
                Status = StatusCodes.Status422UnprocessableEntity,
                Detail = generationException.Message
            };

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        if (exception is QueryCostLimitExceededException costLimitException)
        {
            _logger.LogWarning(
            "Query cost limit exceeded. Estimated bytes: {EstimatedBytes}",
         costLimitException.EstimatedBytesProcessed);

            httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            httpContext.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Type = "query-cost-limit-exceeded",
                Title = "Query processing limit exceeded",
                Status = StatusCodes.Status422UnprocessableEntity,
                Detail = costLimitException.Message
            };

            // Add custom property for estimated bytes
            problemDetails.Extensions["estimatedBytesProcessed"] = costLimitException.EstimatedBytesProcessed;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        if (exception is AnalyticsAuthorizationException authException)
        {
            _logger.LogWarning("Analytics authorization failed: {Message}", authException.Message);

            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            httpContext.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Type = "analytics-authorization-failed",
                Title = "Access denied",
                Status = StatusCodes.Status403Forbidden,
                Detail = authException.Message
            };

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        if (exception is UnsafeAnalyticsQueryException unsafeQueryException)
        {
            _logger.LogWarning("Unsafe analytics query rejected: {Message}", unsafeQueryException.Message);

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            httpContext.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Type = "unsafe-analytics-query",
                Title = "Unsafe analytics query",
                Status = StatusCodes.Status400BadRequest,
                Detail = unsafeQueryException.Message
            };

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        return false;
    }
}
