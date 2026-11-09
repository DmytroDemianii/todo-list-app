using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models.Account;

/// <summary>
/// The model of the sign-up form.
/// </summary>
public class RegisterViewModel
{
    /// <summary>Gets or sets the email address the user signs in with.</summary>
    [Required(ErrorMessage = "Enter your email.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the password.</summary>
    [Required(ErrorMessage = "Enter a password.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "The password must be at least {2} characters long.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Gets or sets the password confirmation; it must match the password.</summary>
    [Required(ErrorMessage = "Confirm the password.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
