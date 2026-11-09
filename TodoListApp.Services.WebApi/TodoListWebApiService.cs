using System.Net;
using System.Net.Http.Json;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models;

namespace TodoListApp.Services.WebApi;

/// <summary>
/// Manages to-do lists by calling the to-do list web API.
/// </summary>
public class TodoListWebApiService : ITodoListWebApiService
{
    private const string BasePath = "api/todolist";

    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="TodoListWebApiService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured with the web API base address.</param>
    public TodoListWebApiService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TodoList>> GetAllAsync(int pageNumber, int pageSize)
    {
        var models = await this.httpClient.GetFromJsonAsync<List<TodoListModel>>(
            new Uri($"{BasePath}?pageNumber={pageNumber}&pageSize={pageSize}", UriKind.Relative));

        return models?.Select(ToDomain).ToList() ?? new List<TodoList>();
    }

    /// <inheritdoc/>
    public async Task<TodoList?> GetByIdAsync(int id)
    {
        using var response = await this.httpClient.GetAsync(new Uri($"{BasePath}/{id}", UriKind.Relative));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        _ = response.EnsureSuccessStatusCode();
        var model = await response.Content.ReadFromJsonAsync<TodoListModel>();
        return model is null ? null : ToDomain(model);
    }

    /// <inheritdoc/>
    public Task<TodoList> AddAsync(TodoList todoList)
    {
        ArgumentNullException.ThrowIfNull(todoList);

        return this.AddCoreAsync(todoList);
    }

    /// <inheritdoc/>
    public Task<bool> UpdateAsync(TodoList todoList)
    {
        ArgumentNullException.ThrowIfNull(todoList);

        return this.UpdateCoreAsync(todoList);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id)
    {
        using var response = await this.httpClient.DeleteAsync(new Uri($"{BasePath}/{id}", UriKind.Relative));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        _ = response.EnsureSuccessStatusCode();
        return true;
    }

    private static TodoList ToDomain(TodoListModel model) => new ()
    {
        Id = model.Id,
        Title = model.Title,
        Description = model.Description,
    };

    private static TodoListModel ToModel(TodoList todoList) => new ()
    {
        Id = todoList.Id,
        Title = todoList.Title,
        Description = todoList.Description,
    };

    private async Task<TodoList> AddCoreAsync(TodoList todoList)
    {
        using var response = await this.httpClient.PostAsJsonAsync(new Uri(BasePath, UriKind.Relative), ToModel(todoList));
        _ = response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<TodoListModel>()
            ?? throw new HttpRequestException("The web API returned an empty response.");
        return ToDomain(created);
    }

    private async Task<bool> UpdateCoreAsync(TodoList todoList)
    {
        using var response = await this.httpClient.PutAsJsonAsync(
            new Uri($"{BasePath}/{todoList.Id}", UriKind.Relative), ToModel(todoList));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        _ = response.EnsureSuccessStatusCode();
        return true;
    }
}
