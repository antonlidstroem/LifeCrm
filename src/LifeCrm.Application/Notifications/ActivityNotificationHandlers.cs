using LifeCrm.Application.Contacts.Commands;
using LifeCrm.Application.Donations.Commands;
using LifeCrm.Application.Interactions.Commands;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Application.Notifications
{
    public sealed class DonationCreatedActivityHandler : INotificationHandler<DonationCreatedNotification>
    {
        private readonly IActivityNotifier _notifier;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<DonationCreatedActivityHandler> _logger;

        public DonationCreatedActivityHandler(IActivityNotifier notifier, ICurrentUserService currentUser,
            ILogger<DonationCreatedActivityHandler> logger)
        { _notifier = notifier; _currentUser = currentUser; _logger = logger; }

        public async Task Handle(DonationCreatedNotification notification, CancellationToken ct)
        {
            var orgId = _currentUser.OrganizationId;
            if (!orgId.HasValue) return;
            try { await _notifier.NotifyAsync(orgId.Value.ToString(), "DonationCreated", new { donationId = notification.DonationId }, ct); }
            catch (Exception ex) { _logger.LogError(ex, "SignalR emit failed for DonationCreated {Id}.", notification.DonationId); }
        }
    }

    public sealed class ContactCreatedActivityHandler : INotificationHandler<ContactCreatedNotification>
    {
        private readonly IActivityNotifier _notifier;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<ContactCreatedActivityHandler> _logger;

        public ContactCreatedActivityHandler(IActivityNotifier notifier, ICurrentUserService currentUser,
            ILogger<ContactCreatedActivityHandler> logger)
        { _notifier = notifier; _currentUser = currentUser; _logger = logger; }

        public async Task Handle(ContactCreatedNotification notification, CancellationToken ct)
        {
            var orgId = _currentUser.OrganizationId;
            if (!orgId.HasValue) return;
            try { await _notifier.NotifyAsync(orgId.Value.ToString(), "ContactCreated", new { contactId = notification.ContactId, name = notification.ContactName }, ct); }
            catch (Exception ex) { _logger.LogError(ex, "SignalR emit failed for ContactCreated {Id}.", notification.ContactId); }
        }
    }

    public sealed class InteractionCreatedActivityHandler : INotificationHandler<InteractionCreatedNotification>
    {
        private readonly IActivityNotifier _notifier;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<InteractionCreatedActivityHandler> _logger;

        public InteractionCreatedActivityHandler(IActivityNotifier notifier, ICurrentUserService currentUser,
            ILogger<InteractionCreatedActivityHandler> logger)
        { _notifier = notifier; _currentUser = currentUser; _logger = logger; }

        public async Task Handle(InteractionCreatedNotification notification, CancellationToken ct)
        {
            var orgId = _currentUser.OrganizationId;
            if (!orgId.HasValue) return;
            try { await _notifier.NotifyAsync(orgId.Value.ToString(), "InteractionLogged", new { interactionId = notification.InteractionId, contactId = notification.ContactId }, ct); }
            catch (Exception ex) { _logger.LogError(ex, "SignalR emit failed for InteractionLogged {Id}.", notification.InteractionId); }
        }
    }
}
