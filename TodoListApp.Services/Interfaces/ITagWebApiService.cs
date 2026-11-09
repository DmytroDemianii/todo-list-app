namespace TodoListApp.Services.Interfaces;

/// <summary>
/// Manages the tags of tasks using the web API.
/// </summary>
public interface ITagWebApiService
{
    /// <summary>Gets a page of the tags added to the current user's tasks.</summary>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of tags on a page.</param>
    /// <returns>The tags on the requested page, sorted by name.</returns>
    Task<IReadOnlyList<TagSummary>> GetAllAsync(int pageNumber, int pageSize);

    /// <summary>Gets a page of the current user's tasks tagged with a tag.</summary>
    /// <param name="tagName">The tag name.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of tasks on a page.</param>
    /// <returns>The tagged tasks on the requested page.</returns>
    Task<IReadOnlyList<TodoTask>> GetTasksAsync(string tagName, int pageNumber, int pageSize);

    /// <summary>Adds a tag to a task.</summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="tagName">The tag name.</param>
    /// <returns><c>true</c> if the task has the tag; <c>false</c> if the task does not exist.</returns>
    Task<bool> AddAsync(int taskId, string tagName);

    /// <summary>Removes a tag from a task.</summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="tagName">The tag name.</param>
    /// <returns><c>true</c> if removed; <c>false</c> if the task does not exist or does not have the tag.</returns>
    Task<bool> RemoveAsync(int taskId, string tagName);
}
