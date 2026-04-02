using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Campaigns.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Campaigns.Queries
{
    public class GetCampaignsQuery : IRequest<PagedResult<CampaignListDto>>
    {
        public PaginationParams Params { get; }
        public GetCampaignsQuery(PaginationParams p) { Params = p; }
    }

    public sealed class GetCampaignsHandler : IRequestHandler<GetCampaignsQuery, PagedResult<CampaignListDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetCampaignsHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<PagedResult<CampaignListDto>> Handle(GetCampaignsQuery q, CancellationToken ct)
        {
            var p     = q.Params;
            var query = _uow.Campaigns.Query();
            if (!string.IsNullOrWhiteSpace(p.Search))
            { var t = p.Search.ToLower(); query = query.Where(c => c.Name.ToLower().Contains(t)); }
            query = p.SortAscending ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name);
            var total = await query.CountAsync(ct);
            var items = await query.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize)
                .Select(c => new CampaignListDto
                {
                    Id = c.Id, Name = c.Name, Status = c.Status, BudgetGoal = c.BudgetGoal,
                    TotalRaised    = c.Donations.Where(d => !d.IsDeleted).Sum(d => (decimal?)d.Amount) ?? 0,
                    ProgressPercent = c.BudgetGoal.HasValue && c.BudgetGoal > 0
                        ? Math.Round((c.Donations.Where(d => !d.IsDeleted).Sum(d => (decimal?)d.Amount) ?? 0) / c.BudgetGoal.Value * 100, 1) : null,
                    StartDate = c.StartDate, EndDate = c.EndDate,
                    DonationCount = c.Donations.Count(d => !d.IsDeleted)
                }).ToListAsync(ct);
            return new PagedResult<CampaignListDto> { Items = items, Page = p.Page, PageSize = p.PageSize, TotalCount = total };
        }
    }

    public class GetCampaignByIdQuery : IRequest<CampaignDto>
    {
        public Guid CampaignId { get; }
        public GetCampaignByIdQuery(Guid id) { CampaignId = id; }
    }

    public sealed class GetCampaignByIdHandler : IRequestHandler<GetCampaignByIdQuery, CampaignDto>
    {
        private readonly IUnitOfWork _uow;
        public GetCampaignByIdHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<CampaignDto> Handle(GetCampaignByIdQuery q, CancellationToken ct)
        {
            var c = await _uow.Campaigns.GetByIdAsync(q.CampaignId, ct)
                ?? throw new NotFoundException(nameof(Campaign), q.CampaignId);
            var raised = await _uow.Donations.Query().Where(d => d.CampaignId == c.Id).SumAsync(d => (decimal?)d.Amount, ct) ?? 0;
            var count  = await _uow.Donations.CountAsync(d => d.CampaignId == c.Id, ct);
            return new CampaignDto
            {
                Id = c.Id, Name = c.Name, Description = c.Description, Status = c.Status,
                BudgetGoal = c.BudgetGoal, TotalRaised = raised,
                ProgressPercent = c.BudgetGoal.HasValue && c.BudgetGoal > 0 ? Math.Round(raised / c.BudgetGoal.Value * 100, 1) : null,
                StartDate = c.StartDate, EndDate = c.EndDate, Notes = c.Notes,
                DonationCount = count, CreatedAt = c.CreatedAt, LastModifiedAt = c.LastModifiedAt
            };
        }
    }
}
