using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Interactions.Commands;
using LifeCrm.Application.Interactions.DTOs;
using LifeCrm.Application.Interactions.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeCrm.Api.Controllers.v1
{
    public class InteractionsController : ApiControllerBase
    {
        // FIXED: Added GetById so the edit dialog can pre-fill interaction data
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InteractionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
            => OkResponse(await Mediator.Send(new GetInteractionByIdQuery(id), ct));

        [HttpPost]
        [Authorize(Policy = "CanWrite")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateInteractionRequest request, CancellationToken ct)
        {
            var id = await Mediator.Send(new CreateInteractionCommand(request), ct);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<Guid>.Ok(id));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "CanWrite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInteractionRequest request, CancellationToken ct)
        {
            await Mediator.Send(new UpdateInteractionCommand(request with { Id = id }), ct);
            return NoContentResponse();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "CanWrite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await Mediator.Send(new DeleteInteractionCommand(id), ct);
            return NoContentResponse();
        }
    }
}
