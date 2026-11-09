using System.ComponentModel.DataAnnotations;
using TodoListApp.Services;

namespace TodoListApp.WebApi.Models;

/// <summary>
/// The task model used by the task endpoints. Read-only properties are set by the server and ignored on input.
/// </summary>
public class TodoTaskModel
{
    /// <summary>Gets or sets the task identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the identifier of the to-do list the task belongs to.</summary>
    public int TodoListId { get; set; }

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
    [EnumDataType(typeof(TaskItemStatus))]
    public TaskItemStatus Status { get; set; }

    /// <summary>Gets or sets the identifier of the user the task is assigned to.</summary>
    public string AssigneeId { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the task is overdue.</summary>
    public bool IsOverdue { get; set; }

    /// <summary>Gets or sets the names of the tags added to the task. Tags are managed by the tag endpoints.</summary>
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();
}
