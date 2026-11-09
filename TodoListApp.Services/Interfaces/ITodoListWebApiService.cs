namespace TodoListApp.Services.Interfaces;

/// <summary>
/// Manages to-do lists using the to-do list web API.
/// </summary>
public interface ITodoListWebApiService
{
    /// <summary>Gets a page of the current user's to-do lists.</summary>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of to-do lists on a page.</param>
    /// <returns>The to-do lists on the requested page.</returns>
    Task<IReadOnlyList<TodoList>> GetAllAsync(int pageNumber, int pageSize);

    /// <summary>Gets a to-do list by its identifier.</summary>
    /// <param name="id">The to-do list identifier.</param>
    /// <returns>The to-do list, or <c>null</c> if it does not exist.</returns>
    Task<TodoList?> GetByIdAsync(int id);

    /// <summary>Creates a new to-do list.</summary>
    /// <param name="todoList">The to-do list data.</param>
    /// <returns>The created to-do list.</returns>
    Task<TodoList> AddAsync(TodoList todoList);

    /// <summary>Updates an existing to-do list.</summary>
    /// <param name="todoList">The new to-do list data.</param>
    /// <returns><c>true</c> if updated; <c>false</c> if the list does not exist.</returns>
    Task<bool> UpdateAsync(TodoList todoList);

    /// <summary>Deletes a to-do list.</summary>
    /// <param name="id">The to-do list identifier.</param>
    /// <returns><c>true</c> if deleted; <c>false</c> if the list does not exist.</returns>
    Task<bool> DeleteAsync(int id);
}
