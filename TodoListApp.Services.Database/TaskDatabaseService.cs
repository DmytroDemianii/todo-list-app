using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;
using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.Services.Database;

/// <summary>
/// Manages to-do list tasks stored in the database.
/// </summary>
public class TaskDatabaseService : ITaskDatabaseService
{
    private readonly TodoListDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskDatabaseService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public TaskDatabaseService(TodoListDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TodoTask>?> GetByTodoListAsync(int todoListId, string ownerId, int pageNumber, int pageSize)
    {
        ArgumentException.ThrowIfNullOrEmpty(ownerId);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        if (!await this.OwnsTodoListAsync(todoListId, ownerId))
        {
            return null;
        }

        var entities = await this.dbContext.Tasks
            .AsNoTracking()
            .Where(x => x.TodoListId == todoListId)
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return entities.Select(ToDomain).ToList();
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<TodoTask>> GetAssignedAsync(string assigneeId, IReadOnlyCollection<TaskItemStatus> statuses, TaskSortBy sortBy, bool descending, int pageNumber, int pageSize)
    {
        ArgumentException.ThrowIfNullOrEmpty(assigneeId);
        ArgumentNullException.ThrowIfNull(statuses);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        if (!Enum.IsDefined(sortBy))
        {
            throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, "Unknown sort property.");
        }

        return this.GetAssignedCoreAsync(assigneeId, statuses, sortBy, descending, pageNumber, pageSize);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<TodoTask>> SearchAsync(string userId, TaskSearchCriteria criteria, int pageNumber, int pageSize)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentNullException.ThrowIfNull(criteria);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        if (criteria.IsEmpty)
        {
            throw new ArgumentException("At least one search criterion must be specified.", nameof(criteria));
        }

        return this.SearchCoreAsync(userId, criteria, pageNumber, pageSize);
    }

    /// <inheritdoc/>
    public async Task<TodoTask?> GetByIdAsync(int id, string ownerId)
    {
        ArgumentException.ThrowIfNullOrEmpty(ownerId);

        // The details page shows the task tags (US17).
        var entity = await this.dbContext.Tasks
            .AsNoTracking()
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.Id == id && x.TodoList.OwnerId == ownerId);

        return entity is null ? null : ToDomain(entity);
    }

    /// <inheritdoc/>
    public Task<TodoTask?> AddAsync(int todoListId, TodoTask task, string ownerId)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentException.ThrowIfNullOrEmpty(ownerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(task.Title);

        return this.AddCoreAsync(todoListId, task, ownerId);
    }

    /// <inheritdoc/>
    public Task<bool> UpdateAsync(TodoTask task, string ownerId)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentException.ThrowIfNullOrEmpty(ownerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(task.Title);

        return this.UpdateCoreAsync(task, ownerId);
    }

    /// <inheritdoc/>
    public Task<bool> UpdateStatusAsync(int id, TaskItemStatus status, string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown task status.");
        }

        return this.UpdateStatusCoreAsync(id, status, userId);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id, string ownerId)
    {
        ArgumentException.ThrowIfNullOrEmpty(ownerId);

        var entity = await this.dbContext.Tasks
            .FirstOrDefaultAsync(x => x.Id == id && x.TodoList.OwnerId == ownerId);

        if (entity is null)
        {
            return false;
        }

        _ = this.dbContext.Tasks.Remove(entity);
        _ = await this.dbContext.SaveChangesAsync();

        return true;
    }

    /// <summary>Converts a task entity to a task; the tags are copied only if they were loaded.</summary>
    /// <param name="entity">The task entity.</param>
    /// <returns>The task.</returns>
    internal static TodoTask ToDomain(TaskEntity entity) => new ()
    {
        Id = entity.Id,
        TodoListId = entity.TodoListId,
        Title = entity.Title,
        Description = entity.Description,
        CreatedDate = entity.CreatedDate,
        DueDate = entity.DueDate,
        Status = entity.Status,
        AssigneeId = entity.AssigneeId,
        Tags = entity.Tags.Select(x => x.Name).Order(StringComparer.OrdinalIgnoreCase).ToList(),
    };

    // Ties are broken by Id so that paging is stable; tasks without a due date always go last.
    private static IQueryable<TaskEntity> Sort(IQueryable<TaskEntity> query, TaskSortBy sortBy, bool descending) => (sortBy, descending) switch
    {
        (TaskSortBy.Title, false) => query.OrderBy(x => x.Title).ThenBy(x => x.Id),
        (TaskSortBy.Title, true) => query.OrderByDescending(x => x.Title).ThenBy(x => x.Id),
        (TaskSortBy.DueDate, false) => query.OrderBy(x => x.DueDate == null).ThenBy(x => x.DueDate).ThenBy(x => x.Id),
        (TaskSortBy.DueDate, true) => query.OrderBy(x => x.DueDate == null).ThenByDescending(x => x.DueDate).ThenBy(x => x.Id),
        _ => query.OrderBy(x => x.Id),
    };

    private Task<bool> OwnsTodoListAsync(int todoListId, string ownerId) =>
        this.dbContext.TodoLists.AnyAsync(x => x.Id == todoListId && x.OwnerId == ownerId);

    private async Task<IReadOnlyList<TodoTask>> GetAssignedCoreAsync(string assigneeId, IReadOnlyCollection<TaskItemStatus> statuses, TaskSortBy sortBy, bool descending, int pageNumber, int pageSize)
    {
        var query = this.dbContext.Tasks
            .AsNoTracking()
            .Where(x => x.AssigneeId == assigneeId);

        if (statuses.Count > 0)
        {
            query = query.Where(x => statuses.Contains(x.Status));
        }

        var entities = await Sort(query, sortBy, descending)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return entities.Select(ToDomain).ToList();
    }

    private async Task<IReadOnlyList<TodoTask>> SearchCoreAsync(string userId, TaskSearchCriteria criteria, int pageNumber, int pageSize)
    {
        var query = this.dbContext.Tasks
            .AsNoTracking()
            .Where(x => x.TodoList.OwnerId == userId || x.AssigneeId == userId);

        if (!string.IsNullOrWhiteSpace(criteria.Title))
        {
            var title = criteria.Title.Trim();
            query = query.Where(x => x.Title.Contains(title));
        }

        if (criteria.CreatedDate.HasValue)
        {
            var from = criteria.CreatedDate.Value.Date;
            var to = from.AddDays(1);
            query = query.Where(x => x.CreatedDate >= from && x.CreatedDate < to);
        }

        if (criteria.DueDate.HasValue)
        {
            var from = criteria.DueDate.Value.Date;
            var to = from.AddDays(1);
            query = query.Where(x => x.DueDate >= from && x.DueDate < to);
        }

        var entities = await Sort(query, TaskSortBy.DueDate, descending: false)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return entities.Select(ToDomain).ToList();
    }

    private async Task<TodoTask?> AddCoreAsync(int todoListId, TodoTask task, string ownerId)
    {
        if (!await this.OwnsTodoListAsync(todoListId, ownerId))
        {
            return null;
        }

        var entity = new TaskEntity
        {
            TodoListId = todoListId,
            Title = task.Title,
            Description = task.Description,
            CreatedDate = DateTime.UtcNow,
            DueDate = task.DueDate,
            Status = task.Status,
            AssigneeId = ownerId,
        };

        _ = await this.dbContext.Tasks.AddAsync(entity);
        _ = await this.dbContext.SaveChangesAsync();

        return ToDomain(entity);
    }

    private async Task<bool> UpdateCoreAsync(TodoTask task, string ownerId)
    {
        var entity = await this.dbContext.Tasks
            .FirstOrDefaultAsync(x => x.Id == task.Id && x.TodoList.OwnerId == ownerId);

        if (entity is null)
        {
            return false;
        }

        entity.Title = task.Title;
        entity.Description = task.Description;
        entity.DueDate = task.DueDate;
        entity.Status = task.Status;
        _ = await this.dbContext.SaveChangesAsync();

        return true;
    }

    private async Task<bool> UpdateStatusCoreAsync(int id, TaskItemStatus status, string userId)
    {
        var entity = await this.dbContext.Tasks
            .FirstOrDefaultAsync(x => x.Id == id && (x.AssigneeId == userId || x.TodoList.OwnerId == userId));

        if (entity is null)
        {
            return false;
        }

        entity.Status = status;
        _ = await this.dbContext.SaveChangesAsync();

        return true;
    }
}
