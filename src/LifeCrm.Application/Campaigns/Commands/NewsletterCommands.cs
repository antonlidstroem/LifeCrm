using LifeCrm.Application.Campaigns.DTOs;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Application.Campaigns.Commands
{
    // ── Send ─────────────────────────────────────────────────────────────────
    public class SendNewsletterCommand : IRequest<NewsletterResultDto>
    {
        public Guid                  CampaignId { get; }
        public SendNewsletterRequest Request    { get; }

        public SendNewsletterCommand(Guid campaignId, SendNewsletterRequest request)
        { CampaignId = campaignId; Request = request; }
    }

    public sealed class SendNewsletterHandler : IRequestHandler<SendNewsletterCommand, NewsletterResultDto>
    {
        private readonly IUnitOfWork         _uow;
        private readonly IEmailService       _email;
        private readonly ICurrentUserService _cu;
        private readonly ILogger<SendNewsletterHandler> _logger;

        public SendNewsletterHandler(
            IUnitOfWork uow, IEmailService email,
            ICurrentUserService cu, ILogger<SendNewsletterHandler> logger)
        { _uow = uow; _email = email; _cu = cu; _logger = logger; }

        public async Task<NewsletterResultDto> Handle(
            SendNewsletterCommand cmd, CancellationToken ct)
        {
            // Verify campaign exists and belongs to this org
            var campaign = await _uow.Campaigns.GetByIdAsync(cmd.CampaignId, ct)
                ?? throw new NotFoundException(nameof(Core.Entities.Campaign), cmd.CampaignId);

            var orgId = _cu.OrganizationId
                ?? throw new ForbiddenException("No organization context.");

            // Build recipient query: contacts with an email address, not opted out
            var query = _uow.Contacts.Query()
                .Where(c => c.Email != null && c.Email != string.Empty && !c.EmailOptOut);

            // Apply tag filter — contact Tags field is comma-separated free text.
            // We require the contact to have ALL requested tags as substrings.
            var req = cmd.Request;
            if (!string.IsNullOrWhiteSpace(req.TagFilter))
            {
                var tags = req.TagFilter
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(t => t.ToLower())
                    .ToList();

                foreach (var tag in tags)
                {
                    var captured = tag; // avoid closure capture of loop variable
                    query = query.Where(c => c.Tags != null && c.Tags.ToLower().Contains(captured));
                }
            }

            var recipients = await query
                .Select(c => new { c.Id, c.Name, c.Email })
                .ToListAsync(ct);

            int sentCount  = 0;
            int errorCount = 0;
            var errors     = new List<string>();

            foreach (var r in recipients)
            {
                try
                {
                    await _email.SendAsync(
                        toEmail:  r.Email!,
                        toName:   r.Name,
                        subject:  req.Subject,
                        htmlBody: req.HtmlBody,
                        ct:       ct);
                    sentCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    var msg = $"{r.Name} <{r.Email}>: {ex.Message}";
                    errors.Add(msg);
                    _logger.LogWarning(
                        "Newsletter send failed for contact {Id} ({Email}): {Error}",
                        r.Id, r.Email, ex.Message);
                }
            }

            _logger.LogInformation(
                "Newsletter '{Subject}' for campaign {CampaignId}: sent={Sent} errors={Errors}",
                req.Subject, cmd.CampaignId, sentCount, errorCount);

            return new NewsletterResultDto
            {
                SentCount    = sentCount,
                SkippedCount = 0,   // already excluded via query filter
                ErrorCount   = errorCount,
                Errors       = errors.AsReadOnly()
            };
        }
    }

    // ── Preview ───────────────────────────────────────────────────────────────
    public class PreviewNewsletterCommand : IRequest<NewsletterPreviewDto>
    {
        public Guid    CampaignId { get; }
        public string? TagFilter  { get; }

        public PreviewNewsletterCommand(Guid campaignId, string? tagFilter)
        { CampaignId = campaignId; TagFilter = tagFilter; }
    }

    public sealed class PreviewNewsletterHandler : IRequestHandler<PreviewNewsletterCommand, NewsletterPreviewDto>
    {
        private readonly IUnitOfWork         _uow;
        private readonly ICurrentUserService _cu;

        public PreviewNewsletterHandler(IUnitOfWork uow, ICurrentUserService cu)
        { _uow = uow; _cu = cu; }

        public async Task<NewsletterPreviewDto> Handle(
            PreviewNewsletterCommand cmd, CancellationToken ct)
        {
            _ = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");

            // All contacts (org-scoped via global filter)
            var allQuery = _uow.Contacts.Query();

            if (!string.IsNullOrWhiteSpace(cmd.TagFilter))
            {
                var tags = cmd.TagFilter
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(t => t.ToLower())
                    .ToList();
                foreach (var tag in tags)
                {
                    var captured = tag;
                    allQuery = allQuery.Where(c => c.Tags != null && c.Tags.ToLower().Contains(captured));
                }
            }

            var totalContacts = await allQuery.CountAsync(ct);
            var optedOut      = await allQuery.CountAsync(c => c.EmailOptOut, ct);
            var noEmail       = await allQuery.CountAsync(
                c => !c.EmailOptOut && (c.Email == null || c.Email == string.Empty), ct);
            var eligible      = totalContacts - optedOut - noEmail;

            return new NewsletterPreviewDto
            {
                EligibleCount = Math.Max(0, eligible),
                OptedOutCount = optedOut,
                NoEmailCount  = noEmail
            };
        }
    }
}
