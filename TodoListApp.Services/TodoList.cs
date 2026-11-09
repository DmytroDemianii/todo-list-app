namespace TodoListApp.Services;

/// <summary>
/// Represents a to-do list.
/// </summary>
public class TodoList
{
    /// <summary>Gets or sets the to-do list identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the to-do list title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the to-do list description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the identifier of the user who owns the to-do list.</summary>
    public string OwnerId { get; set; } = string.Empty;
}
