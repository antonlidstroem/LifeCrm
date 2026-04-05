using LifeCrm.Core.Entities;

namespace LifeCrm.Core.Interfaces
{
    public interface ICsvService
    {
        Task<byte[]> ExportAsync<T>(IEnumerable<T> rows);
        Task<(List<T> Rows, List<CsvParseError> Errors)> ImportAsync<T>(byte[] csvBytes);
    }

    public record CsvParseError(int RowNumber, string Reason, string RawRow);

    public interface IPdfService
    {
        Task<byte[]> GenerateDonationReceiptAsync(Donation donation, Organization org, string receiptNumber);
        Task<byte[]> GenerateDonationSummaryAsync(Contact contact, IEnumerable<Donation> donations,
            Organization org, DateOnly from, DateOnly to);
    }

    public interface IEmailService
    {
        /// <summary>Send one email, optionally with multiple attachments.</summary>
        Task SendAsync(
            string  toEmail,
            string  toName,
            string  subject,
            string  htmlBody,
            // Phase 2: accepts multiple attachments; null/empty = no attachments
            IEnumerable<EmailAttachment>? attachments = null,
            CancellationToken ct = default);
    }

    /// <summary>Represents a single file to attach to an outgoing email.</summary>
    public record EmailAttachment(byte[] Bytes, string FileName, string ContentType);

    public interface IOrganizationReader
    {
        Task<Organization?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task SaveDocumentAsync(Document document, CancellationToken ct = default);
    }
}
