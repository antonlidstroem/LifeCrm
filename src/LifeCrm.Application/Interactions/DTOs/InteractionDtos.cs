using System.ComponentModel.DataAnnotations;
using LifeCrm.Core.Enums;

namespace LifeCrm.Application.Interactions.DTOs
{
    public record InteractionDto
    {
        public Guid Id { get; init; }
        public InteractionType Type { get; init; }
        public string Body { get; init; } = string.Empty;
        public string? Subject { get; init; }
        public DateTimeOffset OccurredAt { get; init; }
        public Guid? ContactId { get; init; }
        public string? ContactName { get; init; }
        public Guid? ProjectId { get; init; }
        public string? ProjectName { get; init; }
        public DateOnly? DueDate { get; init; }
        public bool IsCompleted { get; init; }
        public string CreatedByName { get; init; } = string.Empty;
        public DateTimeOffset CreatedAt { get; init; }
    }

    public record CreateInteractionRequest
    {
        [Required] public InteractionType Type { get; init; } = InteractionType.Note;
        [Required][MaxLength(8000)] public string Body { get; init; } = string.Empty;
        public Guid? ContactId { get; init; }
        public Guid? ProjectId { get; init; }
        [MaxLength(500)] public string? Subject { get; init; }
        public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
        public DateOnly? DueDate { get; init; }
        public bool IsCompleted { get; init; } = false;
    }

    public record UpdateInteractionRequest : CreateInteractionRequest
    {
        [Required] public Guid Id { get; init; }
    }
}
