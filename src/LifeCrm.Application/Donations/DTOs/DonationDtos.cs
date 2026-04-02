using System.ComponentModel.DataAnnotations;
using LifeCrm.Core.Enums;

namespace LifeCrm.Application.Donations.DTOs
{
    public record DonationListDto
    {
        public Guid Id { get; init; }
        public Guid ContactId { get; init; }
        public Guid? ReceiptDocumentId { get; init; }
        public string ContactName { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public DateOnly Date { get; init; }
        public DonationStatus Status { get; init; }
        public string? CampaignName { get; init; }
        public string? ProjectName { get; init; }
        public string? PaymentMethod { get; init; }
        public bool ReceiptSent { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }

    public record DonationDto
    {
        public Guid Id { get; init; }
        public Guid ContactId { get; init; }
        public string ContactName { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public DateOnly Date { get; init; }
        public DonationStatus Status { get; init; }
        public Guid? CampaignId { get; init; }
        public string? CampaignName { get; init; }
        public Guid? ProjectId { get; init; }
        public string? ProjectName { get; init; }
        public Guid? RecurringDonationId { get; init; }
        public string? PaymentMethod { get; init; }
        public string? ReferenceNumber { get; init; }
        public string? Notes { get; init; }
        public bool ReceiptSent { get; init; }
        public DateTimeOffset? ReceiptSentAt { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? LastModifiedAt { get; init; }
    }

    public record CreateDonationRequest
    {
        [Required] public Guid ContactId { get; init; }
        [Required][Range(0.01, 10_000_000)] public decimal Amount { get; init; }
        [Required] public DateOnly Date { get; init; } = DateOnly.FromDateTime(DateTime.Today);
        public Guid? CampaignId { get; init; }
        public Guid? ProjectId  { get; init; }
        [MaxLength(100)] public string? PaymentMethod { get; init; }
        [MaxLength(200)] public string? ReferenceNumber { get; init; }
        [MaxLength(2000)] public string? Notes { get; init; }
        public DonationStatus Status { get; init; } = DonationStatus.Confirmed;
    }

    public record UpdateDonationRequest : CreateDonationRequest
    {
        [Required] public Guid Id { get; init; }
    }
}
