using UmarahBooking.Core.Models;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;
using UmarahBooking.Core.DTO;

namespace UmarahBooking.Middleware
{
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
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // DEBUGGING: Return actual exception message
            // In production, you'd want to be careful, but we need this to debug the 500 error.
            var message = $"GLOBAL_HANDLER_ERROR: {exception.Message}";
            if (exception.InnerException != null)
            {
                message += $" | Inner: {exception.InnerException.Message}";
            }

            var response = ApiResponse<string>.ErrorResponse(message);
            // response.Errors = new List<string> { exception.ToString() }; // Full stack trace if needed

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}

