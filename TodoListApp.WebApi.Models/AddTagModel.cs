using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApi.Models;

/// <summary>
/// The model used to add a tag to a task.
/// </summary>
public class AddTagModel
{
    /// <summary>Gets or sets the tag name.</summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}
