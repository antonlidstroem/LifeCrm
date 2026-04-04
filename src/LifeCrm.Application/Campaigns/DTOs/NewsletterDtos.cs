using System.ComponentModel.DataAnnotations;

namespace LifeCrm.Application.Campaigns.DTOs
{
    public record SendNewsletterRequest
    {
        [Required]
        [MaxLength(500)]
        public string Subject { get; init; } = string.Empty;

        [Required]
        [MaxLength(100_000)]
        public string HtmlBody { get; init; } = string.Empty;

        /// <summary>
        /// Optional comma-separated tags. When set, only contacts that have ALL
        /// of the listed tags (case-insensitive substring match on the Tags field)
        /// receive the newsletter.  Leave null/empty to send to all opted-in contacts.
        /// </summary>
        [MaxLength(500)]
        public string? TagFilter { get; init; }
    }

    public record NewsletterResultDto
    {
        public int SentCount    { get; init; }
        public int SkippedCount { get; init; }   // no email or opted out
        public int ErrorCount   { get; init; }   // had email but send failed
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    }

    /// <summary>Used by the preview endpoint to estimate recipient count before sending.</summary>
    public record NewsletterPreviewDto
    {
        public int EligibleCount { get; init; }   // would receive the email
        public int OptedOutCount { get; init; }   // would be skipped (opted out)
        public int NoEmailCount  { get; init; }   // would be skipped (no address)
    }
}
