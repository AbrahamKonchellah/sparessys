using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SparePartsWeb.Models;

namespace SparePartsWeb.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ConfirmEmailModel> _logger;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager, ILogger<ConfirmEmailModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                _logger.LogWarning("Missing userId or code in confirm email link.");
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Unable to find user with ID {UserId}.", userId);
                StatusMessage = "Error: Unable to find user.";
                return Page();
            }

            try
            {
                //  Decode token correctly
                var decodedBytes = WebEncoders.Base64UrlDecode(code);
                var normalToken = Encoding.UTF8.GetString(decodedBytes);

                var result = await _userManager.ConfirmEmailAsync(user, normalToken);
                _logger.LogInformation(" ConfirmEmail result for {Email}: {Result}", user.Email, result.Succeeded);

                StatusMessage = result.Succeeded
                    ? " Thank you for confirming your email!"
                    : " Error confirming your email.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Exception during email confirmation for user {UserId}.", userId);
                StatusMessage = " Invalid or corrupted confirmation link.";
            }

            return Page();
        }
    }
}
