using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities
{
    public class Newsletter : TenantEntity
    {
        public string  Title             { get; set; } = string.Empty;
        public string  Subject           { get; set; } = string.Empty;
        public string  HtmlBody          { get; set; } = string.Empty;
        public NewsletterStatus Status   { get; set; } = NewsletterStatus.Draft;

        public string? TagFilter         { get; set; }
        public string? ContactTypeFilter { get; set; }

        public DateTimeOffset? SentAt    { get; set; }
        public string?         SentBy    { get; set; }
        public int             SentCount    { get; set; }
        public int             SkippedCount { get; set; }
        public int             ErrorCount   { get; set; }

        public string CreatedBy { get; set; } = "system";

        // Phase 2: navigation to attachments
        public ICollection<NewsletterAttachment> Attachments { get; set; }
            = new List<NewsletterAttachment>();
    }
}
