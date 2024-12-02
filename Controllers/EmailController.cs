using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Globalization;
using System.Net;
using System.ComponentModel.DataAnnotations;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

namespace EmailAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IConfiguration _config;
        public EmailController(IConfiguration configuration)
        {
            _config = configuration;
        }
        public class EmailReportRequest
        {
            [Required]
            public List<string> Emails { get; set; }

            [Required]
            public List<Payment> ReportData { get; set; }
        }
        // POST api/<EmailController>
        [HttpPost("send-report")]
        public async Task<IActionResult> PostMail([FromBody] EmailReportRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;
                var pdfBytes = GeneratePdfReport(request.ReportData);

                var emailTasks = request.Emails.Select(async email =>
                {
                    try
                    {
                        await SendEmailAsync(email, pdfBytes, "Daily Payment Report");
                        return $"Email sent successfully to {email}";
                    }
                    catch (Exception ex)
                    {
                        return $"Failed to send email to {email}: {ex.Message}";
                    }
                });

                var results = await Task.WhenAll(emailTasks);
                return Ok(new { message = "Emails sent to all recipients", results });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to send emails! Try again!", error = ex.Message });
            }
        }
        private async Task SendEmailAsync(string toEmail, byte[] pdfbytes, string pdfName)
        {
            try
            {
                var subject = "Daily Report";
                var plainText = "Daily report for date";
                var htmltext = $"<strong>Daily Report for date {DateTime.Now.ToString("yyyy-MM-dd")}</strong>";

                var apiKey = _config["SendGrid:apiKey"];
                var client = new SendGridClient(apiKey);
                var clientEmail = _config["SendGrid:verifiedSingleEmail"];
                var clientName = _config["SendGrid:name"];
                var from = new EmailAddress(clientEmail, clientName);
                var to = new EmailAddress(toEmail);

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmltext);
                msg.AddAttachment(pdfName, Convert.ToBase64String(pdfbytes), "application/pdf");

                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Accepted)
                {
                    var errorMessage = await response.Body.ReadAsStringAsync();
                    throw new Exception($"Error sending email: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending email: {ex.Message}");
            }
        }

        public class Payment
        {
            [Required]
            public string TransactionReference { get; set; }

            [Required]
            public string TruckNumber { get; set; }

            [Required]
            public DateTime Date { get; set; }

            [Required]
            public int Amount { get; set; }
        }

        private byte[] GeneratePdfReport(List<Payment> reportData)
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
