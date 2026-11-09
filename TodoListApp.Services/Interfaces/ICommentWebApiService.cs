namespace TodoListApp.Services.Interfaces;

/// <summary>
/// Manages the comments of tasks using the web API.
/// </summary>
public interface ICommentWebApiService
{
    /// <summary>Gets a page of the comments of a task, newest first.</summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of comments on a page.</param>
    /// <returns>The comments on the requested page, or <c>null</c> if the task does not exist.</returns>
    Task<IReadOnlyList<TaskComment>?> GetByTaskAsync(int taskId, int pageNumber, int pageSize);

    /// <summary>Gets a comment by its identifier.</summary>
    /// <param name="id">The comment identifier.</param>
    /// <returns>The comment, or <c>null</c> if it does not exist.</returns>
    Task<TaskComment?> GetByIdAsync(int id);

    /// <summary>Adds a comment to a task.</summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="text">The comment text.</param>
    /// <returns><c>true</c> if added; <c>false</c> if the task does not exist.</returns>
    Task<bool> AddAsync(int taskId, string text);

    /// <summary>Changes the text of a comment.</summary>
    /// <param name="id">The comment identifier.</param>
    /// <param name="text">The new comment text.</param>
    /// <returns><c>true</c> if updated; <c>false</c> if the comment does not exist.</returns>
    Task<bool> UpdateAsync(int id, string text);

    /// <summary>Deletes a comment.</summary>
    /// <param name="id">The comment identifier.</param>
    /// <returns><c>true</c> if deleted; <c>false</c> if the comment does not exist.</returns>
    Task<bool> DeleteAsync(int id);
}
