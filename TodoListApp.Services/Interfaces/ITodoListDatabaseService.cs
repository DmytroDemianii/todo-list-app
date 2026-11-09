namespace TodoListApp.Services.Interfaces;

/// <summary>
/// Manages the to-do lists stored in the database.
/// </summary>
public interface ITodoListDatabaseService
{
    /// <summary>Gets a page of the user's to-do lists.</summary>
    /// <param name="ownerId">The identifier of the to-do list owner.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of to-do lists on a page.</param>
    /// <returns>The to-do lists on the requested page.</returns>
    Task<IReadOnlyList<TodoList>> GetAllAsync(string ownerId, int pageNumber, int pageSize);

    /// <summary>Gets a to-do list of the user by its identifier.</summary>
    /// <param name="id">The to-do list identifier.</param>
    /// <param name="ownerId">The identifier of the to-do list owner.</param>
    /// <returns>The to-do list, or <c>null</c> if it does not exist.</returns>
    Task<TodoList?> GetByIdAsync(int id, string ownerId);

    /// <summary>Creates a new to-do list owned by the user.</summary>
    /// <param name="todoList">The to-do list data.</param>
    /// <param name="ownerId">The identifier of the to-do list owner.</param>
    /// <returns>The created to-do list.</returns>
    Task<TodoList> AddAsync(TodoList todoList, string ownerId);

    /// <summary>Updates the title and description of a to-do list of the user.</summary>
    /// <param name="todoList">The new to-do list data.</param>
    /// <param name="ownerId">The identifier of the to-do list owner.</param>
    /// <returns><c>true</c> if updated; <c>false</c> if the to-do list does not exist.</returns>
    Task<bool> UpdateAsync(TodoList todoList, string ownerId);

    /// <summary>Deletes a to-do list of the user together with its tasks.</summary>
    /// <param name="id">The to-do list identifier.</param>
    /// <param name="ownerId">The identifier of the to-do list owner.</param>
    /// <returns><c>true</c> if deleted; <c>false</c> if the to-do list does not exist.</returns>
    Task<bool> DeleteAsync(int id, string ownerId);
}
