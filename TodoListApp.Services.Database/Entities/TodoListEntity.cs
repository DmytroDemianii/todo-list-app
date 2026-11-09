using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Services.Database.Entities;

/// <summary>
/// Represents a to-do list stored in the database.
/// </summary>
public class TodoListEntity
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

    /// <summary>Gets or sets the identifier of the user who owns the to-do list.</summary>
    [Required]
    [MaxLength(450)]
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>Gets the tasks of the to-do list.</summary>
    public ICollection<TaskEntity> Tasks { get; } = new List<TaskEntity>();
}
