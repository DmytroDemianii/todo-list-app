using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.Services.Database;

/// <summary>
/// Manages the to-do lists stored in the database.
/// </summary>
public class TodoListDatabaseService : ITodoListDatabaseService
{
    private readonly TodoListDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="TodoListDatabaseService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public TodoListDatabaseService(TodoListDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc/>
    public Task<TodoList> AddAsync(TodoList todoList, string ownerId)
    {
        ArgumentNullException.ThrowIfNull(todoList);
        ArgumentException.ThrowIfNullOrEmpty(ownerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(todoList.Title);

        return this.AddCoreAsync(todoList, ownerId);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id, string ownerId)
    {
        ArgumentException.ThrowIfNullOrEmpty(ownerId);

        var entity = await this.dbContext.TodoLists
            .FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == ownerId);

        if (entity is null)
        {
            return false;
        }

        _ = this.dbContext.TodoLists.Remove(entity);
        _ = await this.dbContext.SaveChangesAsync();

        return true;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TodoList>> GetAllAsync(string ownerId, int pageNumber, int pageSize)
    {
        ArgumentException.ThrowIfNullOrEmpty(ownerId);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var entities = await this.dbContext.TodoLists
            .AsNoTracking()
            .Where(x => x.OwnerId == ownerId)
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return entities.Select(ToDomain).ToList();
    }

    /// <inheritdoc/>
    public async Task<TodoList?> GetByIdAsync(int id, string ownerId)
    {
        ArgumentException.ThrowIfNullOrEmpty(ownerId);

        var entity = await this.dbContext.TodoLists
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == ownerId);

        return entity is null ? null : ToDomain(entity);
    }

    /// <inheritdoc/>
    public Task<bool> UpdateAsync(TodoList todoList, string ownerId)
    {
        ArgumentNullException.ThrowIfNull(todoList);
        ArgumentException.ThrowIfNullOrEmpty(ownerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(todoList.Title);

        return this.UpdateCoreAsync(todoList, ownerId);
    }

    private static TodoList ToDomain(TodoListEntity entity) => new ()
    {
        Id = entity.Id,
        Title = entity.Title,
        Description = entity.Description,
        OwnerId = entity.OwnerId,
    };

    private async Task<TodoList> AddCoreAsync(TodoList todoList, string ownerId)
    {
        var entity = new TodoListEntity
        {
            Title = todoList.Title,
            Description = todoList.Description,
            OwnerId = ownerId,
        };

        _ = await this.dbContext.TodoLists.AddAsync(entity);
        _ = await this.dbContext.SaveChangesAsync();

        return ToDomain(entity);
    }

    private async Task<bool> UpdateCoreAsync(TodoList todoList, string ownerId)
    {
        var entity = await this.dbContext.TodoLists
            .FirstOrDefaultAsync(x => x.Id == todoList.Id && x.OwnerId == ownerId);

        if (entity is null)
        {
            return false;
        }

        entity.Title = todoList.Title;
        entity.Description = todoList.Description;
        _ = await this.dbContext.SaveChangesAsync();

        return true;
    }
}
