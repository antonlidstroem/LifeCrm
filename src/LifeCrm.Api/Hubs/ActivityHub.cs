using System.Security.Claims;
using LifeCrm.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Api.Hubs
{
    [Authorize]
    public class ActivityHub : Hub
    {
        public async Task JoinOrganization(string orgId)
        {
            var jwtOrgId = Context.User?.FindFirstValue("org_id");
            if (string.IsNullOrEmpty(jwtOrgId) || jwtOrgId != orgId)
                throw new HubException("Unauthorized: org_id mismatch.");
            await Groups.AddToGroupAsync(Context.ConnectionId, orgId);
        }

        public override async Task OnConnectedAsync()
        {
            var orgId = Context.User?.FindFirstValue("org_id");
            if (!string.IsNullOrEmpty(orgId))
                await Groups.AddToGroupAsync(Context.ConnectionId, orgId);
            await base.OnConnectedAsync();
        }
    }

    public sealed class ActivityNotifier : IActivityNotifier
    {
        private readonly IHubContext<ActivityHub> _hubContext;
        private readonly ILogger<ActivityNotifier> _logger;

        public ActivityNotifier(IHubContext<ActivityHub> hubContext, ILogger<ActivityNotifier> logger)
        { _hubContext = hubContext; _logger = logger; }

        public async Task NotifyAsync(string orgId, string eventType, object payload, CancellationToken ct = default)
        {
            try { await _hubContext.Clients.Group(orgId).SendAsync("activity", new { eventType, payload }, ct); }
            catch (Exception ex) { _logger.LogError(ex, "SignalR notification failed for org {OrgId} event {EventType}.", orgId, eventType); }
        }
    }
}
