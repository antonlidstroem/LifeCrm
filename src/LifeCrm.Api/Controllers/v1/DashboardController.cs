using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeCrm.Api.Controllers.v1
{
    public class DashboardController : ApiControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<DashboardDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(CancellationToken ct)
            => OkResponse(await Mediator.Send(new GetDashboardQuery(), ct));
    }
}
