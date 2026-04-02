using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities
{
    public class Project : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
        public string? Location { get; set; }
        public decimal? BudgetGoal { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Notes { get; set; }
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();
    }
}
