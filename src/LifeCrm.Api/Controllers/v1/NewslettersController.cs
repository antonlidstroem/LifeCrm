using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Newsletters.Commands;
using LifeCrm.Application.Newsletters.DTOs;
using LifeCrm.Application.Newsletters.Queries;
using LifeCrm.Core.Enums;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Api.Controllers.v1
{
    public class NewslettersController : ApiControllerBase
    {
        // ── List ──────────────────────────────────────────────────────────────

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<NewsletterListDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationParams paging,
            [FromQuery] NewsletterStatus? status,
            CancellationToken ct)
            => OkResponse(await Mediator.Send(new GetNewslettersQuery(paging, status), ct));

        // ── Single ────────────────────────────────────────────────────────────

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NewsletterDetailDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
            => OkResponse(await Mediator.Send(new GetNewsletterByIdQuery(id), ct));

        // ── Create draft ──────────────────────────────────────────────────────

        [HttpPost]
        [Authorize(Policy = "CanWrite")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(
            [FromBody] CreateNewsletterRequest request, CancellationToken ct)
        {
            var id = await Mediator.Send(new CreateNewsletterCommand(request), ct);
            return CreatedResponse(nameof(GetById), new { id }, id);
        }

        // ── Update draft ──────────────────────────────────────────────────────

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "CanWrite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update(
            Guid id, [FromBody] UpdateNewsletterRequest request, CancellationToken ct)
        {
            await Mediator.Send(new UpdateNewsletterCommand(request with { Id = id }), ct);
            return NoContentResponse();
        }

        // ── Delete draft ──────────────────────────────────────────────────────

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "CanWrite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await Mediator.Send(new DeleteNewsletterCommand(id), ct);
            return NoContentResponse();
        }

        // ── Preview ───────────────────────────────────────────────────────────

        [HttpGet("recipients/preview")]
        [Authorize(Policy = "CanWrite")]
        [ProducesResponseType(typeof(ApiResponse<NewsletterPreviewDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> PreviewRecipients(
            [FromQuery] string? tagFilter,
            [FromQuery] string? contactTypeFilter,
            CancellationToken ct)
            => OkResponse(await Mediator.Send(
                new PreviewNewsletterRecipientsCommand(tagFilter, contactTypeFilter), ct));

        // ── Send ──────────────────────────────────────────────────────────────

        [HttpPost("{id:guid}/send")]
        [Authorize(Policy = "FinanceOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<NewsletterSendResultDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Send(
            Guid id, [FromBody] SendNewsletterRequest request, CancellationToken ct)
            => OkResponse(await Mediator.Send(new SendNewsletterCommand(id, request), ct));

        // ── Attachments (Phase 2) ─────────────────────────────────────────────

        /// <summary>Upload a PDF or image attachment. Max sizes: PDF 10 MB, image 3 MB.</summary>
        [HttpPost("{id:guid}/attachments")]
        [Authorize(Policy = "CanWrite")]
        [RequestSizeLimit(11 * 1024 * 1024)]   // hard cap at 11 MB (covers largest single file)
        [ProducesResponseType(typeof(ApiResponse<AttachmentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadAttachment(
            Guid id, IFormFile file, CancellationToken ct)
        {
            if (file is null || file.Length == 0)
                return BadRequest(ApiResponse.Fail("No file received."));

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);

            var result = await Mediator.Send(
                new AddNewsletterAttachmentCommand(
                    id,
                    file.FileName,
                    file.ContentType,
                    ms.ToArray()), ct);

            return OkResponse(result);
        }

        /// <summary>Delete an attachment from a draft newsletter.</summary>
        [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
        [Authorize(Policy = "CanWrite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteAttachment(
            Guid id, Guid attachmentId, CancellationToken ct)
        {
            await Mediator.Send(new DeleteNewsletterAttachmentCommand(id, attachmentId), ct);
            return NoContentResponse();
        }

        /// <summary>Download an attachment file (for draft preview or sent archive).</summary>
        [HttpGet("{id:guid}/attachments/{attachmentId:guid}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DownloadAttachment(
            Guid id, Guid attachmentId,
            [FromServices] AppDbContext context, CancellationToken ct)
        {
            var att = await context.NewsletterAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.NewsletterId == id, ct);
            if (att is null) return NotFound();
            return File(att.FileBytes, att.ContentType, att.FileName);
        }
    }
}
