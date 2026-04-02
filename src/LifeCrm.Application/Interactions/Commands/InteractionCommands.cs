using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Interactions.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Interactions.Commands
{
    public sealed record InteractionCreatedNotification(Guid InteractionId, Guid? ContactId) : INotification;

    public sealed class CreateInteractionCommand : IRequest<Guid>
    {
        public CreateInteractionRequest Request { get; }
        public CreateInteractionCommand(CreateInteractionRequest r) { Request = r; }
    }

    public sealed class CreateInteractionHandler : IRequestHandler<CreateInteractionCommand, Guid>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _cu;
        private readonly IMediator _mediator;

        public CreateInteractionHandler(IUnitOfWork uow, ICurrentUserService cu, IMediator mediator)
        { _uow = uow; _cu = cu; _mediator = mediator; }

        public async Task<Guid> Handle(CreateInteractionCommand cmd, CancellationToken ct)
        {
            var orgId = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
            var r = cmd.Request;
            if (!r.ContactId.HasValue && !r.ProjectId.HasValue)
                throw new ValidationException("ContactId", "An interaction must be linked to a contact or a project.");
            var interaction = new Interaction
            {
                Id = Guid.NewGuid(), OrganizationId = orgId, Type = r.Type,
                Body = r.Body.Trim(), Subject = r.Subject?.Trim(), OccurredAt = r.OccurredAt,
                ContactId = r.ContactId, ProjectId = r.ProjectId, DueDate = r.DueDate, IsCompleted = r.IsCompleted
            };
            await _uow.Interactions.AddAsync(interaction, ct);
            await _uow.SaveChangesAsync(ct);
            await _mediator.Publish(new InteractionCreatedNotification(interaction.Id, interaction.ContactId), ct);
            return interaction.Id;
        }
    }

    public sealed class UpdateInteractionCommand : IRequest<Unit>
    {
        public UpdateInteractionRequest Request { get; }
        public UpdateInteractionCommand(UpdateInteractionRequest r) { Request = r; }
    }

    public sealed class UpdateInteractionHandler : IRequestHandler<UpdateInteractionCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        public UpdateInteractionHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<Unit> Handle(UpdateInteractionCommand cmd, CancellationToken ct)
        {
            var req = cmd.Request;
            var i = await _uow.Interactions.GetByIdAsync(req.Id, ct)
                ?? throw new NotFoundException(nameof(Interaction), req.Id);
            if (!req.ContactId.HasValue && !req.ProjectId.HasValue)
                throw new ValidationException("ContactId", "An interaction must be linked to a contact or a project.");
            i.Type = req.Type; i.Body = req.Body.Trim(); i.Subject = req.Subject?.Trim();
            i.OccurredAt = req.OccurredAt; i.ContactId = req.ContactId; i.ProjectId = req.ProjectId;
            i.DueDate = req.DueDate; i.IsCompleted = req.IsCompleted;
            _uow.Interactions.Update(i);
            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }

    public sealed class DeleteInteractionCommand : IRequest<Unit>
    {
        public Guid InteractionId { get; }
        public DeleteInteractionCommand(Guid id) { InteractionId = id; }
    }

    public sealed class DeleteInteractionHandler : IRequestHandler<DeleteInteractionCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        public DeleteInteractionHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<Unit> Handle(DeleteInteractionCommand cmd, CancellationToken ct)
        {
            var i = await _uow.Interactions.GetByIdAsync(cmd.InteractionId, ct)
                ?? throw new NotFoundException(nameof(Interaction), cmd.InteractionId);
            _uow.Interactions.Delete(i);
            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
