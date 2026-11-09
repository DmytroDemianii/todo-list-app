using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Services.Database.Entities;

/// <summary>
/// Represents a task of a to-do list stored in the database.
/// </summary>
public class TaskEntity
{
    /// <summary>Gets or sets the task identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the task title.</summary>
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the task description.</summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>Gets or sets the date and time (UTC) when the task was created.</summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>Gets or sets the task due date.</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Gets or sets the task status.</summary>
    public TaskItemStatus Status { get; set; }

    /// <summary>Gets or sets the identifier of the user the task is assigned to.</summary>
    [Required]
    [MaxLength(450)]
    public string AssigneeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the identifier of the to-do list the task belongs to.</summary>
    public int TodoListId { get; set; }

    /// <summary>Gets or sets the to-do list the task belongs to.</summary>
    public TodoListEntity TodoList { get; set; } = null!;

    /// <summary>Gets the tags added to the task.</summary>
    public ICollection<TaskTagEntity> Tags { get; } = new List<TaskTagEntity>();

    /// <summary>Gets the comments added to the task.</summary>
    public ICollection<TaskCommentEntity> Comments { get; } = new List<TaskCommentEntity>();
}
