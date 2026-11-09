using TodoListApp.Services;

namespace TodoListApp.WebApp.Models;

/// <summary>
/// Provides helpers for <see cref="AssignedTaskSort"/>.
/// </summary>
public static class AssignedTaskSortExtensions
{
    /// <summary>Gets the task property the web API sorts by.</summary>
    /// <param name="sort">The sort order.</param>
    /// <returns>The task property to sort by.</returns>
    public static TaskSortBy ToSortBy(this AssignedTaskSort sort) => sort switch
    {
        AssignedTaskSort.TitleAscending or AssignedTaskSort.TitleDescending => TaskSortBy.Title,
        AssignedTaskSort.DueDateAscending or AssignedTaskSort.DueDateDescending => TaskSortBy.DueDate,
        _ => TaskSortBy.None,
    };

    /// <summary>Gets a value indicating whether tasks are sorted in descending order.</summary>
    /// <param name="sort">The sort order.</param>
    /// <returns><c>true</c> for a descending order; otherwise, <c>false</c>.</returns>
    public static bool IsDescending(this AssignedTaskSort sort) =>
        sort is AssignedTaskSort.TitleDescending or AssignedTaskSort.DueDateDescending;

    /// <summary>Gets the text shown to the user for a sort order.</summary>
    /// <param name="sort">The sort order.</param>
    /// <returns>The display text.</returns>
    public static string ToDisplayName(this AssignedTaskSort sort) => sort switch
    {
        AssignedTaskSort.TitleAscending => "Name (A–Z)",
        AssignedTaskSort.TitleDescending => "Name (Z–A)",
        AssignedTaskSort.DueDateAscending => "Due date (earliest first)",
        AssignedTaskSort.DueDateDescending => "Due date (latest first)",
        _ => "Date created",
    };
}
