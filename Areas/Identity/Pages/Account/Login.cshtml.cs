// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SparePartsWeb.Models;
using SparePartsWeb.Services;

namespace SparePartsWeb.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        [TempData]
        public string UnconfirmedEmail { get; set; } //  Store unconfirmed email temporarily

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);

                if (user != null)
                {
                    var isConfirmed = await _userManager.IsEmailConfirmedAsync(user);
                    if (!isConfirmed)
                    {
                        _logger.LogWarning(" Login attempt with unconfirmed email: {Email}", Input.Email);
                        UnconfirmedEmail = Input.Email;
                        ModelState.AddModelError(string.Empty, "Email not confirmed. Please check your inbox or resend the confirmation email below.");
                        return Page();
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation(" User logged in: {Email}", Input.Email);
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning(" User account locked out: {Email}", Input.Email);
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    _logger.LogWarning(" Invalid login attempt for: {Email}", Input.Email);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            return Page();
        }

        // ✅ Handles resending confirmation emails
        public async Task<IActionResult> OnPostResendConfirmationAsync()
        {
            if (string.IsNullOrEmpty(UnconfirmedEmail))
            {
                ModelState.AddModelError(string.Empty, "No unconfirmed email found.");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(UnconfirmedEmail);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return Page();
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = token },
                protocol: Request.Scheme);

            var emailBody = $@"
                <p>Hello {user.UserName},</p>
                <p>Please confirm your email address by clicking the link below:</p>
                <p><a href='{HtmlEncoder.Default.Encode(confirmationUrl)}'
                style='background-color:#28a745;color:white;padding:10px 15px;
                text-decoration:none;border-radius:5px;'>Confirm Email</a></p>
                <br/>
                <p>Thank you for registering with SparePartsWeb.</p>";

            await _emailService.SendEmailAsync(user.Email, "Confirm Your Email", emailBody);

            _logger.LogInformation("📧 Confirmation email resent to: {Email}", user.Email);
            TempData["Message"] = "Confirmation email resent successfully!";
            return RedirectToPage();
        }
    }
}
