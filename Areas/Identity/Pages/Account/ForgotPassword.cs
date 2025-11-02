using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparePartsWeb.Models;
using SparePartsWeb.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;

namespace SparePartsWeb.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<ForgotPasswordModel> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation(" Forgot password request received for: {Email}", Input.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(" Invalid model state for forgot password.");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                _logger.LogWarning(" No user found with email: {Email}", Input.Email);
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                _logger.LogWarning(" User email not confirmed for: {Email}", Input.Email);
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            _logger.LogInformation(" User found: {UserName} ({Email})", user.UserName, user.Email);

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            _logger.LogInformation(" Token generated successfully for {Email}", user.Email);

            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code = token },
                protocol: Request.Scheme);

            var encodedUrl = HtmlEncoder.Default.Encode(callbackUrl);
            _logger.LogInformation(" Reset link generated: {Url}", encodedUrl);

            // Compose email
            var subject = "Reset Your Password";
            var body = $@"
                <p>Hello {user.UserName},</p>
                <p>You requested to reset your password. Click the link below:</p>
                <p><a href='{encodedUrl}' style='background-color:#007bff;color:#fff;
                padding:10px 15px;text-decoration:none;border-radius:5px;'>Reset Password</a></p>
                <br/>
                <p>If you didn’t request this, ignore this email.</p>
                <p>Best regards,<br/>SparePartsWeb Team</p>";

            try
            {
                _logger.LogInformation("Attempting to send password reset email to: {Email}", user.Email);
                await _emailService.SendEmailAsync(user.Email, subject, body);
                _logger.LogInformation(" Password reset email sent successfully to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Failed to send password reset email to {Email}", user.Email);
            }

            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}
