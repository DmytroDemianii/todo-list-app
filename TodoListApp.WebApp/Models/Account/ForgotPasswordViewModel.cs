using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models.Account;

/// <summary>
/// The model of the form that requests password recovery instructions.
/// </summary>
public class ForgotPasswordViewModel
{
    /// <summary>Gets or sets the email address the user signed up with.</summary>
    [Required(ErrorMessage = "Enter your email.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;
}
