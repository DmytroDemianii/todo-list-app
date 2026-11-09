using TodoListApp.Services;

namespace TodoListApp.WebApp.Models;

/// <summary>
/// Provides helpers for <see cref="AssignedTaskFilter"/>.
/// </summary>
public static class AssignedTaskFilterExtensions
{
    /// <summary>Gets the task statuses that match a filter.</summary>
    /// <param name="filter">The filter.</param>
    /// <returns>The matching statuses; an empty list means all statuses.</returns>
    public static IReadOnlyList<TaskItemStatus> ToStatuses(this AssignedTaskFilter filter) => filter switch
    {
        AssignedTaskFilter.Active => new[] { TaskItemStatus.NotStarted, TaskItemStatus.InProgress },
        AssignedTaskFilter.NotStarted => new[] { TaskItemStatus.NotStarted },
        AssignedTaskFilter.InProgress => new[] { TaskItemStatus.InProgress },
        AssignedTaskFilter.Completed => new[] { TaskItemStatus.Completed },
        _ => Array.Empty<TaskItemStatus>(),
    };

    /// <summary>Gets the text shown to the user for a filter.</summary>
    /// <param name="filter">The filter.</param>
    /// <returns>The display text.</returns>
    public static string ToDisplayName(this AssignedTaskFilter filter) => filter switch
    {
        AssignedTaskFilter.NotStarted => TaskItemStatus.NotStarted.ToDisplayName(),
        AssignedTaskFilter.InProgress => TaskItemStatus.InProgress.ToDisplayName(),
        AssignedTaskFilter.Completed => TaskItemStatus.Completed.ToDisplayName(),
        _ => filter.ToString(),
    };
}
