using HealthAxis.Shared.DTOs;
using HealthAxis.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using HealthAxis.API.DTOs;

namespace HealthAxis.API.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
            Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An Unexpected Error occurred : {Message}", exception.Message);

            var (statusCode, message) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                ValidationException => (StatusCodes.Status400BadRequest, exception.Message),
                BusinessRuleException => (StatusCodes.Status400BadRequest, exception.Message),
                AppException => (StatusCodes.Status400BadRequest, exception.Message),
                _ => (StatusCodes.Status500InternalServerError, exception.Message)
            };

            var response = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
                TimeStamp = DateTime.UtcNow,
                Path = httpContext.Request.Path
            };

            await httpContext.Response.WriteAsJsonAsync(response);

            return true;
        }
    }
}
