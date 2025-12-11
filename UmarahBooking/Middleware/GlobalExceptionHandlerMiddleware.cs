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

            var response = ApiResponse<string>.ErrorResponse("An unexpected error occurred. Please try again later.");
            
            // In development, you might want to include the exception message or stack trace
            // response.Errors = new List<string> { exception.Message };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}

