using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;
using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Interfaces;

namespace TodoListApp.Services.Database;

/// <summary>
/// Manages the comments added to tasks stored in the database.
/// </summary>
public class CommentDatabaseService : ICommentDatabaseService
{
    private const int MaxTextLength = 1000;

    private readonly TodoListDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentDatabaseService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public CommentDatabaseService(TodoListDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TaskComment>?> GetByTaskAsync(int taskId, string userId, int pageNumber, int pageSize)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        if (!await this.dbContext.Tasks.AnyAsync(x => x.Id == taskId && (x.TodoList.OwnerId == userId || x.AssigneeId == userId)))
        {
            return null;
        }

        var entities = await this.dbContext.TaskComments
            .AsNoTracking()
            .Where(x => x.TaskId == taskId)
            .OrderByDescending(x => x.CreatedDate)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return entities.Select(ToDomain).ToList();
    }

    /// <inheritdoc/>
    public async Task<TaskComment?> GetByIdAsync(int id, string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        var entity = await this.dbContext.TaskComments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && (x.Task.TodoList.OwnerId == userId || x.Task.AssigneeId == userId));

        return entity is null ? null : ToDomain(entity);
    }

    /// <inheritdoc/>
    public async Task<TaskComment?> AddAsync(int taskId, string text, string ownerId)
    {
        ArgumentException.ThrowIfNullOrEmpty(ownerId);
        var normalizedText = NormalizeText(text);

        if (!await this.dbContext.Tasks.AnyAsync(x => x.Id == taskId && x.TodoList.OwnerId == ownerId))
        {
            return null;
        }

        var entity = new TaskCommentEntity
        {
            TaskId = taskId,
            Text = normalizedText,
            AuthorId = ownerId,
            CreatedDate = DateTime.UtcNow,
        };

        _ = await this.dbContext.TaskComments.AddAsync(entity);
        _ = await this.dbContext.SaveChangesAsync();

        return ToDomain(entity);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(int id, string text, string ownerId)
    {
        ArgumentException.ThrowIfNullOrEmpty(ownerId);
        var normalizedText = NormalizeText(text);

        var entity = await this.dbContext.TaskComments
            .FirstOrDefaultAsync(x => x.Id == id && x.Task.TodoList.OwnerId == ownerId);

        if (entity is null)
        {
            return false;
        }

        entity.Text = normalizedText;
        entity.UpdatedDate = DateTime.UtcNow;
        _ = await this.dbContext.SaveChangesAsync();

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id, string ownerId)
    {
        ArgumentException.ThrowIfNullOrEmpty(ownerId);

        var entity = await this.dbContext.TaskComments
            .FirstOrDefaultAsync(x => x.Id == id && x.Task.TodoList.OwnerId == ownerId);

        if (entity is null)
        {
            return false;
        }

        _ = this.dbContext.TaskComments.Remove(entity);
        _ = await this.dbContext.SaveChangesAsync();

        return true;
    }

    private static TaskComment ToDomain(TaskCommentEntity entity) => new ()
    {
        Id = entity.Id,
        TaskId = entity.TaskId,
        Text = entity.Text,
        AuthorId = entity.AuthorId,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
    };

    /// <summary>Trims a comment text and checks that it is valid.</summary>
    /// <param name="text">The comment text.</param>
    /// <returns>The trimmed text.</returns>
    /// <exception cref="ArgumentException">The text is empty or too long.</exception>
    private static string NormalizeText(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var normalizedText = text.Trim();
        if (normalizedText.Length > MaxTextLength)
        {
            throw new ArgumentException("A comment can be up to 1000 characters long.", nameof(text));
        }

        return normalizedText;
    }
}
