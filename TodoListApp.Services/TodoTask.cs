namespace TodoListApp.Services;

/// <summary>
/// Represents a task of a to-do list.
/// </summary>
public class TodoTask
{
    /// <summary>Gets or sets the task identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the identifier of the to-do list the task belongs to.</summary>
    public int TodoListId { get; set; }

    /// <summary>Gets or sets the task title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the task description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the date and time (UTC) when the task was created.</summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>Gets or sets the task due date.</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Gets or sets the task status.</summary>
    public TaskItemStatus Status { get; set; }

    /// <summary>Gets or sets the identifier of the user the task is assigned to.</summary>
    public string AssigneeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the names of the tags added to the task.</summary>
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets a value indicating whether the task is overdue: it is not completed and its due date has passed.
    /// </summary>
    public bool IsOverdue =>
        this.Status != TaskItemStatus.Completed
        && this.DueDate.HasValue
        && this.DueDate.Value.Date < DateTime.UtcNow.Date;
}
