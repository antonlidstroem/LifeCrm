using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Interactions.DTOs;
using LifeCrm.Application.Interactions.Queries;
using LifeCrm.Application.Projects.Commands;
using LifeCrm.Application.Projects.DTOs;
using LifeCrm.Application.Projects.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeCrm.Api.Controllers.v1
{
    public class ProjectsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams paging, CancellationToken ct)
            => OkResponse(await Mediator.Send(new GetProjectsQuery(paging), ct));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
            => OkResponse(await Mediator.Send(new GetProjectByIdQuery(id), ct));

        // FIXED: Added interactions sub-resource endpoint so ProjectDetailPage can
        // load project interactions via ApiClient.GetProjectInteractionsAsync().
        [HttpGet("{id:guid}/interactions")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<InteractionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInteractions(Guid id, CancellationToken ct)
            => OkResponse(await Mediator.Send(new GetInteractionsByProjectQuery(id), ct));

        [HttpPost]
        [Authorize(Policy = "CanWrite")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateProjectRequest request, CancellationToken ct)
        {
            var id = await Mediator.Send(new CreateProjectCommand(request), ct);
            return CreatedResponse(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "CanWrite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken ct)
        {
            await Mediator.Send(new UpdateProjectCommand(request with { Id = id }), ct);
            return NoContentResponse();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await Mediator.Send(new DeleteProjectCommand(id), ct);
            return NoContentResponse();
        }
    }
}
