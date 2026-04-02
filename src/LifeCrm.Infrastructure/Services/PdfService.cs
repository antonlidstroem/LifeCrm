using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestDocument = QuestPDF.Fluent.Document; 

namespace LifeCrm.Infrastructure.Services
{
    public class PdfService : IPdfService
    {
        static PdfService() { QuestPDF.Settings.License = LicenseType.Community; }

        public Task<byte[]> GenerateDonationReceiptAsync(
            Donation donation, Organization org, string receiptNumber)
        {
            var doc = QuestDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.Content().Column(col =>
                    {
                        col.Item().Text(org.Name).Bold().FontSize(20);
                        col.Item().Text($"Donation Receipt — {receiptNumber}").FontSize(14);
                        col.Item().PaddingVertical(10).LineHorizontal(1);
                        col.Item().Text($"Amount:  {donation.Amount:C}");
                        col.Item().Text($"Date:    {donation.Date:yyyy-MM-dd}");
                        col.Item().Text($"Status:  {donation.Status}");
                        if (!string.IsNullOrEmpty(donation.PaymentMethod))
                            col.Item().Text($"Method:  {donation.PaymentMethod}");
                        if (!string.IsNullOrEmpty(donation.ReferenceNumber))
                            col.Item().Text($"Ref:     {donation.ReferenceNumber}");
                    });
                });
            });
            return Task.FromResult(doc.GeneratePdf());
        }

        public Task<byte[]> GenerateDonationSummaryAsync(
            Contact contact, IEnumerable<Donation> donations,
            Organization org, DateOnly from, DateOnly to)
        {
            var list  = donations.ToList();
            var total = list.Sum(d => d.Amount);

            var doc = QuestDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.Content().Column(col =>
                    {
                        col.Item().Text(org.Name).Bold().FontSize(20);
                        col.Item().Text($"Donation Summary for {contact.Name}").FontSize(14);
                        col.Item().Text($"Period: {from:yyyy-MM-dd} – {to:yyyy-MM-dd}");
                        col.Item().PaddingVertical(10).LineHorizontal(1);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(1);
                            });
                            table.Header(h =>
                            {
                                h.Cell().Text("Date").Bold();
                                h.Cell().Text("Amount").Bold();
                                h.Cell().Text("Status").Bold();
                            });
                            foreach (var d in list)
                            {
                                table.Cell().Text(d.Date.ToString("yyyy-MM-dd"));
                                table.Cell().Text(d.Amount.ToString("C"));
                                table.Cell().Text(d.Status.ToString());
                            }
                        });
                        col.Item().PaddingTop(10).Text($"Total: {total:C}").Bold();
                    });
                });
            });
            return Task.FromResult(doc.GeneratePdf());
        }
    }
}
