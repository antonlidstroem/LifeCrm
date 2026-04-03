using LifeCrm.Application.Common.DTOs;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Dashboard
{
    public class GetDashboardQuery : IRequest<DashboardDto> { }

    public sealed class GetDashboardHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
    {
        private readonly IUnitOfWork _uow;
        public GetDashboardHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<DashboardDto> Handle(GetDashboardQuery q, CancellationToken ct)
        {
            var today          = DateOnly.FromDateTime(DateTime.UtcNow);
            var thisMonthStart = new DateOnly(today.Year, today.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd   = thisMonthStart.AddDays(-1);

            // FIXED: Use DateTimeOffset.UtcNow-based threshold for CreatedAt comparisons
            // to avoid timezone mismatch between DateOnly.ToDateTime and DateTimeOffset columns.
            var thisMonthStartDto = new DateTimeOffset(thisMonthStart.Year, thisMonthStart.Month, 1,
                0, 0, 0, TimeSpan.Zero);

            var thisMonth = await _uow.Donations.Query()
                .Where(d => d.Date >= thisMonthStart)
                .SumAsync(d => (decimal?)d.Amount, ct) ?? 0;

            var lastMonth = await _uow.Donations.Query()
                .Where(d => d.Date >= lastMonthStart && d.Date <= lastMonthEnd)
                .SumAsync(d => (decimal?)d.Amount, ct) ?? 0;

            var moM = lastMonth > 0
                ? Math.Round((thisMonth - lastMonth) / lastMonth * 100, 1)
                : 0;

            var contacts = await _uow.Contacts.CountAsync(_ => true, ct);

            // FIXED: Compare DateTimeOffset CreatedAt using a DateTimeOffset threshold
            var newCon = await _uow.Contacts.CountAsync(
                c => c.CreatedAt >= thisMonthStartDto, ct);

            var active  = await _uow.Campaigns.GetActiveAsync(ct);
            var ids     = active.Select(c => c.Id).ToList();
            var totals  = await _uow.Donations.Query()
                .Where(d => d.CampaignId.HasValue && ids.Contains(d.CampaignId.Value))
                .GroupBy(d => d.CampaignId!.Value)
                .Select(g => new { g.Key, Total = g.Sum(d => (decimal?)d.Amount) ?? 0 })
                .ToListAsync(ct);
            var totDict = totals.ToDictionary(x => x.Key, x => x.Total);

            var top = active.Take(5).Select(c =>
            {
                var r = totDict.GetValueOrDefault(c.Id, 0m);
                return new CampaignSummaryDto
                {
                    Id = c.Id, Name = c.Name, BudgetGoal = c.BudgetGoal, TotalRaised = r,
                    ProgressPercent = c.BudgetGoal.HasValue && c.BudgetGoal > 0
                        ? Math.Round(r / c.BudgetGoal.Value * 100, 1) : null
                };
            }).ToList().AsReadOnly();

            var recentDon = await _uow.Donations.Query()
                .OrderByDescending(d => d.CreatedAt).Take(10)
                .Select(d => new ActivityFeedItemDto
                {
                    ActivityType = "Donation",
                    EntityId     = d.Id,
                    ContactId    = d.ContactId,
                    ContactName  = d.Contact != null ? d.Contact.Name : string.Empty,
                    Summary      = "$" + d.Amount.ToString("N2") + " donation",
                    OccurredAt   = d.CreatedAt
                }).ToListAsync(ct);

            var recentInt = await _uow.Interactions.Query()
                .Where(i => i.ContactId.HasValue)
                .OrderByDescending(i => i.OccurredAt).Take(10)
                .Select(i => new ActivityFeedItemDto
                {
                    ActivityType = "Interaction",
                    EntityId     = i.Id,
                    ContactId    = i.ContactId!.Value,
                    ContactName  = i.Contact != null ? i.Contact.Name : string.Empty,
                    Summary      = i.Type.ToString() + " logged",
                    OccurredAt   = i.OccurredAt
                }).ToListAsync(ct);

            var feed = recentDon.Concat(recentInt)
                .OrderByDescending(a => a.OccurredAt)
                .Take(10).ToList().AsReadOnly();

            return new DashboardDto
            {
                DonationsThisMonth        = thisMonth,
                DonationsLastMonth        = lastMonth,
                DonationsMoMChangePercent = moM,
                TotalContacts             = contacts,
                NewContactsThisMonth      = newCon,
                ActiveCampaigns           = active.Count,
                TopCampaigns              = top,
                RecentActivity            = feed
            };
        }
    }
}
