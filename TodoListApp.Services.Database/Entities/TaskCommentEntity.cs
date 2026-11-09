using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Services.Database.Entities;

/// <summary>
/// Represents a comment added to a task, stored in the database.
/// </summary>
public class TaskCommentEntity
{
    /// <summary>Gets or sets the comment identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the comment text.</summary>
    [Required]
    [MaxLength(1000)]
    public string Text { get; set; } = string.Empty;

    /// <summary>Gets or sets the identifier of the user who added the comment.</summary>
    [Required]
    [MaxLength(450)]
    public string AuthorId { get; set; } = string.Empty;

    /// <summary>Gets or sets the date and time (UTC) when the comment was added.</summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>Gets or sets the date and time (UTC) when the comment was last edited, if it was.</summary>
    public DateTime? UpdatedDate { get; set; }

    /// <summary>Gets or sets the identifier of the commented task.</summary>
    public int TaskId { get; set; }

    /// <summary>Gets or sets the commented task.</summary>
    public TaskEntity Task { get; set; } = null!;
}
