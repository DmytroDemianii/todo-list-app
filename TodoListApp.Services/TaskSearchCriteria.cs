namespace TodoListApp.Services;

/// <summary>
/// Describes the criteria to search tasks by. Only the specified criteria are applied.
/// </summary>
public class TaskSearchCriteria
{
    /// <summary>Gets or sets the text the task title must contain.</summary>
    public string? Title { get; set; }

    /// <summary>Gets or sets the date (the time part is ignored) when the task was created.</summary>
    public DateTime? CreatedDate { get; set; }

    /// <summary>Gets or sets the task due date (the time part is ignored).</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Gets a value indicating whether no criteria are specified.</summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(this.Title) && !this.CreatedDate.HasValue && !this.DueDate.HasValue;
}
