namespace TodoListApp.Services;

/// <summary>
/// Describes the task property tasks are sorted by.
/// </summary>
public enum TaskSortBy
{
    /// <summary>Tasks are sorted in the order they were created.</summary>
    None = 0,

    /// <summary>Tasks are sorted by title.</summary>
    Title = 1,

    /// <summary>Tasks are sorted by due date; tasks without a due date always go last.</summary>
    DueDate = 2,
}
