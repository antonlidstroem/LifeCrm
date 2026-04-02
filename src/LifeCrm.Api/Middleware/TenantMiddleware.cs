using LifeCrm.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Api.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        { _next = next; _logger = logger; }

        public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUser)
        {
            if (context.User.Identity?.IsAuthenticated == true && !currentUser.OrganizationId.HasValue)
                _logger.LogWarning("Authenticated user has no org_id claim. Path: {Path}", context.Request.Path);
            await _next(context);
        }
    }
}
