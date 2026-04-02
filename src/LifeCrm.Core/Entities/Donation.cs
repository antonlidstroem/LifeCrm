using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities
{
    public class Donation : TenantEntity
    {
        public Guid ContactId { get; set; }
        public decimal Amount { get; set; }
        public DateOnly Date { get; set; }
        public DonationStatus Status { get; set; } = DonationStatus.Confirmed;
        public Guid? CampaignId { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? RecurringDonationId { get; set; }
        public string? PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public bool ReceiptSent { get; set; } = false;
        public DateTimeOffset? ReceiptSentAt { get; set; }
        public string CreatedBy { get; set; } = "system";
        public Contact? Contact { get; set; }
        public Campaign? Campaign { get; set; }
        public Project? Project { get; set; }
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
