using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entities;

namespace TodoListApp.Services.Database;

/// <summary>
/// Represents the database context for the TodoListDb database.
/// </summary>
public class TodoListDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TodoListDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the context.</param>
    public TodoListDbContext(DbContextOptions<TodoListDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the to-do lists.
    /// </summary>
    public DbSet<TodoListEntity> TodoLists { get; set; }

    /// <summary>
    /// Gets or sets the tasks.
    /// </summary>
    public DbSet<TaskEntity> Tasks { get; set; }

    /// <summary>
    /// Gets or sets the tags added to tasks.
    /// </summary>
    public DbSet<TaskTagEntity> TaskTags { get; set; }

    /// <summary>
    /// Gets or sets the comments added to tasks.
    /// </summary>
    public DbSet<TaskCommentEntity> TaskComments { get; set; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        // Every query filters to-do lists by their owner.
        _ = modelBuilder.Entity<TodoListEntity>().HasIndex(x => x.OwnerId);

        // Tasks are deleted together with their to-do list.
        _ = modelBuilder.Entity<TaskEntity>()
            .HasOne(x => x.TodoList)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.TodoListId)
            .OnDelete(DeleteBehavior.Cascade);

        // The assigned tasks page (Epic 3) filters tasks by their assignee.
        _ = modelBuilder.Entity<TaskEntity>().HasIndex(x => x.AssigneeId);

        // Tags are deleted together with their task; a task cannot have the same tag twice (Epic 5).
        _ = modelBuilder.Entity<TaskTagEntity>()
            .HasOne(x => x.Task)
            .WithMany(x => x.Tags)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<TaskTagEntity>().HasIndex(x => new { x.TaskId, x.Name }).IsUnique();

        // The tags page and the tasks-by-tag page group and filter by the tag name.
        _ = modelBuilder.Entity<TaskTagEntity>().HasIndex(x => x.Name);

        // Comments are deleted together with their task (Epic 6); the foreign key also indexes TaskId.
        _ = modelBuilder.Entity<TaskCommentEntity>()
            .HasOne(x => x.Task)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
