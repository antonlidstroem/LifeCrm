using System.Security.Claims;
using LifeCrm.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LifeCrm.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _http;

        public CurrentUserService(IHttpContextAccessor http) { _http = http; }

        private ClaimsPrincipal? User => _http.HttpContext?.User;

        public Guid? UserId
        {
            get
            {
                // Fix: Använd FindFirst(X)?.Value istället för FindFirstValue
                var val = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User?.FindFirst("sub")?.Value;

                return Guid.TryParse(val, out var id) ? id : null;
            }
        }

        public Guid? OrganizationId
        {
            get
            {
                // Fix: Samma här
                var val = User?.FindFirst("org_id")?.Value;
                return Guid.TryParse(val, out var id) ? id : null;
            }
        }

        // Fix: Och här
        public string? UserRole => User?.FindFirst(ClaimTypes.Role)?.Value;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
    }
}