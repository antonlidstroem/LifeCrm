using System.Net;
using System.Text.Json;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        { _next = next; _logger = logger; _env = env; }

        public async Task InvokeAsync(HttpContext context)
        {
            try { await _next(context); }
            catch (Exception ex) { await HandleExceptionAsync(context, ex); }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            int statusCode;
            IEnumerable<string> errors;

            switch (exception)
            {
                case NotFoundException notFound:
                    statusCode = (int)HttpStatusCode.NotFound;
                    errors = new[] { notFound.Message };
                    _logger.LogWarning("Not found: {Message}", notFound.Message);
                    break;
                case ValidationException validation:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    errors = validation.Errors.Select(e => $"{e.Field}: {e.Message}");
                    _logger.LogWarning("Validation failed: {Errors}", string.Join("; ", errors));
                    break;
                case ForbiddenException forbidden:
                    statusCode = (int)HttpStatusCode.Forbidden;
                    errors = new[] { forbidden.Message };
                    _logger.LogWarning("Forbidden: {Message}", forbidden.Message);
                    break;
                case ConflictException conflict:
                    statusCode = (int)HttpStatusCode.Conflict;
                    errors = new[] { conflict.Message };
                    _logger.LogWarning("Conflict: {Message}", conflict.Message);
                    break;
                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    errors = _env.IsDevelopment()
                        ? new[] { exception.Message, exception.StackTrace ?? string.Empty }
                        : new[] { "An unexpected error occurred. Please try again." };
                    _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = statusCode;
            var response = ApiResponse.Fail(errors);
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}
