using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Newsletters.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Application.Newsletters.Commands
{
    // ── Create Draft ──────────────────────────────────────────────────────────

    public class CreateNewsletterCommand : IRequest<Guid>
    {
        public CreateNewsletterRequest Request { get; }
        public CreateNewsletterCommand(CreateNewsletterRequest r) { Request = r; }
    }

    public sealed class CreateNewsletterHandler : IRequestHandler<CreateNewsletterCommand, Guid>
    {
        private readonly IUnitOfWork         _uow;
        private readonly ICurrentUserService _cu;
        public CreateNewsletterHandler(IUnitOfWork uow, ICurrentUserService cu) { _uow = uow; _cu = cu; }

        public async Task<Guid> Handle(CreateNewsletterCommand cmd, CancellationToken ct)
        {
            var orgId = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
            var nl = new Newsletter
            {
                Id             = Guid.NewGuid(),
                OrganizationId = orgId,
                Title          = cmd.Request.Title.Trim(),
                Subject        = cmd.Request.Subject.Trim(),
                HtmlBody       = cmd.Request.HtmlBody,
                Status         = NewsletterStatus.Draft,
                CreatedBy      = _cu.UserId?.ToString() ?? "system"
            };
            await _uow.Newsletters.AddAsync(nl, ct);
            await _uow.SaveChangesAsync(ct);
            return nl.Id;
        }
    }

    // ── Update Draft ──────────────────────────────────────────────────────────

    public class UpdateNewsletterCommand : IRequest<Unit>
    {
        public UpdateNewsletterRequest Request { get; }
        public UpdateNewsletterCommand(UpdateNewsletterRequest r) { Request = r; }
    }

    public sealed class UpdateNewsletterHandler : IRequestHandler<UpdateNewsletterCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        public UpdateNewsletterHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<Unit> Handle(UpdateNewsletterCommand cmd, CancellationToken ct)
        {
            var nl = await _uow.Newsletters.GetByIdAsync(cmd.Request.Id, ct)
                ?? throw new NotFoundException(nameof(Newsletter), cmd.Request.Id);

            if (nl.Status == NewsletterStatus.Sent)
                throw new ConflictException("Cannot edit a newsletter that has already been sent.");

            nl.Title    = cmd.Request.Title.Trim();
            nl.Subject  = cmd.Request.Subject.Trim();
            nl.HtmlBody = cmd.Request.HtmlBody;
            _uow.Newsletters.Update(nl);
            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }

    // ── Delete Draft ──────────────────────────────────────────────────────────

    public class DeleteNewsletterCommand : IRequest<Unit>
    {
        public Guid NewsletterId { get; }
        public DeleteNewsletterCommand(Guid id) { NewsletterId = id; }
    }

    public sealed class DeleteNewsletterHandler : IRequestHandler<DeleteNewsletterCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        public DeleteNewsletterHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<Unit> Handle(DeleteNewsletterCommand cmd, CancellationToken ct)
        {
            var nl = await _uow.Newsletters.GetByIdAsync(cmd.NewsletterId, ct)
                ?? throw new NotFoundException(nameof(Newsletter), cmd.NewsletterId);

            if (nl.Status == NewsletterStatus.Sent)
                throw new ConflictException("Cannot delete a newsletter that has already been sent.");

            _uow.Newsletters.Delete(nl);
            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }

    // ── Preview recipients ────────────────────────────────────────────────────

    public class PreviewNewsletterRecipientsCommand : IRequest<NewsletterPreviewDto>
    {
        public string? TagFilter         { get; }
        public string? ContactTypeFilter { get; }
        public PreviewNewsletterRecipientsCommand(string? tagFilter, string? contactTypeFilter)
        { TagFilter = tagFilter; ContactTypeFilter = contactTypeFilter; }
    }

    public sealed class PreviewNewsletterRecipientsHandler
        : IRequestHandler<PreviewNewsletterRecipientsCommand, NewsletterPreviewDto>
    {
        private readonly IUnitOfWork         _uow;
        private readonly ICurrentUserService _cu;
        public PreviewNewsletterRecipientsHandler(IUnitOfWork uow, ICurrentUserService cu)
        { _uow = uow; _cu = cu; }

        public async Task<NewsletterPreviewDto> Handle(
            PreviewNewsletterRecipientsCommand cmd, CancellationToken ct)
        {
            _ = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
            var q        = BuildRecipientQuery(_uow, cmd.TagFilter, cmd.ContactTypeFilter);
            var total    = await q.CountAsync(ct);
            var optedOut = await q.CountAsync(c => c.EmailOptOut, ct);
            var noEmail  = await q.CountAsync(
                c => !c.EmailOptOut && (c.Email == null || c.Email == string.Empty), ct);

            return new NewsletterPreviewDto
            {
                EligibleCount = Math.Max(0, total - optedOut - noEmail),
                OptedOutCount = optedOut,
                NoEmailCount  = noEmail
            };
        }

        /// <summary>
        /// Shared between Preview and Send so filtering logic is defined exactly once.
        /// </summary>
        internal static IQueryable<Contact> BuildRecipientQuery(
            IUnitOfWork uow, string? tagFilter, string? contactTypeFilter)
        {
            var q = uow.Contacts.Query();

            if (!string.IsNullOrWhiteSpace(tagFilter))
            {
                foreach (var tag in tagFilter
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(t => t.ToLower()))
                {
                    var captured = tag;
                    q = q.Where(c => c.Tags != null && c.Tags.ToLower().Contains(captured));
                }
            }

            if (!string.IsNullOrWhiteSpace(contactTypeFilter))
            {
                var types = contactTypeFilter
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(t => Enum.TryParse<ContactType>(t, true, out var parsed)
                        ? parsed : (ContactType?)null)
                    .Where(t => t.HasValue)
                    .Select(t => t!.Value)
                    .ToList();

                if (types.Count > 0)
                    q = q.Where(c => types.Contains(c.Type));
            }

            return q;
        }
    }

    // ── Send ──────────────────────────────────────────────────────────────────

    public class SendNewsletterCommand : IRequest<NewsletterSendResultDto>
    {
        public Guid                  NewsletterId { get; }
        public SendNewsletterRequest Request      { get; }
        public SendNewsletterCommand(Guid id, SendNewsletterRequest r) { NewsletterId = id; Request = r; }
    }

    public sealed class SendNewsletterHandler : IRequestHandler<SendNewsletterCommand, NewsletterSendResultDto>
    {
        private readonly IUnitOfWork         _uow;
        private readonly IEmailService       _email;
        private readonly ICurrentUserService _cu;
        private readonly IConfiguration      _config;
        private readonly ILogger<SendNewsletterHandler> _logger;

        public SendNewsletterHandler(IUnitOfWork uow, IEmailService email,
            ICurrentUserService cu, IConfiguration config, ILogger<SendNewsletterHandler> logger)
        { _uow = uow; _email = email; _cu = cu; _config = config; _logger = logger; }

        public async Task<NewsletterSendResultDto> Handle(
            SendNewsletterCommand cmd, CancellationToken ct)
        {
            // Load newsletter with its attachments in one query
            var nl = await _uow.Newsletters.Query()
                .Include(n => n.Attachments.Where(a => !a.IsDeleted))
                .FirstOrDefaultAsync(n => n.Id == cmd.NewsletterId, ct)
                ?? throw new NotFoundException(nameof(Newsletter), cmd.NewsletterId);

            if (nl.Status == NewsletterStatus.Sent)
                throw new ConflictException("This newsletter has already been sent.");

            if (string.IsNullOrWhiteSpace(nl.HtmlBody))
                throw new ValidationException("HtmlBody",
                    "Cannot send a newsletter with an empty body.");

            var req = cmd.Request;

            // Build Phase-2 attachment list once — reused for every recipient
            var attachments = nl.Attachments
                .Select(a => new EmailAttachment(a.FileBytes, a.FileName, a.ContentType))
                .ToList();

            var recipients = await PreviewNewsletterRecipientsHandler
                .BuildRecipientQuery(_uow, req.TagFilter, req.ContactTypeFilter)
                .Where(c => !c.EmailOptOut &&
                            c.Email != null && c.Email != string.Empty)
                .Select(c => new { c.Id, c.Name, c.Email })
                .ToListAsync(ct);

            int sentCount  = 0;
            int errorCount = 0;
            var errors     = new List<string>();

            foreach (var r in recipients)
            {
                try
                {
                    // Phase 4: personalise unsubscribe link per recipient
                    var secretKey   = _config.GetSection("Jwt")["SecretKey"] ?? string.Empty;
                    var appBaseUrl  = (_config["AppBaseUrl"] ?? string.Empty).TrimEnd('/');
                    var token       = LifeCrm.Api.Controllers.v1.UnsubscribeController
                        .GenerateToken(r.Id, nl.OrganizationId, secretKey);
                    var unsubUrl    = $"{appBaseUrl}/api/v1/unsubscribe?token={token}";
                    var footer      = "<br/><hr style=\"margin:32px 0;border:none;border-top:1px solid #ddd\"/>"
                                    + "<p style=\"font-size:11px;color:#999;text-align:center\">"
                                    + "Du f&#229;r detta e-postmeddelande fr&#229;n en organisation du &#228;r registrerad hos.<br/>"
                                    + $"<a href=\"{unsubUrl}\" style=\"color:#999\">Avregistrera dig fr&#229;n framtida utskick</a></p>";
                    var body        = nl.HtmlBody + footer;

                    await _email.SendAsync(
                        r.Email!, r.Name, nl.Subject, body,
                        attachments.Count > 0 ? attachments : null,
                        ct);
                    sentCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    errors.Add($"{r.Name} <{r.Email}>: {ex.Message}");
                    _logger.LogWarning(
                        "Newsletter {Id} send failed for {Email}: {Err}",
                        nl.Id, r.Email, ex.Message);
                }
            }

            nl.Status            = NewsletterStatus.Sent;
            nl.SentAt            = DateTimeOffset.UtcNow;
            nl.SentBy            = _cu.UserId?.ToString();
            nl.TagFilter         = req.TagFilter;
            nl.ContactTypeFilter = req.ContactTypeFilter;
            nl.SentCount         = sentCount;
            nl.SkippedCount      = 0;
            nl.ErrorCount        = errorCount;
            _uow.Newsletters.Update(nl);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Newsletter '{Subject}' sent: {Sent} ok, {Err} failed, {Att} attachments",
                nl.Subject, sentCount, errorCount, attachments.Count);

            return new NewsletterSendResultDto
            {
                SentCount    = sentCount,
                SkippedCount = 0,
                ErrorCount   = errorCount,
                Errors       = errors.AsReadOnly()
            };
        }
    }
}
