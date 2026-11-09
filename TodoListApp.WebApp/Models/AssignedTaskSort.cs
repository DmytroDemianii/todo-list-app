namespace TodoListApp.WebApp.Models;

/// <summary>
/// Describes the order of tasks on the assigned tasks page.
/// </summary>
public enum AssignedTaskSort
{
    /// <summary>Tasks are shown in the order they were created.</summary>
    Default = 0,

    /// <summary>Tasks are sorted by name from A to Z.</summary>
    TitleAscending = 1,

    /// <summary>Tasks are sorted by name from Z to A.</summary>
    TitleDescending = 2,

    /// <summary>Tasks with the earliest due date go first.</summary>
    DueDateAscending = 3,

    /// <summary>Tasks with the latest due date go first.</summary>
    DueDateDescending = 4,
}
