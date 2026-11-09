using Microsoft.EntityFrameworkCore;
using TodoListApp.WebApp.Data;
using TodoListApp.WebApp.Services.Interfaces;

namespace TodoListApp.WebApp.Services;

/// <summary>
/// Finds the email addresses of users in the UsersDb database.
/// </summary>
public class UserNameService : IUserNameService
{
    private readonly UsersDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserNameService"/> class.
    /// </summary>
    /// <param name="dbContext">The ASP.NET Core Identity database context.</param>
    public UserNameService(UsersDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyDictionary<string, string>> GetEmailsAsync(IEnumerable<string> userIds)
    {
        ArgumentNullException.ThrowIfNull(userIds);

        return this.GetEmailsCoreAsync(userIds);
    }

    private async Task<IReadOnlyDictionary<string, string>> GetEmailsCoreAsync(IEnumerable<string> userIds)
    {
        var ids = userIds.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<string, string>();
        }

        // One query for all users shown on a page.
        return await this.dbContext.Users
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id) && x.Email != null)
            .ToDictionaryAsync(x => x.Id, x => x.Email!);
    }
}
