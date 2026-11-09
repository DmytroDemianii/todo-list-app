using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TodoListApp.Services;

/// <summary>
/// Describes the current state of a task.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskItemStatus
{
    /// <summary>The task has not been started yet.</summary>
    [Display(Name = "Not started")]
    NotStarted = 0,

    /// <summary>The task is in progress.</summary>
    [Display(Name = "In progress")]
    InProgress = 1,

    /// <summary>The task is completed.</summary>
    [Display(Name = "Completed")]
    Completed = 2,
}
