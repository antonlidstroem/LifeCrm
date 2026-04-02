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
        Task<byte[]> GenerateDonationSummaryAsync(Contact contact, IEnumerable<Donation> donations, Organization org, DateOnly from, DateOnly to);
    }

    public interface IEmailService
    {
        Task SendAsync(string toEmail, string toName, string subject, string htmlBody,
            byte[]? attachmentBytes = null, string? attachmentName = null, CancellationToken ct = default);
    }

    public interface IOrganizationReader
    {
        Task<Organization?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task SaveDocumentAsync(Document document, CancellationToken ct = default);
    }
}
