using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparePartsWeb.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;

namespace SparePartsWeb.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
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
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don’t reveal if user exists or not
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code = token },
                protocol: Request.Scheme);

            var encodedUrl = HtmlEncoder.Default.Encode(callbackUrl);

            var subject = "Reset Your Password";
            var body = $@"
                <p>Hello {user.UserName},</p>
                <p>You requested to reset your password. Click below to continue:</p>
                <p><a href='{encodedUrl}' style='background-color:#007bff;color:#fff;
                padding:10px 15px;text-decoration:none;border-radius:5px;'>Reset Password</a></p>
                <br/>
                <p>If you didn’t request this, ignore this email.</p>
                <p>Best regards,<br/>SparePartsWeb Team</p>";

            Console.WriteLine($"📧 Sending password reset email to: {Input.Email}");

            await _emailSender.SendEmailAsync(Input.Email, subject, body);

            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}
