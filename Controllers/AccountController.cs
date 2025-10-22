using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SparePartsWeb.Models;
using SparePartsWeb.Services;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EmailService _emailService;

    public AccountController(UserManager<ApplicationUser> userManager, EmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    // GET: /Account/ForgotPassword
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    // POST: /Account/ForgotPassword
    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            ModelState.AddModelError("", "Email is required.");
            return View();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            ModelState.AddModelError("", "No user found with this email.");
            return View();
        }

        // Generate reset token
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Create reset link
        var resetLink = Url.Action("ResetPassword", "Account",
            new { token, email = user.Email }, Request.Scheme);

        // Send email
        var subject = "Reset Your Password - SparePartsWeb";
        var message = $"<p>Hi {user.UserName},</p>" +
                      $"<p>Click below to reset your password:</p>" +
                      $"<a href='{resetLink}'>Reset Password</a>" +
                      $"<p>If you didn’t request this, ignore this email.</p>";

        await _emailService.SendEmailAsync(user.Email, subject, message);

        ViewBag.Message = "A password reset link has been sent to your email.";
        return View("ForgotPasswordConfirmation");
    }

    // GET: /Account/ResetPassword
    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
    {
        if (token == null || email == null)
            return RedirectToAction("ForgotPassword");

        ViewBag.Token = token;
        ViewBag.Email = email;
        return View();
    }

    // POST: /Account/ResetPassword
    [HttpPost]
    public async Task<IActionResult> ResetPassword(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            ModelState.AddModelError("", "Invalid email.");
            return View();
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (result.Succeeded)
        {
            ViewBag.Message = "Password reset successfully.";
            return View("ResetPasswordConfirmation");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View();
    }
}
