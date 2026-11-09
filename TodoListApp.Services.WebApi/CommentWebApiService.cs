using System.Net;
using System.Net.Http.Json;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models;

namespace TodoListApp.Services.WebApi;

/// <summary>
/// Manages the comments of tasks by calling the comment web API.
/// </summary>
public class CommentWebApiService : ICommentWebApiService
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentWebApiService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured with the web API base address.</param>
    public CommentWebApiService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TaskComment>?> GetByTaskAsync(int taskId, int pageNumber, int pageSize)
    {
        using var response = await this.httpClient.GetAsync(
            new Uri($"api/tasks/{taskId}/comments?pageNumber={pageNumber}&pageSize={pageSize}", UriKind.Relative));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        _ = response.EnsureSuccessStatusCode();
        var models = await response.Content.ReadFromJsonAsync<List<CommentModel>>();
        return models?.Select(ToDomain).ToList() ?? new List<TaskComment>();
    }

    /// <inheritdoc/>
    public async Task<TaskComment?> GetByIdAsync(int id)
    {
        using var response = await this.httpClient.GetAsync(CommentUri(id));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        _ = response.EnsureSuccessStatusCode();
        var model = await response.Content.ReadFromJsonAsync<CommentModel>();
        return model is null ? null : ToDomain(model);
    }

    /// <inheritdoc/>
    public async Task<bool> AddAsync(int taskId, string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        using var response = await this.httpClient.PostAsJsonAsync(
            new Uri($"api/tasks/{taskId}/comments", UriKind.Relative), new CommentTextModel { Text = text });
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        _ = response.EnsureSuccessStatusCode();
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(int id, string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        using var response = await this.httpClient.PutAsJsonAsync(CommentUri(id), new CommentTextModel { Text = text });
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
        using var response = await this.httpClient.DeleteAsync(CommentUri(id));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        _ = response.EnsureSuccessStatusCode();
        return true;
    }

    private static Uri CommentUri(int id) => new ($"api/comments/{id}", UriKind.Relative);

    private static TaskComment ToDomain(CommentModel model) => new ()
    {
        Id = model.Id,
        TaskId = model.TaskId,
        Text = model.Text,
        AuthorId = model.AuthorId,
        CreatedDate = model.CreatedDate,
        UpdatedDate = model.UpdatedDate,
    };
}
