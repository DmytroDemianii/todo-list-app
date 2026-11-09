using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models;

namespace TodoListApp.Services.WebApi;

/// <summary>
/// Manages to-do list tasks by calling the task web API.
/// </summary>
public class TaskWebApiService : ITaskWebApiService
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskWebApiService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured with the web API base address.</param>
    public TaskWebApiService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TodoTask>?> GetByTodoListAsync(int todoListId, int pageNumber, int pageSize)
    {
        using var response = await this.httpClient.GetAsync(
            new Uri($"api/todolist/{todoListId}/tasks?pageNumber={pageNumber}&pageSize={pageSize}", UriKind.Relative));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        _ = response.EnsureSuccessStatusCode();
        var models = await response.Content.ReadFromJsonAsync<List<TodoTaskModel>>();
        return models?.Select(ToDomain).ToList() ?? new List<TodoTask>();
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<TodoTask>> GetAssignedAsync(IEnumerable<TaskItemStatus> statuses, TaskSortBy sortBy, bool descending, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(statuses);

        return this.GetAssignedCoreAsync(statuses, sortBy, descending, pageNumber, pageSize);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TodoTask>> SearchAsync(string? title, DateTime? createdDate, DateTime? dueDate, int pageNumber, int pageSize)
    {
        var query = new StringBuilder($"api/tasks/search?pageNumber={pageNumber}&pageSize={pageSize}");
        if (!string.IsNullOrWhiteSpace(title))
        {
            _ = query.Append("&title=").Append(Uri.EscapeDataString(title.Trim()));
        }

        if (createdDate.HasValue)
        {
            _ = query.Append("&createdDate=").Append(createdDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        if (dueDate.HasValue)
        {
            _ = query.Append("&dueDate=").Append(dueDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        var models = await this.httpClient.GetFromJsonAsync<List<TodoTaskModel>>(
            new Uri(query.ToString(), UriKind.Relative));

        return models?.Select(ToDomain).ToList() ?? new List<TodoTask>();
    }

    /// <inheritdoc/>
    public async Task<TodoTask?> GetByIdAsync(int id)
    {
        using var response = await this.httpClient.GetAsync(TaskUri(id));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        _ = response.EnsureSuccessStatusCode();
        var model = await response.Content.ReadFromJsonAsync<TodoTaskModel>();
        return model is null ? null : ToDomain(model);
    }

    /// <inheritdoc/>
    public Task<TodoTask?> AddAsync(int todoListId, TodoTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        return this.AddCoreAsync(todoListId, task);
    }

    /// <inheritdoc/>
    public Task<bool> UpdateAsync(TodoTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        return this.UpdateCoreAsync(task);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateStatusAsync(int id, TaskItemStatus status)
    {
        using var response = await this.httpClient.PutAsJsonAsync(
            new Uri($"api/tasks/{id}/status", UriKind.Relative), new TaskStatusModel { Status = status });
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        _ = response.EnsureSuccessStatusCode();
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id)
    {
        using var response = await this.httpClient.DeleteAsync(TaskUri(id));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        _ = response.EnsureSuccessStatusCode();
        return true;
    }

    /// <summary>Converts a task received from the web API to a task.</summary>
    /// <param name="model">The task model received from the web API.</param>
    /// <returns>The task.</returns>
    internal static TodoTask ToDomain(TodoTaskModel model) => new ()
    {
        Id = model.Id,
        TodoListId = model.TodoListId,
        Title = model.Title,
        Description = model.Description,
        CreatedDate = model.CreatedDate,
        DueDate = model.DueDate,
        Status = model.Status,
        AssigneeId = model.AssigneeId,
        Tags = model.Tags,
    };

    private static Uri TaskUri(int id) => new ($"api/tasks/{id}", UriKind.Relative);

    private static TodoTaskModel ToModel(TodoTask task) => new ()
    {
        Id = task.Id,
        TodoListId = task.TodoListId,
        Title = task.Title,
        Description = task.Description,
        DueDate = task.DueDate,
        Status = task.Status,
    };

    private async Task<IReadOnlyList<TodoTask>> GetAssignedCoreAsync(IEnumerable<TaskItemStatus> statuses, TaskSortBy sortBy, bool descending, int pageNumber, int pageSize)
    {
        var statusQuery = string.Concat(statuses.Select(x => $"&status={x}"));
        var sortQuery = $"&sortBy={sortBy}&descending={(descending ? "true" : "false")}";
        var models = await this.httpClient.GetFromJsonAsync<List<TodoTaskModel>>(
            new Uri($"api/tasks/assigned?pageNumber={pageNumber}&pageSize={pageSize}{statusQuery}{sortQuery}", UriKind.Relative));

        return models?.Select(ToDomain).ToList() ?? new List<TodoTask>();
    }

    private async Task<TodoTask?> AddCoreAsync(int todoListId, TodoTask task)
    {
        using var response = await this.httpClient.PostAsJsonAsync(
            new Uri($"api/todolist/{todoListId}/tasks", UriKind.Relative), ToModel(task));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        _ = response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<TodoTaskModel>()
            ?? throw new HttpRequestException("The web API returned an empty response.");
        return ToDomain(created);
    }

    private async Task<bool> UpdateCoreAsync(TodoTask task)
    {
        using var response = await this.httpClient.PutAsJsonAsync(TaskUri(task.Id), ToModel(task));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        _ = response.EnsureSuccessStatusCode();
        return true;
    }
}
