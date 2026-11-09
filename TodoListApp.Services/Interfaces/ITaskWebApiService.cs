namespace TodoListApp.Services.Interfaces;

/// <summary>
/// Manages to-do list tasks using the web API.
/// </summary>
public interface ITaskWebApiService
{
    /// <summary>Gets a page of tasks of a to-do list.</summary>
    /// <param name="todoListId">The to-do list identifier.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of tasks on a page.</param>
    /// <returns>The tasks on the requested page, or <c>null</c> if the to-do list does not exist.</returns>
    Task<IReadOnlyList<TodoTask>?> GetByTodoListAsync(int todoListId, int pageNumber, int pageSize);

    /// <summary>Gets a page of tasks assigned to the current user.</summary>
    /// <param name="statuses">The statuses of the tasks to return; all tasks are returned if it is empty.</param>
    /// <param name="sortBy">The task property to sort by.</param>
    /// <param name="descending"><c>true</c> to sort in descending order; otherwise, <c>false</c>.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of tasks on a page.</param>
    /// <returns>The tasks on the requested page.</returns>
    Task<IReadOnlyList<TodoTask>> GetAssignedAsync(IEnumerable<TaskItemStatus> statuses, TaskSortBy sortBy, bool descending, int pageNumber, int pageSize);

    /// <summary>Searches the current user's tasks. Only the specified criteria are applied.</summary>
    /// <param name="title">The text the task title must contain.</param>
    /// <param name="createdDate">The date when the task was created.</param>
    /// <param name="dueDate">The task due date.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of tasks on a page.</param>
    /// <returns>The found tasks on the requested page.</returns>
    Task<IReadOnlyList<TodoTask>> SearchAsync(string? title, DateTime? createdDate, DateTime? dueDate, int pageNumber, int pageSize);

    /// <summary>Gets a task by its identifier.</summary>
    /// <param name="id">The task identifier.</param>
    /// <returns>The task, or <c>null</c> if it does not exist.</returns>
    Task<TodoTask?> GetByIdAsync(int id);

    /// <summary>Adds a new task to a to-do list.</summary>
    /// <param name="todoListId">The to-do list identifier.</param>
    /// <param name="task">The task data.</param>
    /// <returns>The created task, or <c>null</c> if the to-do list does not exist.</returns>
    Task<TodoTask?> AddAsync(int todoListId, TodoTask task);

    /// <summary>Updates a task.</summary>
    /// <param name="task">The new task data.</param>
    /// <returns><c>true</c> if updated; <c>false</c> if the task does not exist.</returns>
    Task<bool> UpdateAsync(TodoTask task);

    /// <summary>Changes the status of a task.</summary>
    /// <param name="id">The task identifier.</param>
    /// <param name="status">The new task status.</param>
    /// <returns><c>true</c> if updated; <c>false</c> if the task does not exist.</returns>
    Task<bool> UpdateStatusAsync(int id, TaskItemStatus status);

    /// <summary>Deletes a task.</summary>
    /// <param name="id">The task identifier.</param>
    /// <returns><c>true</c> if deleted; <c>false</c> if the task does not exist.</returns>
    Task<bool> DeleteAsync(int id);
}
