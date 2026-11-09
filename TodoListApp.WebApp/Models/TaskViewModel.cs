using System.ComponentModel.DataAnnotations;
using TodoListApp.Services;

namespace TodoListApp.WebApp.Models;

/// <summary>
/// The task model used by the task views.
/// </summary>
public class TaskViewModel
{
    /// <summary>Gets or sets the task identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the identifier of the to-do list the task belongs to.</summary>
    public int TodoListId { get; set; }

    /// <summary>Gets or sets the task title.</summary>
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the task description.</summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>Gets or sets the date and time (UTC) when the task was created.</summary>
    [Display(Name = "Created")]
    public DateTime CreatedDate { get; set; }

    /// <summary>Gets or sets the task due date.</summary>
    [Display(Name = "Due date")]
    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    /// <summary>Gets or sets the task status.</summary>
    public TaskItemStatus Status { get; set; }

    /// <summary>Gets or sets the identifier of the user the task is assigned to.</summary>
    [Display(Name = "Assignee")]
    public string AssigneeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the email of the user the task is assigned to, or <c>null</c> if it is unknown.</summary>
    public string? AssigneeName { get; set; }

    /// <summary>Gets or sets a value indicating whether the task is overdue.</summary>
    public bool IsOverdue { get; set; }

    /// <summary>Gets or sets the names of the tags added to the task.</summary>
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();

    /// <summary>Gets or sets the comments on the current comment page, newest first.</summary>
    public IReadOnlyList<CommentViewModel> Comments { get; set; } = Array.Empty<CommentViewModel>();

    /// <summary>Gets or sets the current comment page number.</summary>
    public int CommentPageNumber { get; set; } = 1;

    /// <summary>Gets or sets a value indicating whether there is a next comment page.</summary>
    public bool HasNextCommentPage { get; set; }
}
