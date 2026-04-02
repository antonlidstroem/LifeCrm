using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Projects.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Projects.Queries
{
    public class GetProjectsQuery : IRequest<PagedResult<ProjectListDto>>
    {
        public PaginationParams Params { get; }
        public GetProjectsQuery(PaginationParams p) { Params = p; }
    }

    public sealed class GetProjectsHandler : IRequestHandler<GetProjectsQuery, PagedResult<ProjectListDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetProjectsHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<PagedResult<ProjectListDto>> Handle(GetProjectsQuery q, CancellationToken ct)
        {
            var p = q.Params;
            var query = _uow.Projects.Query();
            if (!string.IsNullOrWhiteSpace(p.Search))
            { var t = p.Search.ToLower(); query = query.Where(x => x.Name.ToLower().Contains(t) || (x.Location != null && x.Location.ToLower().Contains(t))); }
            query = p.SortAscending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
            var total = await query.CountAsync(ct);
            var items = await query.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize)
                .Select(x => new ProjectListDto
                {
                    Id = x.Id, Name = x.Name, Status = x.Status, Location = x.Location, BudgetGoal = x.BudgetGoal,
                    TotalFunded = x.Donations.Where(d => !d.IsDeleted).Sum(d => (decimal?)d.Amount) ?? 0,
                    StartDate = x.StartDate, EndDate = x.EndDate
                }).ToListAsync(ct);
            return new PagedResult<ProjectListDto> { Items = items, Page = p.Page, PageSize = p.PageSize, TotalCount = total };
        }
    }

    public class GetProjectByIdQuery : IRequest<ProjectDto>
    {
        public Guid ProjectId { get; }
        public GetProjectByIdQuery(Guid id) { ProjectId = id; }
    }

    public sealed class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto>
    {
        private readonly IUnitOfWork _uow;
        public GetProjectByIdHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<ProjectDto> Handle(GetProjectByIdQuery q, CancellationToken ct)
        {
            var p = await _uow.Projects.GetByIdAsync(q.ProjectId, ct)
                ?? throw new NotFoundException(nameof(Project), q.ProjectId);
            var funded = await _uow.Donations.Query().Where(d => d.ProjectId == p.Id).SumAsync(d => (decimal?)d.Amount, ct) ?? 0;
            var dc = await _uow.Donations.CountAsync(d => d.ProjectId == p.Id, ct);
            var ic = await _uow.Interactions.CountAsync(i => i.ProjectId == p.Id, ct);
            return new ProjectDto
            {
                Id = p.Id, Name = p.Name, Description = p.Description, Status = p.Status,
                Location = p.Location, BudgetGoal = p.BudgetGoal, TotalFunded = funded,
                StartDate = p.StartDate, EndDate = p.EndDate, Notes = p.Notes,
                DonationCount = dc, InteractionCount = ic, CreatedAt = p.CreatedAt, LastModifiedAt = p.LastModifiedAt
            };
        }
    }
}
