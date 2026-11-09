using System.ComponentModel.DataAnnotations;
using TodoListApp.Services;

namespace TodoListApp.WebApi.Models;

/// <summary>
/// The model used to change the status of a task.
/// </summary>
public class TaskStatusModel
{
    /// <summary>Gets or sets the new task status.</summary>
    [EnumDataType(typeof(TaskItemStatus))]
    public TaskItemStatus Status { get; set; }
}
