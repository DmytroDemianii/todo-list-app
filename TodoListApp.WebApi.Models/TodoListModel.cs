using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApi.Models;

/// <summary>
/// The to-do list model used by the to-do list endpoints of the web API and by its clients.
/// </summary>
public class TodoListModel
{
    /// <summary>Gets or sets the to-do list identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the to-do list title.</summary>
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the to-do list description.</summary>
    [MaxLength(500)]
    public string? Description { get; set; }
}
