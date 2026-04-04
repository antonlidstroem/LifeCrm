using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Interactions.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Interactions.Queries
{
    internal static class InteractionMapper
    {
        internal static InteractionDto Map(Interaction i) => new()
        {
            Id            = i.Id,
            Type          = i.Type,
            Body          = i.Body,
            Subject       = i.Subject,
            OccurredAt    = i.OccurredAt,
            ContactId     = i.ContactId,
            ContactName   = i.Contact?.Name,
            ProjectId     = i.ProjectId,
            ProjectName   = i.Project?.Name,
            DueDate       = i.DueDate,
            IsCompleted   = i.IsCompleted,
            CreatedByName = i.CreatedBy,
            CreatedAt     = i.CreatedAt
        };
    }

    // FIXED: Added GetInteractionByIdQuery so the edit dialog can load a single interaction
    public class GetInteractionByIdQuery : IRequest<InteractionDto>
    {
        public Guid InteractionId { get; }
        public GetInteractionByIdQuery(Guid id) { InteractionId = id; }
    }

    public sealed class GetInteractionByIdHandler
        : IRequestHandler<GetInteractionByIdQuery, InteractionDto>
    {
        private readonly IUnitOfWork _uow;
        public GetInteractionByIdHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<InteractionDto> Handle(
            GetInteractionByIdQuery q, CancellationToken ct)
        {
            var i = await _uow.Interactions.GetByIdAsync(q.InteractionId, ct)
                ?? throw new NotFoundException(nameof(Interaction), q.InteractionId);
            return InteractionMapper.Map(i);
        }
    }

    public class GetInteractionsByContactQuery : IRequest<IReadOnlyList<InteractionDto>>
    {
        public Guid ContactId { get; }
        public GetInteractionsByContactQuery(Guid id) { ContactId = id; }
    }

    public sealed class GetInteractionsByContactHandler
        : IRequestHandler<GetInteractionsByContactQuery, IReadOnlyList<InteractionDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetInteractionsByContactHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<IReadOnlyList<InteractionDto>> Handle(
            GetInteractionsByContactQuery q, CancellationToken ct)
        {
            var list = await _uow.InteractionRepo.GetByContactAsync(q.ContactId, ct);
            return list.Take(200).Select(InteractionMapper.Map).ToList().AsReadOnly();
        }
    }

    public class GetInteractionsByProjectQuery : IRequest<IReadOnlyList<InteractionDto>>
    {
        public Guid ProjectId { get; }
        public GetInteractionsByProjectQuery(Guid id) { ProjectId = id; }
    }

    public sealed class GetInteractionsByProjectHandler
        : IRequestHandler<GetInteractionsByProjectQuery, IReadOnlyList<InteractionDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetInteractionsByProjectHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<IReadOnlyList<InteractionDto>> Handle(
            GetInteractionsByProjectQuery q, CancellationToken ct)
        {
            var list = await _uow.InteractionRepo.GetByProjectAsync(q.ProjectId, ct);
            return list.Take(200).Select(InteractionMapper.Map).ToList().AsReadOnly();
        }
    }
}
