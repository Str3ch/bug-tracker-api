using BugTracker.Api.DTOs.Errors;
using System.Net;
using System.Text.Json;

namespace BugTracker.Api.Middleware
{
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
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}",
                context.TraceIdentifier);

            var response = new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "An unexpected error occurred.",
                TraceId = context.TraceIdentifier
            };

            context.Response.StatusCode =  StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}