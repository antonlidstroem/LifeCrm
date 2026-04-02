using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Documents.Commands;
using LifeCrm.Application.Documents.DTOs;
using LifeCrm.Application.Donations.Commands;
using LifeCrm.Application.Donations.DTOs;
using LifeCrm.Application.Donations.Queries;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Api.Controllers.v1
{
    public class DonationsController : ApiControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<DonationListDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams paging, [FromQuery] Guid? contactId, CancellationToken ct)
            => OkResponse(await Mediator.Send(new GetDonationsQuery(paging, contactId), ct));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
            => OkResponse(await Mediator.Send(new GetDonationByIdQuery(id), ct));

        [HttpPost]
        [Authorize(Policy = "FinanceOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateDonationRequest request, CancellationToken ct)
        {
            var id = await Mediator.Send(new CreateDonationCommand(request), ct);
            return CreatedResponse(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "FinanceOrAdmin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDonationRequest request, CancellationToken ct)
        {
            await Mediator.Send(new UpdateDonationCommand(request with { Id = id }), ct);
            return NoContentResponse();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await Mediator.Send(new DeleteDonationCommand(id), ct);
            return NoContentResponse();
        }

        [HttpPost("{id:guid}/receipt")]
        [Authorize(Policy = "FinanceOrAdmin")]
        public async Task<IActionResult> GenerateReceipt(Guid id, [FromQuery] bool sendByEmail = false, CancellationToken ct = default)
            => OkResponse(await Mediator.Send(new GenerateDonationReceiptCommand(new GenerateDonationReceiptRequest { DonationId = id, SendByEmail = sendByEmail }), ct));

        [HttpGet("{donationId:guid}/receipt/{documentId:guid}/download")]
        public async Task<IActionResult> DownloadReceipt(Guid donationId, Guid documentId, [FromServices] AppDbContext context, CancellationToken ct)
        {
            var doc = await context.Documents.FirstOrDefaultAsync(d => d.Id == documentId && d.DonationId == donationId, ct);
            if (doc is null) return NotFound();
            return File(doc.PdfBytes, "application/pdf", System.IO.Path.GetFileName(doc.FileName));
        }
    }
}
