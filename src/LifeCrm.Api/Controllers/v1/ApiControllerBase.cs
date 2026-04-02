using LifeCrm.Application.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace LifeCrm.Api.Controllers.v1
{
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public abstract class ApiControllerBase : ControllerBase
    {
        private ISender? _mediator;
        protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

        protected IActionResult OkResponse<T>(T data, string? message = null)
            => Ok(ApiResponse<T>.Ok(data, message));

        protected IActionResult CreatedResponse<T>(string actionName, object? routeValues, T data)
            => CreatedAtAction(actionName, routeValues, ApiResponse<T>.Ok(data));

        protected IActionResult NoContentResponse() => NoContent();

        protected IActionResult NotFoundResponse(string message) => NotFound(ApiResponse.Fail(message));
    }
}
