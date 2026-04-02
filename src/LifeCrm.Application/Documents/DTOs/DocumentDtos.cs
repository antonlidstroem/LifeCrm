using System.ComponentModel.DataAnnotations;
using LifeCrm.Core.Enums;

namespace LifeCrm.Application.Documents.DTOs
{
    public record DocumentDto
    {
        public Guid Id { get; init; }
        public DocumentType Type { get; init; }
        public string FileName { get; init; } = string.Empty;
        public string? ReceiptNumber { get; init; }
        public Guid ContactId { get; init; }
        public string ContactName { get; init; } = string.Empty;
        public Guid? DonationId { get; init; }
        public DateOnly? PeriodStart { get; init; }
        public DateOnly? PeriodEnd { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }

    public record GenerateDonationReceiptRequest
    {
        [Required] public Guid DonationId { get; init; }
        public bool SendByEmail { get; init; } = false;
    }

    public record GenerateDonationSummaryRequest
    {
        [Required] public Guid ContactId { get; init; }
        [Required] public DateOnly PeriodStart { get; init; }
        [Required] public DateOnly PeriodEnd { get; init; }
        public bool SendByEmail { get; init; } = false;
    }
}
