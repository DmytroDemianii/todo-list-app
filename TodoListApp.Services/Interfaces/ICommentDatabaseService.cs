namespace TodoListApp.Services.Interfaces;

/// <summary>
/// Manages the comments added to tasks stored in the database.
/// </summary>
public interface ICommentDatabaseService
{
    /// <summary>Gets a page of the comments of a task the user has access to, newest first.</summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="userId">The identifier of the to-do list owner or the task assignee.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of comments on a page.</param>
    /// <returns>The comments on the requested page, or <c>null</c> if the task does not exist.</returns>
    Task<IReadOnlyList<TaskComment>?> GetByTaskAsync(int taskId, string userId, int pageNumber, int pageSize);

    /// <summary>Gets a comment of a task the user has access to.</summary>
    /// <param name="id">The comment identifier.</param>
    /// <param name="userId">The identifier of the to-do list owner or the task assignee.</param>
    /// <returns>The comment, or <c>null</c> if it does not exist.</returns>
    Task<TaskComment?> GetByIdAsync(int id, string userId);

    /// <summary>Adds a comment to a task of the user's to-do list.</summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="text">The comment text, up to 1000 characters.</param>
    /// <param name="ownerId">The identifier of the to-do list owner, who becomes the comment author.</param>
    /// <returns>The added comment, or <c>null</c> if the task does not exist.</returns>
    Task<TaskComment?> AddAsync(int taskId, string text, string ownerId);

    /// <summary>Changes the text of a comment on a task of the user's to-do list.</summary>
    /// <param name="id">The comment identifier.</param>
    /// <param name="text">The new comment text, up to 1000 characters.</param>
    /// <param name="ownerId">The identifier of the to-do list owner.</param>
    /// <returns><c>true</c> if updated; <c>false</c> if the comment does not exist.</returns>
    Task<bool> UpdateAsync(int id, string text, string ownerId);

    /// <summary>Deletes a comment on a task of the user's to-do list.</summary>
    /// <param name="id">The comment identifier.</param>
    /// <param name="ownerId">The identifier of the to-do list owner.</param>
    /// <returns><c>true</c> if deleted; <c>false</c> if the comment does not exist.</returns>
    Task<bool> DeleteAsync(int id, string ownerId);
}
