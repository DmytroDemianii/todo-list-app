using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models;

/// <summary>
/// The model of the task search page: the search criteria and the found tasks.
/// </summary>
public class TaskSearchViewModel
{
    /// <summary>Gets or sets the text the task title must contain.</summary>
    [StringLength(100)]
    public string? Title { get; set; }

    /// <summary>Gets or sets the date when the task was created.</summary>
    [Display(Name = "Created on")]
    [DataType(DataType.Date)]
    public DateTime? CreatedDate { get; set; }

    /// <summary>Gets or sets the task due date.</summary>
    [Display(Name = "Due on")]
    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    /// <summary>Gets a value indicating whether at least one search criterion is specified.</summary>
    public bool HasCriteria => !string.IsNullOrWhiteSpace(this.Title) || this.CreatedDate.HasValue || this.DueDate.HasValue;

    /// <summary>Gets or sets the found tasks on the current page.</summary>
    public IReadOnlyList<TaskViewModel> Tasks { get; set; } = Array.Empty<TaskViewModel>();

    /// <summary>Gets or sets the current page number.</summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>Gets or sets a value indicating whether there is a next page.</summary>
    public bool HasNextPage { get; set; }
}
