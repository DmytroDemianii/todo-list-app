namespace TodoListApp.WebApp.Models;

/// <summary>
/// Describes which tasks are shown on the assigned tasks page.
/// </summary>
public enum AssignedTaskFilter
{
    /// <summary>Tasks that are not completed: not started or in progress.</summary>
    Active = 0,

    /// <summary>Tasks that have not been started yet.</summary>
    NotStarted = 1,

    /// <summary>Tasks that are in progress.</summary>
    InProgress = 2,

    /// <summary>Completed tasks.</summary>
    Completed = 3,

    /// <summary>All tasks.</summary>
    All = 4,
}
