using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities
{
    public class Interaction : TenantEntity
    {
        public InteractionType Type { get; set; } = InteractionType.Note;
        public string Body { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? ContactId { get; set; }
        public Guid? ProjectId { get; set; }
        public DateOnly? DueDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public string CreatedBy { get; set; } = "system";
        public Contact? Contact { get; set; }
        public Project? Project { get; set; }
    }
}
