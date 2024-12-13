using static EmailAPI.Controllers.EmailController;

using EmailAPI.DTOs;
namespace EmailAPI.Services
{
    public interface IReportService
    {
        byte[] GeneratePdfReport(List<Payment> reportData);
    }
}
