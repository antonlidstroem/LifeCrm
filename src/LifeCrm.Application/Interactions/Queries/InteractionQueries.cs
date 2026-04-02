using LifeCrm.Application.Interactions.DTOs;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Interactions.Queries
{
    public class GetInteractionsByContactQuery : IRequest<IReadOnlyList<InteractionDto>>
    {
        public Guid ContactId { get; }
        public GetInteractionsByContactQuery(Guid id) { ContactId = id; }
    }

    public sealed class GetInteractionsByContactHandler : IRequestHandler<GetInteractionsByContactQuery, IReadOnlyList<InteractionDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetInteractionsByContactHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<IReadOnlyList<InteractionDto>> Handle(GetInteractionsByContactQuery q, CancellationToken ct)
        {
            var list = await _uow.InteractionRepo.GetByContactAsync(q.ContactId, ct);
            return list.Select(i => new InteractionDto
            {
                Id = i.Id, Type = i.Type, Body = i.Body, Subject = i.Subject, OccurredAt = i.OccurredAt,
                ContactId = i.ContactId, ContactName = i.Contact?.Name, ProjectId = i.ProjectId,
                ProjectName = i.Project?.Name, DueDate = i.DueDate, IsCompleted = i.IsCompleted,
                CreatedByName = i.CreatedBy, CreatedAt = i.CreatedAt
            }).ToList().AsReadOnly();
        }
    }

    public class GetInteractionsByProjectQuery : IRequest<IReadOnlyList<InteractionDto>>
    {
        public Guid ProjectId { get; }
        public GetInteractionsByProjectQuery(Guid id) { ProjectId = id; }
    }

    public sealed class GetInteractionsByProjectHandler : IRequestHandler<GetInteractionsByProjectQuery, IReadOnlyList<InteractionDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetInteractionsByProjectHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<IReadOnlyList<InteractionDto>> Handle(GetInteractionsByProjectQuery q, CancellationToken ct)
        {
            var list = await _uow.InteractionRepo.GetByProjectAsync(q.ProjectId, ct);
            return list.Select(i => new InteractionDto
            {
                Id = i.Id, Type = i.Type, Body = i.Body, Subject = i.Subject, OccurredAt = i.OccurredAt,
                ContactId = i.ContactId, ContactName = i.Contact?.Name, ProjectId = i.ProjectId,
                ProjectName = i.Project?.Name, DueDate = i.DueDate, IsCompleted = i.IsCompleted,
                CreatedByName = i.CreatedBy, CreatedAt = i.CreatedAt
            }).ToList().AsReadOnly();
        }
    }
}
