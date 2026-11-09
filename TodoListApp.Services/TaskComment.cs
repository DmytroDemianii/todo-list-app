namespace TodoListApp.Services;

/// <summary>
/// Represents a comment added to a task.
/// </summary>
public class TaskComment
{
    /// <summary>Gets or sets the comment identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the identifier of the commented task.</summary>
    public int TaskId { get; set; }

    /// <summary>Gets or sets the comment text.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Gets or sets the identifier of the user who added the comment.</summary>
    public string AuthorId { get; set; } = string.Empty;

    /// <summary>Gets or sets the date and time (UTC) when the comment was added.</summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>Gets or sets the date and time (UTC) when the comment was last edited, if it was.</summary>
    public DateTime? UpdatedDate { get; set; }
}
