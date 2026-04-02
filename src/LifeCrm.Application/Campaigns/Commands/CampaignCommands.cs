using LifeCrm.Application.Common.Behaviours;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Campaigns.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Campaigns.Commands
{
    public class CreateCampaignCommand : IRequest<Guid>
    {
        public CreateCampaignRequest Request { get; }
        public CreateCampaignCommand(CreateCampaignRequest r) { Request = r; }
    }

    public sealed class CreateCampaignHandler : IRequestHandler<CreateCampaignCommand, Guid>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _cu;
        public CreateCampaignHandler(IUnitOfWork uow, ICurrentUserService cu) { _uow = uow; _cu = cu; }

        public async Task<Guid> Handle(CreateCampaignCommand cmd, CancellationToken ct)
        {
            var orgId = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
            var c = new Campaign
            {
                Id = Guid.NewGuid(), OrganizationId = orgId,
                Name = cmd.Request.Name.Trim(), Description = cmd.Request.Description?.Trim(),
                BudgetGoal = cmd.Request.BudgetGoal, StartDate = cmd.Request.StartDate,
                EndDate = cmd.Request.EndDate, Status = cmd.Request.Status, Notes = cmd.Request.Notes?.Trim()
            };
            await _uow.Campaigns.AddAsync(c, ct);
            await _uow.SaveChangesAsync(ct);
            return c.Id;
        }
    }

    public class UpdateCampaignCommand : IRequest<Unit>
    {
        public UpdateCampaignRequest Request { get; }
        public UpdateCampaignCommand(UpdateCampaignRequest r) { Request = r; }
    }

    public sealed class UpdateCampaignHandler : IRequestHandler<UpdateCampaignCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        public UpdateCampaignHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<Unit> Handle(UpdateCampaignCommand cmd, CancellationToken ct)
        {
            var c = await _uow.Campaigns.GetByIdAsync(cmd.Request.Id, ct)
                ?? throw new NotFoundException(nameof(Campaign), cmd.Request.Id);
            c.Name = cmd.Request.Name.Trim(); c.Description = cmd.Request.Description?.Trim();
            c.BudgetGoal = cmd.Request.BudgetGoal; c.StartDate = cmd.Request.StartDate;
            c.EndDate = cmd.Request.EndDate; c.Status = cmd.Request.Status; c.Notes = cmd.Request.Notes?.Trim();
            _uow.Campaigns.Update(c);
            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }

    public class DeleteCampaignCommand : IRequest<Unit>
    {
        public Guid CampaignId { get; }
        public DeleteCampaignCommand(Guid id) { CampaignId = id; }
    }

    public sealed class DeleteCampaignHandler : IRequestHandler<DeleteCampaignCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        public DeleteCampaignHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<Unit> Handle(DeleteCampaignCommand cmd, CancellationToken ct)
        {
            var c = await _uow.Campaigns.GetByIdAsync(cmd.CampaignId, ct)
                ?? throw new NotFoundException(nameof(Campaign), cmd.CampaignId);
            _uow.Campaigns.Delete(c);
            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
