namespace TodoListApp.Services.Interfaces;

/// <summary>
/// Manages the tags added to tasks stored in the database.
/// </summary>
public interface ITagDatabaseService
{
    /// <summary>Gets a page of the tags added to the tasks the user has access to.</summary>
    /// <param name="userId">The identifier of the to-do list owner or the task assignee.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of tags on a page.</param>
    /// <returns>The tags on the requested page, sorted by name.</returns>
    Task<IReadOnlyList<TagSummary>> GetAllAsync(string userId, int pageNumber, int pageSize);

    /// <summary>Gets a page of the tasks the user has access to that are tagged with a tag.</summary>
    /// <param name="userId">The identifier of the to-do list owner or the task assignee.</param>
    /// <param name="tagName">The tag name.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of tasks on a page.</param>
    /// <returns>The tasks on the requested page, sorted by due date.</returns>
    Task<IReadOnlyList<TodoTask>> GetTasksAsync(string userId, string tagName, int pageNumber, int pageSize);

    /// <summary>Adds a tag to a task of the user's to-do list. Adding a tag the task already has does nothing.</summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="tagName">The tag name: letters, digits, spaces, '-' and '_', up to 50 characters.</param>
    /// <param name="ownerId">The identifier of the to-do list owner.</param>
    /// <returns><c>true</c> if the task has the tag; <c>false</c> if the task does not exist.</returns>
    Task<bool> AddAsync(int taskId, string tagName, string ownerId);

    /// <summary>Removes a tag from a task of the user's to-do list.</summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="tagName">The tag name.</param>
    /// <param name="ownerId">The identifier of the to-do list owner.</param>
    /// <returns><c>true</c> if removed; <c>false</c> if the task does not exist or does not have the tag.</returns>
    Task<bool> RemoveAsync(int taskId, string tagName, string ownerId);
}
