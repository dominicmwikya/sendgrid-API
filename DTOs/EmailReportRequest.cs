using System.ComponentModel.DataAnnotations;

namespace EmailAPI.DTOs
{
    public class EmailReportRequest
    {
        [Required]
        public List<string> Emails { get; set; }

        [Required]
        public List<Payment> ReportData { get; set; }
    }
}
