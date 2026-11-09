using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Controllers;

/// <summary>
/// Provides pages to view the tags of the current user's tasks and the tasks tagged with a tag.
/// </summary>
public partial class TagController : Controller
{
    private const int TagPageSize = 30;
    private const int TaskPageSize = 10;

    private readonly ITagWebApiService tagService;
    private readonly ILogger<TagController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagController"/> class.
    /// </summary>
    /// <param name="tagService">The service that manages task tags via the web API.</param>
    /// <param name="logger">The logger.</param>
    public TagController(ITagWebApiService tagService, ILogger<TagController> logger)
    {
        this.tagService = tagService;
        this.logger = logger;
    }

    /// <summary>Shows a page of all tags added to the tasks the user has access to (US18).</summary>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <returns>The tags page.</returns>
    [HttpGet]
    public async Task<IActionResult> Index(int pageNumber = 1)
    {
        if (pageNumber < 1)
        {
            return this.BadRequest();
        }

        try
        {
            var tags = await this.tagService.GetAllAsync(pageNumber, TagPageSize);
            return this.View(new TagListViewModel
            {
                Tags = tags,
                PageNumber = pageNumber,
                HasNextPage = tags.Count == TagPageSize,
            });
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "get tags");
            return this.ApiErrorView();
        }
    }

    /// <summary>Shows a page of the tasks tagged with a tag (US19).</summary>
    /// <param name="name">The tag name.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <returns>The tagged tasks page.</returns>
    [HttpGet]
    public async Task<IActionResult> Tasks(string name, int pageNumber = 1)
    {
        if (string.IsNullOrWhiteSpace(name) || pageNumber < 1)
        {
            return this.BadRequest();
        }

        try
        {
            var tasks = await this.tagService.GetTasksAsync(name, pageNumber, TaskPageSize);
            return this.View(new TaggedTaskListViewModel
            {
                TagName = name,
                Tasks = tasks.Select(TaskController.ToViewModel).ToList(),
                PageNumber = pageNumber,
                HasNextPage = tasks.Count == TaskPageSize,
            });
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "get tagged tasks");
            return this.ApiErrorView();
        }
    }

    private ViewResult ApiErrorView()
    {
        this.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return this.View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Web API call failed: {Operation}.")]
    private partial void LogApiError(Exception exception, string operation);
}
