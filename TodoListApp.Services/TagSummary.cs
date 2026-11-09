namespace TodoListApp.Services;

/// <summary>
/// Represents a tag and the number of the user's tasks tagged with it.
/// </summary>
public class TagSummary
{
    /// <summary>Gets or sets the tag name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of tasks tagged with the tag.</summary>
    public int TaskCount { get; set; }
}
