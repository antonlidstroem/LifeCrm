using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Contacts.Commands;
using LifeCrm.Application.Contacts.DTOs;
using LifeCrm.Application.Contacts.Queries;
using LifeCrm.Application.Donations.DTOs;
using LifeCrm.Application.Donations.Queries;
using LifeCrm.Application.Interactions.DTOs;
using LifeCrm.Application.Interactions.Queries;
using LifeCrm.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeCrm.Api.Controllers.v1
{
    public class ContactsController : ApiControllerBase
    {
        private readonly ICsvService _csv;
        public ContactsController(ICsvService csv) { _csv = csv; }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ContactListDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams paging, CancellationToken ct)
            => OkResponse(await Mediator.Send(new GetContactsQuery(paging), ct));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
            => OkResponse(await Mediator.Send(new GetContactByIdQuery(id), ct));

        [HttpGet("{id:guid}/donations")]
        public async Task<IActionResult> GetDonations(Guid id, [FromQuery] PaginationParams paging, CancellationToken ct)
            => OkResponse(await Mediator.Send(new GetDonationsQuery(paging, id), ct));

        [HttpGet("{id:guid}/interactions")]
        public async Task<IActionResult> GetInteractions(Guid id, CancellationToken ct)
            => OkResponse(await Mediator.Send(new GetInteractionsByContactQuery(id), ct));

        [HttpPost]
        [Authorize(Policy = "CanWrite")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateContactRequest request, CancellationToken ct)
        {
            var id = await Mediator.Send(new CreateContactCommand(request), ct);
            return CreatedResponse(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "CanWrite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactRequest request, CancellationToken ct)
        {
            await Mediator.Send(new UpdateContactCommand(request with { Id = id }), ct);
            return NoContentResponse();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await Mediator.Send(new DeleteContactCommand(id), ct);
            return NoContentResponse();
        }

        [HttpGet("export")]
        [Authorize(Policy = "CanWrite")]
        [Produces("text/csv")]
        public async Task<IActionResult> Export(CancellationToken ct)
        {
            var contacts = await Mediator.Send(new GetContactsQuery(new PaginationParams { PageSize = 10000 }), ct);
            var rows = contacts.Items.Select(c => new ContactCsvRow
                { Name = c.Name, Type = c.Type.ToString(), Email = c.Email, Phone = c.Phone, Tags = c.Tags });
            var csvBytes = await _csv.ExportAsync(rows);
            return File(csvBytes, "text/csv", $"contacts-{DateTime.Today:yyyy-MM-dd}.csv");
        }

        [HttpPost("import")]
        [Authorize(Policy = "CanWrite")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [ProducesResponseType(typeof(ApiResponse<ImportContactsResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
        {
            if (file is null || file.Length == 0) return BadRequest(ApiResponse.Fail("Please upload a CSV file."));
            if (file.Length > 5 * 1024 * 1024)   return BadRequest(ApiResponse.Fail("File exceeds 5 MB limit."));
            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest(ApiResponse.Fail("Only .csv files are accepted."));
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            var result = await Mediator.Send(new ImportContactsCommand(ms.ToArray()), ct);
            return OkResponse(result, $"Import complete: {result.SuccessCount} contacts added.");
        }
    }
}
