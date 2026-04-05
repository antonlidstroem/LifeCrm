using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Newsletters.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Newsletters.Commands
{
    // ── Upload attachment ──────────────────────────────────────────────────────

    public class AddNewsletterAttachmentCommand : IRequest<AttachmentDto>
    {
        public Guid   NewsletterId { get; }
        public string FileName     { get; }
        public string ContentType  { get; }
        public byte[] FileBytes    { get; }

        public AddNewsletterAttachmentCommand(
            Guid newslId, string fileName, string contentType, byte[] fileBytes)
        {
            NewsletterId = newslId;
            FileName     = fileName;
            ContentType  = contentType;
            FileBytes    = fileBytes;
        }
    }

    public sealed class AddNewsletterAttachmentHandler
        : IRequestHandler<AddNewsletterAttachmentCommand, AttachmentDto>
    {
        // Allowed MIME types and per-file size limits
        private static readonly Dictionary<string, long> AllowedTypes = new()
        {
            ["application/pdf"] = 10 * 1024 * 1024,   // PDF → 10 MB
            ["image/jpeg"]      =  3 * 1024 * 1024,   // JPEG → 3 MB
            ["image/png"]       =  3 * 1024 * 1024,   // PNG  → 3 MB
            ["image/gif"]       =  3 * 1024 * 1024,   // GIF  → 3 MB
            ["image/webp"]      =  3 * 1024 * 1024,   // WebP → 3 MB
        };
        private const int MaxImagesPerNewsletter = 3;

        private readonly IUnitOfWork         _uow;
        private readonly ICurrentUserService _cu;

        public AddNewsletterAttachmentHandler(IUnitOfWork uow, ICurrentUserService cu)
        { _uow = uow; _cu = cu; }

        public async Task<AttachmentDto> Handle(
            AddNewsletterAttachmentCommand cmd, CancellationToken ct)
        {
            var nl = await _uow.Newsletters.GetByIdAsync(cmd.NewsletterId, ct)
                ?? throw new NotFoundException(nameof(Newsletter), cmd.NewsletterId);

            if (nl.Status == NewsletterStatus.Sent)
                throw new ConflictException("Cannot add attachments to a sent newsletter.");

            // Validate MIME type
            if (!AllowedTypes.TryGetValue(cmd.ContentType.ToLower(), out var maxBytes))
                throw new ValidationException("ContentType",
                    "Only PDF and image files (JPEG, PNG, GIF, WebP) are allowed.");

            // Validate file size
            if (cmd.FileBytes.Length > maxBytes)
                throw new ValidationException("FileSize",
                    $"File exceeds the {maxBytes / 1024 / 1024} MB limit for {cmd.ContentType}.");

            // Validate image count limit
            if (cmd.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                var existingImages = await _uow.NewsletterAttachments
                    .CountAsync(a => a.NewsletterId == cmd.NewsletterId &&
                                     a.ContentType.StartsWith("image/"), ct);
                if (existingImages >= MaxImagesPerNewsletter)
                    throw new ConflictException(
                        $"Maximum {MaxImagesPerNewsletter} images allowed per newsletter.");
            }

            var orgId = _cu.OrganizationId ?? throw new ForbiddenException();
            var att = new NewsletterAttachment
            {
                Id             = Guid.NewGuid(),
                OrganizationId = orgId,
                NewsletterId   = cmd.NewsletterId,
                FileName       = cmd.FileName,
                ContentType    = cmd.ContentType.ToLower(),
                FileBytes      = cmd.FileBytes,
                FileSizeBytes  = cmd.FileBytes.Length
            };

            await _uow.NewsletterAttachments.AddAsync(att, ct);
            await _uow.SaveChangesAsync(ct);

            return new AttachmentDto
            {
                Id            = att.Id,
                FileName      = att.FileName,
                ContentType   = att.ContentType,
                FileSizeBytes = att.FileSizeBytes
            };
        }
    }

    // ── Delete attachment ─────────────────────────────────────────────────────

    public class DeleteNewsletterAttachmentCommand : IRequest<Unit>
    {
        public Guid NewsletterId  { get; }
        public Guid AttachmentId  { get; }

        public DeleteNewsletterAttachmentCommand(Guid newslId, Guid attId)
        { NewsletterId = newslId; AttachmentId = attId; }
    }

    public sealed class DeleteNewsletterAttachmentHandler
        : IRequestHandler<DeleteNewsletterAttachmentCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        public DeleteNewsletterAttachmentHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<Unit> Handle(
            DeleteNewsletterAttachmentCommand cmd, CancellationToken ct)
        {
            var att = await _uow.NewsletterAttachments.GetByIdAsync(cmd.AttachmentId, ct)
                ?? throw new NotFoundException(nameof(NewsletterAttachment), cmd.AttachmentId);

            if (att.NewsletterId != cmd.NewsletterId)
                throw new ForbiddenException("Attachment does not belong to this newsletter.");

            var nl = await _uow.Newsletters.GetByIdAsync(cmd.NewsletterId, ct);
            if (nl?.Status == NewsletterStatus.Sent)
                throw new ConflictException("Cannot remove attachments from a sent newsletter.");

            _uow.NewsletterAttachments.Delete(att);
            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
