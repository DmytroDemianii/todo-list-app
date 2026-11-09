using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models;

/// <summary>
/// The to-do list model used by the to-do list views.
/// </summary>
public class TodoListViewModel
{
    /// <summary>Gets or sets the to-do list identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the to-do list title.</summary>
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the to-do list description.</summary>
    [StringLength(500)]
    public string? Description { get; set; }
}
