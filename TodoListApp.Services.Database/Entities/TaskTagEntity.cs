using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Services.Database.Entities;

/// <summary>
/// Represents a tag added to a task, stored in the database.
/// </summary>
public class TaskTagEntity
{
    /// <summary>Gets or sets the identifier of the tag record.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the tag name.</summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the identifier of the tagged task.</summary>
    public int TaskId { get; set; }

    /// <summary>Gets or sets the tagged task.</summary>
    public TaskEntity Task { get; set; } = null!;
}
