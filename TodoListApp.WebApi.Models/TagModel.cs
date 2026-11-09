namespace TodoListApp.WebApi.Models;

/// <summary>
/// The tag model returned by the tag list endpoint.
/// </summary>
public class TagModel
{
    /// <summary>Gets or sets the tag name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of tasks tagged with the tag.</summary>
    public int TaskCount { get; set; }
}
