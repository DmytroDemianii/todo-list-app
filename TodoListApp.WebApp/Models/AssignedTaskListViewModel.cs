namespace TodoListApp.WebApp.Models;

/// <summary>
/// The model of the page that shows the tasks assigned to the current user.
/// </summary>
public class AssignedTaskListViewModel
{
    /// <summary>Gets or sets the tasks on the current page.</summary>
    public IReadOnlyList<TaskViewModel> Tasks { get; set; } = Array.Empty<TaskViewModel>();

    /// <summary>Gets or sets the status filter applied to the tasks.</summary>
    public AssignedTaskFilter Filter { get; set; }

    /// <summary>Gets or sets the order of the tasks.</summary>
    public AssignedTaskSort Sort { get; set; }

    /// <summary>Gets or sets the current page number.</summary>
    public int PageNumber { get; set; }

    /// <summary>Gets or sets a value indicating whether there is a next page.</summary>
    public bool HasNextPage { get; set; }
}
