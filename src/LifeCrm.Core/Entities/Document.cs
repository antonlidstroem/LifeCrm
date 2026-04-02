using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities
{
    public class Document : TenantEntity
    {
        public DocumentType Type { get; set; }
        public string FileName { get; set; } = string.Empty;
        public byte[] PdfBytes { get; set; } = Array.Empty<byte>();
        public Guid? ContactId { get; set; }
        public Guid? DonationId { get; set; }
        public string? ReceiptNumber { get; set; }
        public DateOnly? PeriodStart { get; set; }
        public DateOnly? PeriodEnd { get; set; }
        public Contact? Contact { get; set; }
        public Donation? Donation { get; set; }
    }
}
