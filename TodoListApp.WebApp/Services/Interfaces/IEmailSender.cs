namespace TodoListApp.WebApp.Services.Interfaces;

/// <summary>
/// Sends emails to the users of the web application.
/// </summary>
public interface IEmailSender
{
    /// <summary>Sends an email.</summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="htmlMessage">The email body in HTML.</param>
    /// <returns>A task that completes when the email is sent.</returns>
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}
