using System.ComponentModel.DataAnnotations;
using LifeCrm.Core.Enums;

namespace LifeCrm.Application.Projects.DTOs
{
    public record ProjectListDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public ProjectStatus Status { get; init; }
        public string? Location { get; init; }
        public decimal? BudgetGoal { get; init; }
        public decimal TotalFunded { get; init; }
        public DateOnly? StartDate { get; init; }
        public DateOnly? EndDate { get; init; }
    }

    public record ProjectDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public ProjectStatus Status { get; init; }
        public string? Location { get; init; }
        public decimal? BudgetGoal { get; init; }
        public decimal TotalFunded { get; init; }
        public DateOnly? StartDate { get; init; }
        public DateOnly? EndDate { get; init; }
        public string? Notes { get; init; }
        public int DonationCount { get; init; }
        public int InteractionCount { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? LastModifiedAt { get; init; }
    }

    public record CreateProjectRequest
    {
        [Required][MaxLength(200)] public string Name { get; init; } = string.Empty;
        [MaxLength(2000)] public string? Description { get; init; }
        public ProjectStatus Status { get; init; } = ProjectStatus.Planning;
        [MaxLength(200)] public string? Location { get; init; }
        [Range(0, 100_000_000)] public decimal? BudgetGoal { get; init; }
        public DateOnly? StartDate { get; init; }
        public DateOnly? EndDate { get; init; }
        [MaxLength(4000)] public string? Notes { get; init; }
    }

    public record UpdateProjectRequest : CreateProjectRequest
    {
        [Required] public Guid Id { get; init; }
    }
}
