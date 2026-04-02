using LifeCrm.Application.Campaigns.Commands;
using LifeCrm.Application.Campaigns.DTOs;
using LifeCrm.Application.Campaigns.Queries;
using LifeCrm.Application.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeCrm.Api.Controllers.v1
{
    public class CampaignsController : ApiControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CampaignListDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams paging, CancellationToken ct)
            => OkResponse(await Mediator.Send(new GetCampaignsQuery(paging), ct));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CampaignDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
            => OkResponse(await Mediator.Send(new GetCampaignByIdQuery(id), ct));

        [HttpPost]
        [Authorize(Policy = "CanWrite")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateCampaignRequest request, CancellationToken ct)
        {
            var id = await Mediator.Send(new CreateCampaignCommand(request), ct);
            return CreatedResponse(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "CanWrite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCampaignRequest request, CancellationToken ct)
        {
            await Mediator.Send(new UpdateCampaignCommand(request with { Id = id }), ct);
            return NoContentResponse();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await Mediator.Send(new DeleteCampaignCommand(id), ct);
            return NoContentResponse();
        }
    }
}
