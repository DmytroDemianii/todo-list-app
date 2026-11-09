using System.Net;
using System.Net.Http.Json;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models;

namespace TodoListApp.Services.WebApi;

/// <summary>
/// Manages the tags of tasks by calling the tag web API.
/// </summary>
public class TagWebApiService : ITagWebApiService
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagWebApiService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured with the web API base address.</param>
    public TagWebApiService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TagSummary>> GetAllAsync(int pageNumber, int pageSize)
    {
        var tags = await this.httpClient.GetFromJsonAsync<List<TagModel>>(
            new Uri($"api/tags?pageNumber={pageNumber}&pageSize={pageSize}", UriKind.Relative));

        return tags?.Select(x => new TagSummary { Name = x.Name, TaskCount = x.TaskCount }).ToList() ?? new List<TagSummary>();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TodoTask>> GetTasksAsync(string tagName, int pageNumber, int pageSize)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tagName);

        var models = await this.httpClient.GetFromJsonAsync<List<TodoTaskModel>>(
            new Uri($"api/tags/{Uri.EscapeDataString(tagName)}/tasks?pageNumber={pageNumber}&pageSize={pageSize}", UriKind.Relative));

        return models?.Select(TaskWebApiService.ToDomain).ToList() ?? new List<TodoTask>();
    }

    /// <inheritdoc/>
    public async Task<bool> AddAsync(int taskId, string tagName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tagName);

        using var response = await this.httpClient.PostAsJsonAsync(
            new Uri($"api/tasks/{taskId}/tags", UriKind.Relative), new AddTagModel { Name = tagName });
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        _ = response.EnsureSuccessStatusCode();
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveAsync(int taskId, string tagName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tagName);

        using var response = await this.httpClient.DeleteAsync(
            new Uri($"api/tasks/{taskId}/tags/{Uri.EscapeDataString(tagName)}", UriKind.Relative));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        _ = response.EnsureSuccessStatusCode();
        return true;
    }
}
