using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Globalization;
using System.Net;
using System.ComponentModel.DataAnnotations;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using EmailAPI.Services;
using EmailAPI.DTOs;
namespace EmailAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private IReportService _reportService;
        private readonly IConfiguration _config;
        public EmailController(IConfiguration configuration, IReportService reportService)
        {
            _config = configuration;
            _reportService = reportService;
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
                var pdfBytes =  _reportService.GeneratePdfReport(request.ReportData);
                    //GeneratePdfReport(request.ReportData);
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
    }
}
