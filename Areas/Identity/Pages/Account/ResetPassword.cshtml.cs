#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SparePartsWeb.Models;

namespace SparePartsWeb.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ResetPasswordModel> _logger;

        public ResetPasswordModel(
            UserManager<ApplicationUser> userManager,
            ILogger<ResetPasswordModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required]
            public string Code { get; set; } = string.Empty;
        }

        public IActionResult OnGet(string code = null, string email = null)
        {
            if (code == null || email == null)
            {
                _logger.LogWarning("Reset password link accessed without code or email");
                return RedirectToPage("./ForgotPassword");
            }

            Input = new InputModel
            {
                Code = code,
                Email = email
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for reset password");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                _logger.LogWarning("Reset password attempted for non-existent user: {Email}", Input.Email);
                // Don't reveal that the user does not exist
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            _logger.LogInformation("Attempting to reset password for user: {Email}", Input.Email);

            // Decode the token
            string decodedToken;
            try
            {
                var decodedBytes = WebEncoders.Base64UrlDecode(Input.Code);
                decodedToken = Encoding.UTF8.GetString(decodedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decode reset token for user: {Email}", Input.Email);
                ModelState.AddModelError(string.Empty, "Invalid reset token. Please request a new password reset link.");
                return Page();
            }

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, Input.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("Password reset successfully for user: {Email}", Input.Email);
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            _logger.LogWarning("Password reset failed for user: {Email}. Errors: {Errors}", 
                Input.Email, string.Join(", ", result.Errors.Select(e => e.Description)));

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}

