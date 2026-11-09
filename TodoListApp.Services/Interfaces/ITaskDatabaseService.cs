namespace TodoListApp.Services.Interfaces;

/// <summary>
/// Manages to-do list tasks stored in the database.
/// </summary>
public interface ITaskDatabaseService
{
    /// <summary>Gets a page of tasks of a to-do list.</summary>
    /// <param name="todoListId">The to-do list identifier.</param>
    /// <param name="ownerId">The identifier of the to-do list owner.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of tasks on a page.</param>
    /// <returns>The tasks on the requested page, or <c>null</c> if the to-do list does not exist.</returns>
    Task<IReadOnlyList<TodoTask>?> GetByTodoListAsync(int todoListId, string ownerId, int pageNumber, int pageSize);

    /// <summary>Gets a page of tasks assigned to a user.</summary>
    /// <param name="assigneeId">The identifier of the user the tasks are assigned to.</param>
    /// <param name="statuses">The statuses of the tasks to return; all tasks are returned if it is empty.</param>
    /// <param name="sortBy">The task property to sort by.</param>
    /// <param name="descending"><c>true</c> to sort in descending order; otherwise, <c>false</c>.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of tasks on a page.</param>
    /// <returns>The tasks on the requested page.</returns>
    Task<IReadOnlyList<TodoTask>> GetAssignedAsync(string assigneeId, IReadOnlyCollection<TaskItemStatus> statuses, TaskSortBy sortBy, bool descending, int pageNumber, int pageSize);

    /// <summary>Searches the tasks the user has access to: tasks of the user's to-do lists and tasks assigned to the user.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="criteria">The search criteria; at least one criterion must be specified.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of tasks on a page.</param>
    /// <returns>The found tasks on the requested page, sorted by due date.</returns>
    Task<IReadOnlyList<TodoTask>> SearchAsync(string userId, TaskSearchCriteria criteria, int pageNumber, int pageSize);

    /// <summary>Gets a task by its identifier.</summary>
    /// <param name="id">The task identifier.</param>
    /// <param name="ownerId">The identifier of the to-do list owner.</param>
    /// <returns>The task, or <c>null</c> if it does not exist.</returns>
    Task<TodoTask?> GetByIdAsync(int id, string ownerId);

    /// <summary>Adds a new task to a to-do list and assigns it to the owner.</summary>
    /// <param name="todoListId">The to-do list identifier.</param>
    /// <param name="task">The task data.</param>
    /// <param name="ownerId">The identifier of the to-do list owner.</param>
    /// <returns>The created task, or <c>null</c> if the to-do list does not exist.</returns>
    Task<TodoTask?> AddAsync(int todoListId, TodoTask task, string ownerId);

    /// <summary>Updates the title, description, due date and status of a task.</summary>
    /// <param name="task">The new task data.</param>
    /// <param name="ownerId">The identifier of the to-do list owner.</param>
    /// <returns><c>true</c> if updated; <c>false</c> if the task does not exist.</returns>
    Task<bool> UpdateAsync(TodoTask task, string ownerId);

    /// <summary>Changes the status of a task assigned to the user or of a task in the user's to-do list.</summary>
    /// <param name="id">The task identifier.</param>
    /// <param name="status">The new task status.</param>
    /// <param name="userId">The identifier of the assignee or the to-do list owner.</param>
    /// <returns><c>true</c> if updated; <c>false</c> if the task does not exist or the user has no access to it.</returns>
    Task<bool> UpdateStatusAsync(int id, TaskItemStatus status, string userId);

    /// <summary>Deletes a task.</summary>
    /// <param name="id">The task identifier.</param>
    /// <param name="ownerId">The identifier of the to-do list owner.</param>
    /// <returns><c>true</c> if deleted; <c>false</c> if the task does not exist.</returns>
    Task<bool> DeleteAsync(int id, string ownerId);
}
