using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApi.Models;

/// <summary>
/// The model used to add or edit a comment.
/// </summary>
public class CommentTextModel
{
    /// <summary>Gets or sets the comment text.</summary>
    [Required]
    [MaxLength(1000)]
    public string Text { get; set; } = string.Empty;
}
