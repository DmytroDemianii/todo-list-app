using TodoListApp.WebApp.Services.Interfaces;

namespace TodoListApp.WebApp.Services;

/// <summary>
/// An email sender for development: instead of sending emails, it writes them to the application log.
/// Replace it with an SMTP or email service implementation to send real emails.
/// </summary>
public partial class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingEmailSender"/> class.
    /// </summary>
    /// <param name="logger">The logger the emails are written to.</param>
    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        this.logger = logger;
    }

    /// <inheritdoc/>
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        this.LogEmail(email, subject, htmlMessage);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Email to {Email}: {Subject}\n{Body}")]
    private partial void LogEmail(string email, string subject, string body);
}
