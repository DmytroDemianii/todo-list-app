using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;
using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.Services.Database;

/// <summary>
/// Manages the tags added to tasks stored in the database.
/// </summary>
public partial class TagDatabaseService : ITagDatabaseService
{
    private const int MaxTagNameLength = 50;

    private readonly TodoListDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagDatabaseService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public TagDatabaseService(TodoListDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TagSummary>> GetAllAsync(string userId, int pageNumber, int pageSize)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        return await this.AccessibleTags(userId)
            .GroupBy(x => x.Name)
            .Select(x => new TagSummary { Name = x.Key, TaskCount = x.Count() })
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TodoTask>> GetTasksAsync(string userId, string tagName, int pageNumber, int pageSize)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        var name = NormalizeTagName(tagName);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var entities = await this.dbContext.Tasks
            .AsNoTracking()
            .Include(x => x.Tags)
            .Where(x => (x.TodoList.OwnerId == userId || x.AssigneeId == userId) && x.Tags.Any(t => t.Name == name))
            .OrderBy(x => x.DueDate == null)
            .ThenBy(x => x.DueDate)
            .ThenBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return entities.Select(TaskDatabaseService.ToDomain).ToList();
    }

    /// <inheritdoc/>
    public async Task<bool> AddAsync(int taskId, string tagName, string ownerId)
    {
        ArgumentException.ThrowIfNullOrEmpty(ownerId);
        var name = NormalizeTagName(tagName);

        if (!await this.dbContext.Tasks.AnyAsync(x => x.Id == taskId && x.TodoList.OwnerId == ownerId))
        {
            return false;
        }

        if (!await this.dbContext.TaskTags.AnyAsync(x => x.TaskId == taskId && x.Name == name))
        {
            _ = await this.dbContext.TaskTags.AddAsync(new TaskTagEntity { TaskId = taskId, Name = name });
            _ = await this.dbContext.SaveChangesAsync();
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveAsync(int taskId, string tagName, string ownerId)
    {
        ArgumentException.ThrowIfNullOrEmpty(ownerId);
        var name = NormalizeTagName(tagName);

        var entity = await this.dbContext.TaskTags
            .FirstOrDefaultAsync(x => x.TaskId == taskId && x.Name == name && x.Task.TodoList.OwnerId == ownerId);

        if (entity is null)
        {
            return false;
        }

        _ = this.dbContext.TaskTags.Remove(entity);
        _ = await this.dbContext.SaveChangesAsync();

        return true;
    }

    /// <summary>Trims a tag name and checks that it is valid.</summary>
    /// <param name="tagName">The tag name.</param>
    /// <returns>The trimmed tag name.</returns>
    /// <exception cref="ArgumentException">The tag name is empty, too long or contains invalid characters.</exception>
    private static string NormalizeTagName(string tagName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tagName);

        var name = tagName.Trim();
        if (name.Length > MaxTagNameLength || !TagNameRegex().IsMatch(name))
        {
            throw new ArgumentException(
                $"A tag must be up to {MaxTagNameLength} characters long and contain only letters, digits, spaces, '-' and '_'.",
                nameof(tagName));
        }

        return name;
    }

    [GeneratedRegex(@"^[\p{L}\p{N}][\p{L}\p{N} _-]*$")]
    private static partial Regex TagNameRegex();

    private IQueryable<TaskTagEntity> AccessibleTags(string userId) =>
        this.dbContext.TaskTags
            .AsNoTracking()
            .Where(x => x.Task.TodoList.OwnerId == userId || x.Task.AssigneeId == userId);
}
