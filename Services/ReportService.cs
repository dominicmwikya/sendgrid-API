using EmailAPI.Controllers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using EmailAPI.DTOs;

namespace EmailAPI.Services
{
    public class ReportService : IReportService
    {
       public  byte[] GeneratePdfReport(List<Payment> reportData)
        {
            using var outputstream = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("DAILY PAYMENT REPORT")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(40);
                            x.Item().Text("Daily Toll Payment report");
                            x.Item().Text("Company: Nimule station");
                            x.Item().Text("Date:" + DateTime.Now.ToString("yyyy-MM-dd"));

                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(100);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Border(1).Padding(2).Text("Date").SemiBold();
                                    header.Cell().Border(1).Padding(2).Text("Truck Number").SemiBold();
                                    header.Cell().Border(1).Padding(2).Text("Transaction Ref").SemiBold();
                                    header.Cell().Border(1).Padding(2).Text("Amount").SemiBold();
                                });

                                foreach (var payment in reportData)
                                {
                                    table.Cell().Border(1).Padding(2).Text(payment.Date.ToString("yyyy-MM-dd"));
                                    table.Cell().Border(1).Padding(2).Text(payment.TruckNumber);
                                    table.Cell().Border(1).Padding(2).Text(payment.TransactionReference);
                                    table.Cell().Border(1).Padding(2).Text(payment.Amount.ToString("C", new CultureInfo("en-KE")));
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            }).GeneratePdf(outputstream);
            return outputstream.ToArray();
        }
    }
}
