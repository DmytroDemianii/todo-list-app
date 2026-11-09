using TodoListApp.Services;

namespace TodoListApp.WebApp.Models;

/// <summary>
/// Provides display helpers for <see cref="TaskItemStatus"/>.
/// </summary>
public static class TaskItemStatusExtensions
{
    /// <summary>Gets the text shown to the user for a task status.</summary>
    /// <param name="status">The task status.</param>
    /// <returns>The display text.</returns>
    public static string ToDisplayName(this TaskItemStatus status) => status switch
    {
        TaskItemStatus.NotStarted => "Not started",
        TaskItemStatus.InProgress => "In progress",
        TaskItemStatus.Completed => "Completed",
        _ => status.ToString(),
    };

    /// <summary>Gets the CSS class of the status badge.</summary>
    /// <param name="status">The task status.</param>
    /// <returns>The CSS class name.</returns>
    public static string ToCssClass(this TaskItemStatus status) => status switch
    {
        TaskItemStatus.InProgress => "status-in-progress",
        TaskItemStatus.Completed => "status-completed",
        _ => "status-not-started",
    };
}
