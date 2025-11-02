using Microsoft.AspNetCore.Mvc;
using SparePartsWeb.Services;

namespace SparePartsWeb.Controllers
{
    public class EmailTestController : Controller
    {
        private readonly IEmailService _emailService;

        public EmailTestController(IEmailService emailService)
        {
            _emailService = emailService;
        }
        [HttpGet("test-email")]
        public async Task<IActionResult> SendTestEmail()
        {
            string toEmail = "bonilinux70@gmail.com"; // 🟡 replace with your actual email
            string subject = "✅ Email Test from SparePartsWeb";
            string message = "Hello! This is a test email from your ASP.NET Core app.";

            await _emailService.SendEmailAsync(toEmail, subject, message);

            return Content($"✅ Test email sent successfully to {toEmail}!");
        }
    }
}

