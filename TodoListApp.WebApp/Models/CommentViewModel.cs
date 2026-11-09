using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models;

/// <summary>
/// The comment model used by the comment views and forms.
/// </summary>
public class CommentViewModel
{
    /// <summary>Gets or sets the comment identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the identifier of the commented task.</summary>
    public int TaskId { get; set; }

    /// <summary>Gets or sets the comment text.</summary>
    [Required(ErrorMessage = "Enter a comment.")]
    [StringLength(1000, ErrorMessage = "A comment can be up to 1000 characters long.")]
    [Display(Name = "Comment")]
    public string Text { get; set; } = string.Empty;

    /// <summary>Gets or sets the identifier of the user who added the comment.</summary>
    public string AuthorId { get; set; } = string.Empty;

    /// <summary>Gets or sets the email of the user who added the comment, or <c>null</c> if it is unknown.</summary>
    public string? AuthorName { get; set; }

    /// <summary>Gets or sets the date and time (UTC) when the comment was added.</summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>Gets or sets the date and time (UTC) when the comment was last edited, if it was.</summary>
    public DateTime? UpdatedDate { get; set; }
}
