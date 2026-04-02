using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Users.DTOs;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Api.Controllers.v1
{
    [Authorize(Policy = "AdminOnly")]
    public class UsersController : ApiControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public UsersController(AppDbContext context, ICurrentUserService currentUser)
        { _context = context; _currentUser = currentUser; }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserSummaryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var orgId = _currentUser.OrganizationId ?? throw new ForbiddenException();
            var users = await _context.Users.IgnoreQueryFilters()
                .Where(u => u.OrganizationId == orgId && !u.IsDeleted)
                .OrderBy(u => u.FullName)
                .Select(u => new UserSummaryDto { Id = u.Id, FullName = u.FullName, Email = u.Email, Role = u.Role, IsActive = u.IsActive, LastLoginAt = u.LastLoginAt, CreatedAt = u.CreatedAt })
                .ToListAsync(ct);
            return OkResponse<IReadOnlyList<UserSummaryDto>>(users);
        }

        [HttpPatch("{id:guid}/role")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ChangeRole(Guid id, [FromBody] UserRole newRole, CancellationToken ct)
        {
            var orgId = _currentUser.OrganizationId ?? throw new ForbiddenException();
            if (id == _currentUser.UserId && newRole != UserRole.Admin)
                throw new ValidationException("Role", "You cannot remove your own Admin role.");
            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId, ct)
                ?? throw new NotFoundException("User", id);
            user.Role = newRole;
            await _context.SaveChangesAsync(ct);
            return NoContentResponse();
        }

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            if (id == _currentUser.UserId) throw new ValidationException("Id", "You cannot deactivate your own account.");
            var orgId = _currentUser.OrganizationId ?? throw new ForbiddenException();
            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId, ct)
                ?? throw new NotFoundException("User", id);
            user.IsActive = false;
            await _context.SaveChangesAsync(ct);
            return NoContentResponse();
        }

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
        {
            var orgId = _currentUser.OrganizationId ?? throw new ForbiddenException();
            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId, ct)
                ?? throw new NotFoundException("User", id);
            user.IsActive = true;
            await _context.SaveChangesAsync(ct);
            return NoContentResponse();
        }
    }
}
