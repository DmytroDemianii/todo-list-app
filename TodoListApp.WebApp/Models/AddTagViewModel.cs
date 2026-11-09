using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models;

/// <summary>
/// The form model used to add a tag to a task.
/// </summary>
public class AddTagViewModel
{
    /// <summary>Gets or sets the tag name.</summary>
    [Required(ErrorMessage = "Enter a tag.")]
    [StringLength(50, ErrorMessage = "A tag can be up to 50 characters long.")]
    [RegularExpression(@"^\s*[\p{L}\p{N}][\p{L}\p{N} _-]*\s*$", ErrorMessage = "A tag can contain only letters, digits, spaces, '-' and '_'.")]
    [Display(Name = "New tag")]
    public string Name { get; set; } = string.Empty;
}
