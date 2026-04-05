using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Newsletters.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Newsletters.Queries
{
    // ── Paged list ────────────────────────────────────────────────────────────

    public class GetNewslettersQuery : IRequest<PagedResult<NewsletterListDto>>
    {
        public PaginationParams  Params       { get; }
        public NewsletterStatus? StatusFilter { get; }

        public GetNewslettersQuery(PaginationParams p, NewsletterStatus? status = null)
        { Params = p; StatusFilter = status; }
    }

    public sealed class GetNewslettersHandler
        : IRequestHandler<GetNewslettersQuery, PagedResult<NewsletterListDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetNewslettersHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<PagedResult<NewsletterListDto>> Handle(
            GetNewslettersQuery q, CancellationToken ct)
        {
            var p     = q.Params;
            var query = _uow.Newsletters.Query();

            if (q.StatusFilter.HasValue)
                query = query.Where(n => n.Status == q.StatusFilter.Value);

            if (!string.IsNullOrWhiteSpace(p.Search))
            {
                var term = p.Search.ToLower();
                query = query.Where(n =>
                    n.Title.ToLower().Contains(term) ||
                    n.Subject.ToLower().Contains(term));
            }

            query = query.OrderByDescending(n => n.SentAt ?? n.CreatedAt);

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((p.Page - 1) * p.PageSize)
                .Take(p.PageSize)
                .Select(n => new NewsletterListDto
                {
                    Id                = n.Id,
                    Title             = n.Title,
                    Subject           = n.Subject,
                    Status            = n.Status,
                    CreatedAt         = n.CreatedAt,
                    SentAt            = n.SentAt,
                    SentBy            = n.SentBy,
                    SentCount         = n.SentCount,
                    SkippedCount      = n.SkippedCount,
                    ErrorCount        = n.ErrorCount,
                    TagFilter         = n.TagFilter,
                    ContactTypeFilter = n.ContactTypeFilter,
                    AttachmentCount   = n.Attachments.Count(a => !a.IsDeleted)  // Phase 2
                })
                .ToListAsync(ct);

            return new PagedResult<NewsletterListDto>
            {
                Items = items, Page = p.Page,
                PageSize = p.PageSize, TotalCount = total
            };
        }
    }

    // ── Single ────────────────────────────────────────────────────────────────

    public class GetNewsletterByIdQuery : IRequest<NewsletterDetailDto>
    {
        public Guid NewsletterId { get; }
        public GetNewsletterByIdQuery(Guid id) { NewsletterId = id; }
    }

    public sealed class GetNewsletterByIdHandler
        : IRequestHandler<GetNewsletterByIdQuery, NewsletterDetailDto>
    {
        private readonly IUnitOfWork _uow;
        public GetNewsletterByIdHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<NewsletterDetailDto> Handle(
            GetNewsletterByIdQuery q, CancellationToken ct)
        {
            var n = await _uow.Newsletters.Query()
                .Include(n => n.Attachments.Where(a => !a.IsDeleted))   // Phase 2: eager-load
                .FirstOrDefaultAsync(n => n.Id == q.NewsletterId, ct)
                ?? throw new NotFoundException(nameof(Newsletter), q.NewsletterId);

            return new NewsletterDetailDto
            {
                Id                = n.Id,
                Title             = n.Title,
                Subject           = n.Subject,
                HtmlBody          = n.HtmlBody,
                Status            = n.Status,
                CreatedAt         = n.CreatedAt,
                SentAt            = n.SentAt,
                SentBy            = n.SentBy,
                SentCount         = n.SentCount,
                SkippedCount      = n.SkippedCount,
                ErrorCount        = n.ErrorCount,
                TagFilter         = n.TagFilter,
                ContactTypeFilter = n.ContactTypeFilter,
                AttachmentCount   = n.Attachments.Count,
                Attachments       = n.Attachments.Select(a => new AttachmentDto
                {
                    Id            = a.Id,
                    FileName      = a.FileName,
                    ContentType   = a.ContentType,
                    FileSizeBytes = a.FileSizeBytes
                }).ToList().AsReadOnly()
            };
        }
    }
}
