namespace TodoListApp.WebApp.Models;

/// <summary>
/// The model of the page that shows the tasks of a to-do list.
/// </summary>
public class TaskListViewModel
{
    /// <summary>Gets or sets the to-do list identifier.</summary>
    public int TodoListId { get; set; }

    /// <summary>Gets or sets the to-do list title.</summary>
    public string TodoListTitle { get; set; } = string.Empty;

    /// <summary>Gets or sets the tasks on the current page.</summary>
    public IReadOnlyList<TaskViewModel> Tasks { get; set; } = Array.Empty<TaskViewModel>();

    /// <summary>Gets or sets the current page number.</summary>
    public int PageNumber { get; set; }

    /// <summary>Gets or sets a value indicating whether there is a next page.</summary>
    public bool HasNextPage { get; set; }
}
