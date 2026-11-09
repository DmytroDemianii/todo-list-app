namespace TodoListApp.WebApp.Models;

/// <summary>
/// Represents the data shown on the error page.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Gets or sets the identifier of the request that failed.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets a value indicating whether the request identifier should be shown.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(this.RequestId);
}
