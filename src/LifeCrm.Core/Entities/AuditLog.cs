namespace LifeCrm.Core.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string ChangedBy { get; set; } = "system";
        public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
    }
}
