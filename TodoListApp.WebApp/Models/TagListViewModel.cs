using TodoListApp.Services;

namespace TodoListApp.WebApp.Models;

/// <summary>
/// The model of the page that shows all tags of the current user's tasks.
/// </summary>
public class TagListViewModel
{
    /// <summary>Gets or sets the tags on the current page.</summary>
    public IReadOnlyList<TagSummary> Tags { get; set; } = Array.Empty<TagSummary>();

    /// <summary>Gets or sets the current page number.</summary>
    public int PageNumber { get; set; }

    /// <summary>Gets or sets a value indicating whether there is a next page.</summary>
    public bool HasNextPage { get; set; }
}
