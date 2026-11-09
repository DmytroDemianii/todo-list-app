using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using TodoListApp.WebApp.Models.Account;
using TodoListApp.WebApp.Services.Interfaces;

namespace TodoListApp.WebApp.Controllers;

/// <summary>
/// Provides the sign-up, sign-in, sign-out and password recovery pages (Epic 7).
/// </summary>
[AllowAnonymous]
public partial class AccountController : Controller
{
    /// <summary>The <see cref="Controller.TempData"/> key of the password reset link shown in the development environment.</summary>
    internal const string DevelopmentResetLinkKey = "DevelopmentResetLink";

    private readonly UserManager<IdentityUser> userManager;
    private readonly SignInManager<IdentityUser> signInManager;
    private readonly IEmailSender emailSender;
    private readonly IWebHostEnvironment environment;
    private readonly ILogger<AccountController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountController"/> class.
    /// </summary>
    /// <param name="userManager">The ASP.NET Core Identity user manager.</param>
    /// <param name="signInManager">The ASP.NET Core Identity sign-in manager.</param>
    /// <param name="emailSender">The service that sends the password recovery emails.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <param name="logger">The logger.</param>
    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IEmailSender emailSender,
        IWebHostEnvironment environment,
        ILogger<AccountController> logger)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.emailSender = emailSender;
        this.environment = environment;
        this.logger = logger;
    }

    /// <summary>Shows the sign-up form (US26).</summary>
    /// <returns>The sign-up page.</returns>
    [HttpGet]
    public IActionResult Register() => this.View(new RegisterViewModel());

    /// <summary>Creates a user account and signs the user in (US26).</summary>
    /// <param name="model">The form data.</param>
    /// <returns>A redirect to the to-do lists, or the form with errors.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model is null)
        {
            return this.BadRequest();
        }

        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        var user = new IdentityUser { UserName = model.Email, Email = model.Email };
        var result = await this.userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error.Description);
            }

            return this.View(model);
        }

        this.LogUserEvent(user.Id, "signed up");
        await this.signInManager.SignInAsync(user, isPersistent: false);
        return this.RedirectToAction(nameof(TodoListController.Index), "TodoList");
    }

    /// <summary>Shows the sign-in form (US27).</summary>
    /// <param name="returnUrl">The local URL to return to after signing in.</param>
    /// <returns>The sign-in page.</returns>
    [HttpGet]
#pragma warning disable CA1054 // The return URL comes from the query string as is and is checked with Url.IsLocalUrl.
    public IActionResult Login(string? returnUrl = null) => this.View(new LoginViewModel { ReturnUrl = returnUrl });
#pragma warning restore CA1054

    /// <summary>Signs the user in with an email and a password (US27).</summary>
    /// <param name="model">The form data.</param>
    /// <returns>A redirect to the return URL or the to-do lists, or the form with errors.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (model is null)
        {
            return this.BadRequest();
        }

        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        var result = await this.signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            this.LogSignInFailed("locked out");
            this.ModelState.AddModelError(string.Empty, "The account is locked after too many failed attempts. Try again in a few minutes.");
            return this.View(model);
        }

        if (!result.Succeeded)
        {
            this.LogSignInFailed("wrong email or password");
            this.ModelState.AddModelError(string.Empty, "The email or password is incorrect.");
            return this.View(model);
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && this.Url.IsLocalUrl(model.ReturnUrl))
        {
            return this.LocalRedirect(model.ReturnUrl);
        }

        return this.RedirectToAction(nameof(TodoListController.Index), "TodoList");
    }

    /// <summary>Signs the user out (US28).</summary>
    /// <returns>A redirect to the sign-in page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await this.signInManager.SignOutAsync();
        return this.RedirectToAction(nameof(this.Login));
    }

    /// <summary>Shows the form to request password recovery instructions (US29).</summary>
    /// <returns>The forgot password page.</returns>
    [HttpGet]
    public IActionResult ForgotPassword() => this.View(new ForgotPasswordViewModel());

    /// <summary>Sends password recovery instructions to the email the user signed up with (US29).</summary>
    /// <param name="model">The form data.</param>
    /// <returns>A redirect to the confirmation page, or the form with errors.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (model is null)
        {
            return this.BadRequest();
        }

        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        // The confirmation page is the same whether the account exists or not, so emails cannot be guessed.
        var user = await this.userManager.FindByEmailAsync(model.Email);
        if (user is not null)
        {
            var token = await this.userManager.GeneratePasswordResetTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetLink = this.Url.Action(nameof(this.ResetPassword), "Account", new { code, email = model.Email }, this.Request.Scheme)
                ?? throw new InvalidOperationException("Cannot build the password reset link.");

            await this.emailSender.SendEmailAsync(
                model.Email,
                "Reset your To-Do List password",
                $"To set a new password, <a href=\"{HtmlEncoder.Default.Encode(resetLink)}\">click here</a>.");

            this.LogUserEvent(user.Id, "requested a password reset");
            if (this.environment.IsDevelopment())
            {
                this.TempData[DevelopmentResetLinkKey] = resetLink;
            }
        }

        return this.RedirectToAction(nameof(this.ForgotPasswordConfirmation));
    }

    /// <summary>Tells the user that the recovery instructions were sent (US29).</summary>
    /// <returns>The confirmation page.</returns>
    [HttpGet]
    public IActionResult ForgotPasswordConfirmation() => this.View();

    /// <summary>Shows the form to set a new password; the user opens it from the recovery email (US29).</summary>
    /// <param name="code">The password reset code.</param>
    /// <param name="email">The email address of the account.</param>
    /// <returns>The reset password page, or 400 if there is no code.</returns>
    [HttpGet]
    public IActionResult ResetPassword(string? code = null, string? email = null) =>
        string.IsNullOrEmpty(code) ? this.BadRequest() : this.View(new ResetPasswordViewModel { Code = code, Email = email ?? string.Empty });

    /// <summary>Sets a new password with the reset code (US29).</summary>
    /// <param name="model">The form data.</param>
    /// <returns>A redirect to the confirmation page, or the form with errors.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (model is null)
        {
            return this.BadRequest();
        }

        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        var user = await this.userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            // The result is not revealed, so emails cannot be guessed.
            return this.RedirectToAction(nameof(this.ResetPasswordConfirmation));
        }

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
        }
        catch (FormatException)
        {
            this.ModelState.AddModelError(string.Empty, "The password reset link is invalid.");
            return this.View(model);
        }

        var result = await this.userManager.ResetPasswordAsync(user, token, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error.Description);
            }

            return this.View(model);
        }

        this.LogUserEvent(user.Id, "reset the password");
        return this.RedirectToAction(nameof(this.ResetPasswordConfirmation));
    }

    /// <summary>Tells the user that the password was changed (US29).</summary>
    /// <returns>The confirmation page.</returns>
    [HttpGet]
    public IActionResult ResetPasswordConfirmation() => this.View();

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} {Action}.")]
    private partial void LogUserEvent(string userId, string action);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Sign-in failed: {Reason}.")]
    private partial void LogSignInFailed(string reason);
}
