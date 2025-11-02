using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparePartsWeb.Models;
using SparePartsWeb.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace SparePartsWeb.Areas.Identity.Pages.Account
{
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<ResendEmailConfirmationModel> _logger;

        public ResendEmailConfirmationModel(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<ResendEmailConfirmationModel> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? Message { get; set; }

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
            {
                Message = "Please enter a valid email.";
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                Message = "If this email is registered, a confirmation link will be sent.";
                return Page();
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                Message = "This email is already confirmed.";
                return Page();
            }

         
var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);


var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));


var callbackUrl = Url.Page(
    "/Account/ConfirmEmail",
    pageHandler: null,
    values: new { area = "Identity", userId = user.Id, code = encodedToken },
    protocol: Request.Scheme);


var subject = "Confirm your SparePartsWeb account";
var body = $@"
    <p>Hello {user.UserName},</p>
    <p>Please confirm your account by clicking below:</p>
    <p><a href='{callbackUrl}' style='background-color:#28a745;color:white;
    padding:10px 15px;text-decoration:none;border-radius:5px;'>Confirm Email</a></p>
    <p>If you didn’t create this account, ignore this email.</p>";

            try
            {
                await _emailService.SendEmailAsync(user.Email, subject, body);
                Message = "Confirmation email has been sent. Check your inbox.";
                _logger.LogInformation(" Resent confirmation email to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Failed to send confirmation email to {Email}", user.Email);
                Message = "Error sending email. Please try again later.";
            }

            return Page();
        }
    }
}
