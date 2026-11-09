using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi.Controllers;

/// <summary>
/// Provides REST endpoints to view tags and to add tags to or remove them from tasks.
/// </summary>
[ApiController]
public partial class TagController : ControllerBase
{
    private readonly ITagDatabaseService databaseService;
    private readonly ILogger<TagController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagController"/> class.
    /// </summary>
    /// <param name="databaseService">The service that manages tags in the database.</param>
    /// <param name="logger">The logger.</param>
    public TagController(ITagDatabaseService databaseService, ILogger<TagController> logger)
    {
        this.databaseService = databaseService;
        this.logger = logger;
    }

    /// <summary>
    /// Gets a page of the tags added to the tasks the current user has access to (US18).
    /// </summary>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of tags on a page.</param>
    /// <returns>The tags on the requested page with the number of tagged tasks.</returns>
    [HttpGet("api/tags")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TagModel>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var tags = await this.databaseService.GetAllAsync(this.User.GetUserId(), pageNumber, pageSize);
            return this.Ok(tags.Select(x => new TagModel { Name = x.Name, TaskCount = x.TaskCount }));
        }
        catch (ArgumentException ex)
        {
            this.LogInvalidRequest(ex, "get tags");
            return this.BadRequest(ex.Message);
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "get tags");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets a page of the current user's tasks tagged with a tag (US19).
    /// </summary>
    /// <param name="name">The tag name.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of tasks on a page.</param>
    /// <returns>The tagged tasks on the requested page.</returns>
    [HttpGet("api/tags/{name}/tasks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TodoTaskModel>>> GetTasks(string name, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var tasks = await this.databaseService.GetTasksAsync(this.User.GetUserId(), name, pageNumber, pageSize);
            return this.Ok(tasks.Select(TaskController.ToModel));
        }
        catch (ArgumentException ex)
        {
            this.LogInvalidRequest(ex, "get tagged tasks");
            return this.BadRequest(ex.Message);
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "get tagged tasks");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Adds a tag to a task (US20). Adding a tag the task already has does nothing.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="model">The tag to add.</param>
    /// <returns>204 if the task has the tag, 400 if the tag name is invalid, or 404 if the task does not exist.</returns>
    [HttpPost("api/tasks/{taskId:int}/tags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Add(int taskId, [FromBody] AddTagModel model)
    {
        if (model is null)
        {
            return this.BadRequest();
        }

        try
        {
            if (!await this.databaseService.AddAsync(taskId, model.Name, this.User.GetUserId()))
            {
                this.LogTaskNotFound(taskId);
                return this.NotFound();
            }

            this.LogTagChanged(taskId, "added");
            return this.NoContent();
        }
        catch (ArgumentException ex)
        {
            this.LogInvalidRequest(ex, "add a tag");
            return this.BadRequest(ex.Message);
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "add a tag");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Removes a tag from a task (US21).
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="name">The tag name.</param>
    /// <returns>204 if the tag was removed, or 404 if the task does not exist or does not have the tag.</returns>
    [HttpDelete("api/tasks/{taskId:int}/tags/{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Remove(int taskId, string name)
    {
        try
        {
            if (!await this.databaseService.RemoveAsync(taskId, name, this.User.GetUserId()))
            {
                this.LogTaskNotFound(taskId);
                return this.NotFound();
            }

            this.LogTagChanged(taskId, "removed");
            return this.NoContent();
        }
        catch (ArgumentException ex)
        {
            this.LogInvalidRequest(ex, "remove a tag");
            return this.BadRequest(ex.Message);
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "remove a tag");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Invalid request to {Operation}.")]
    private partial void LogInvalidRequest(Exception exception, string operation);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Task {TaskId} or its tag was not found.")]
    private partial void LogTaskNotFound(int taskId);

    [LoggerMessage(Level = LogLevel.Information, Message = "A tag was {Action} for task {TaskId}.")]
    private partial void LogTagChanged(int taskId, string action);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to {Operation}.")]
    private partial void LogUnexpectedError(Exception exception, string operation);
}
