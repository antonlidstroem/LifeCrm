using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities
{
    public class Campaign : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CampaignStatus Status { get; set; } = CampaignStatus.Draft;
        public decimal? BudgetGoal { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Notes { get; set; }
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    }
}
