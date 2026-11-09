using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models.Account;

/// <summary>
/// The model of the sign-in form.
/// </summary>
public class LoginViewModel
{
    /// <summary>Gets or sets the email address.</summary>
    [Required(ErrorMessage = "Enter your email.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the password.</summary>
    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the user stays signed in after closing the browser.</summary>
    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    /// <summary>Gets or sets the local URL to return to after signing in.</summary>
#pragma warning disable CA1056 // The return URL is posted back as is and is checked with Url.IsLocalUrl.
    public string? ReturnUrl { get; set; }
#pragma warning restore CA1056
}
