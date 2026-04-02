using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Documents.Commands;
using LifeCrm.Application.Documents.DTOs;
using LifeCrm.Core.Enums;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Api.Controllers.v1
{
    public class DocumentsController : ApiControllerBase
    {
        [HttpPost("summary")]
        [Authorize(Policy = "FinanceOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<DocumentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GenerateSummary([FromBody] GenerateDonationSummaryRequest request, CancellationToken ct)
            => OkResponse(await Mediator.Send(new GenerateDonationSummaryCommand(request), ct));

        [HttpGet("{id:guid}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Download(Guid id, [FromServices] AppDbContext context, CancellationToken ct)
        {
            var doc = await context.Documents.FirstOrDefaultAsync(d => d.Id == id, ct);
            if (doc is null) return NotFound();
            return File(doc.PdfBytes, "application/pdf", System.IO.Path.GetFileName(doc.FileName));
        }

        [HttpGet("donation/{donationId:guid}/receipt/latest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DownloadLatestReceipt(Guid donationId, [FromServices] AppDbContext context, CancellationToken ct)
        {
            var doc = await context.Documents
                .Where(d => d.DonationId == donationId && d.Type == DocumentType.DonationReceipt)
                .OrderByDescending(d => d.CreatedAt)
                .FirstOrDefaultAsync(ct);
            if (doc is null) return NotFound();
            return File(doc.PdfBytes, "application/pdf", System.IO.Path.GetFileName(doc.FileName));
        }
    }
}
