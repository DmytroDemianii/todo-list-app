namespace TodoListApp.WebApp.Services.Interfaces;

/// <summary>
/// Finds the names shown to users for user identifiers, such as a task assignee or a comment author.
/// </summary>
public interface IUserNameService
{
    /// <summary>Gets the email addresses of users by their identifiers.</summary>
    /// <param name="userIds">The user identifiers; duplicates are allowed.</param>
    /// <returns>The email addresses by user identifier; unknown identifiers are not included.</returns>
    Task<IReadOnlyDictionary<string, string>> GetEmailsAsync(IEnumerable<string> userIds);
}
