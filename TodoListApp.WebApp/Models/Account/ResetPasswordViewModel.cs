using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models.Account;

/// <summary>
/// The model of the form that sets a new password with a password reset code.
/// </summary>
public class ResetPasswordViewModel
{
    /// <summary>Gets or sets the email address of the account.</summary>
    [Required(ErrorMessage = "Enter your email.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the new password.</summary>
    [Required(ErrorMessage = "Enter a new password.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "The password must be at least {2} characters long.")]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Gets or sets the new password confirmation; it must match the new password.</summary>
    [Required(ErrorMessage = "Confirm the new password.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    [Display(Name = "Confirm new password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>Gets or sets the password reset code from the recovery email.</summary>
    [Required]
    public string Code { get; set; } = string.Empty;
}
