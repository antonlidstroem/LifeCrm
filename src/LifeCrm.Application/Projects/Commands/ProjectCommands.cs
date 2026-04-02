using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Projects.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Projects.Commands
{
    public class CreateProjectCommand : IRequest<Guid>
    {
        public CreateProjectRequest Request { get; }
        public CreateProjectCommand(CreateProjectRequest r) { Request = r; }
    }

    public sealed class CreateProjectHandler : IRequestHandler<CreateProjectCommand, Guid>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _cu;
        public CreateProjectHandler(IUnitOfWork uow, ICurrentUserService cu) { _uow = uow; _cu = cu; }

        public async Task<Guid> Handle(CreateProjectCommand cmd, CancellationToken ct)
        {
            var orgId = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
            var p = new Project
            {
                Id = Guid.NewGuid(), OrganizationId = orgId,
                Name = cmd.Request.Name.Trim(), Description = cmd.Request.Description?.Trim(),
                Status = cmd.Request.Status, BudgetGoal = cmd.Request.BudgetGoal,
                StartDate = cmd.Request.StartDate, EndDate = cmd.Request.EndDate,
                Location = cmd.Request.Location?.Trim(), Notes = cmd.Request.Notes?.Trim()
            };
            await _uow.Projects.AddAsync(p, ct);
            await _uow.SaveChangesAsync(ct);
            return p.Id;
        }
    }

    public class UpdateProjectCommand : IRequest<Unit>
    {
        public UpdateProjectRequest Request { get; }
        public UpdateProjectCommand(UpdateProjectRequest r) { Request = r; }
    }

    public sealed class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        public UpdateProjectHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<Unit> Handle(UpdateProjectCommand cmd, CancellationToken ct)
        {
            var p = await _uow.Projects.GetByIdAsync(cmd.Request.Id, ct)
                ?? throw new NotFoundException(nameof(Project), cmd.Request.Id);
            p.Name = cmd.Request.Name.Trim(); p.Description = cmd.Request.Description?.Trim();
            p.Status = cmd.Request.Status; p.BudgetGoal = cmd.Request.BudgetGoal;
            p.StartDate = cmd.Request.StartDate; p.EndDate = cmd.Request.EndDate;
            p.Location = cmd.Request.Location?.Trim(); p.Notes = cmd.Request.Notes?.Trim();
            _uow.Projects.Update(p);
            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }

    public class DeleteProjectCommand : IRequest<Unit>
    {
        public Guid ProjectId { get; }
        public DeleteProjectCommand(Guid id) { ProjectId = id; }
    }

    public sealed class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        public DeleteProjectHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<Unit> Handle(DeleteProjectCommand cmd, CancellationToken ct)
        {
            var p = await _uow.Projects.GetByIdAsync(cmd.ProjectId, ct)
                ?? throw new NotFoundException(nameof(Project), cmd.ProjectId);
            _uow.Projects.Delete(p);
            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
