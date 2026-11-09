using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi.Controllers;

/// <summary>
/// Provides REST endpoints to manage the tasks of the current user's to-do lists.
/// </summary>
[ApiController]
public partial class TaskController : ControllerBase
{
    private readonly ITaskDatabaseService databaseService;
    private readonly ILogger<TaskController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskController"/> class.
    /// </summary>
    /// <param name="databaseService">The service that manages tasks in the database.</param>
    /// <param name="logger">The logger.</param>
    public TaskController(ITaskDatabaseService databaseService, ILogger<TaskController> logger)
    {
        this.databaseService = databaseService;
        this.logger = logger;
    }

    /// <summary>
    /// Gets a page of tasks of a to-do list (US05).
    /// </summary>
    /// <param name="todoListId">The to-do list identifier.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of tasks on a page.</param>
    /// <returns>The tasks on the requested page, or 404 if the to-do list does not exist.</returns>
    [HttpGet("api/todolist/{todoListId:int}/tasks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TodoTaskModel>>> GetByTodoList(int todoListId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var tasks = await this.databaseService.GetByTodoListAsync(todoListId, this.User.GetUserId(), pageNumber, pageSize);
            if (tasks is null)
            {
                this.LogTodoListNotFound(todoListId);
                return this.NotFound();
            }

            return this.Ok(tasks.Select(ToModel));
        }
        catch (ArgumentException ex)
        {
            this.LogInvalidRequest(ex, "get tasks");
            return this.BadRequest(ex.Message);
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "get tasks", todoListId);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets a page of tasks assigned to the current user (US11), optionally filtered by status (US12) and sorted (US13).
    /// </summary>
    /// <param name="status">The statuses of the tasks to return, e.g. <c>?status=NotStarted&amp;status=InProgress</c>; all tasks are returned if none is given.</param>
    /// <param name="sortBy">The task property to sort by: <c>None</c>, <c>Title</c> or <c>DueDate</c>.</param>
    /// <param name="descending"><c>true</c> to sort in descending order.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of tasks on a page.</param>
    /// <returns>The tasks on the requested page.</returns>
    [HttpGet("api/tasks/assigned")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TodoTaskModel>>> GetAssigned(
        [FromQuery] IEnumerable<TaskItemStatus>? status,
        [FromQuery] TaskSortBy sortBy = TaskSortBy.None,
        [FromQuery] bool descending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var statuses = status?.ToList() ?? new List<TaskItemStatus>();
            var tasks = await this.databaseService.GetAssignedAsync(this.User.GetUserId(), statuses, sortBy, descending, pageNumber, pageSize);
            return this.Ok(tasks.Select(ToModel));
        }
        catch (ArgumentException ex)
        {
            this.LogInvalidRequest(ex, "get assigned tasks");
            return this.BadRequest(ex.Message);
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogOperationFailed(ex, "get assigned tasks");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Searches the current user's tasks by title, creation date or due date (US15).
    /// Only the specified criteria are applied; at least one is required.
    /// </summary>
    /// <param name="title">The text the task title must contain.</param>
    /// <param name="createdDate">The date when the task was created, e.g. <c>2026-10-31</c>.</param>
    /// <param name="dueDate">The task due date, e.g. <c>2026-11-05</c>.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of tasks on a page.</param>
    /// <returns>The found tasks on the requested page, or 400 if no criteria are specified.</returns>
    [HttpGet("api/tasks/search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TodoTaskModel>>> Search(
        [FromQuery] string? title,
        [FromQuery] DateTime? createdDate,
        [FromQuery] DateTime? dueDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var criteria = new TaskSearchCriteria { Title = title, CreatedDate = createdDate, DueDate = dueDate };
            var tasks = await this.databaseService.SearchAsync(this.User.GetUserId(), criteria, pageNumber, pageSize);
            return this.Ok(tasks.Select(ToModel));
        }
        catch (ArgumentException ex)
        {
            this.LogInvalidRequest(ex, "search tasks");
            return this.BadRequest(ex.Message);
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogOperationFailed(ex, "search tasks");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets a task by its identifier (US06).
    /// </summary>
    /// <param name="id">The task identifier.</param>
    /// <returns>The task, or 404 if it does not exist.</returns>
    [HttpGet("api/tasks/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TodoTaskModel>> GetById(int id)
    {
        try
        {
            var task = await this.databaseService.GetByIdAsync(id, this.User.GetUserId());
            if (task is null)
            {
                this.LogTaskNotFound(id);
                return this.NotFound();
            }

            return this.Ok(ToModel(task));
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "get a task", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Adds a new task to a to-do list (US07). The task is assigned to the current user.
    /// </summary>
    /// <param name="todoListId">The to-do list identifier.</param>
    /// <param name="model">The task data.</param>
    /// <returns>The created task with its location, or 404 if the to-do list does not exist.</returns>
    [HttpPost("api/todolist/{todoListId:int}/tasks")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TodoTaskModel>> Create(int todoListId, [FromBody] TodoTaskModel model)
    {
        if (model is null)
        {
            return this.BadRequest();
        }

        try
        {
            var created = await this.databaseService.AddAsync(todoListId, ToDomain(model), this.User.GetUserId());
            if (created is null)
            {
                this.LogTodoListNotFound(todoListId);
                return this.NotFound();
            }

            this.LogChanged(created.Id, "created");
            return this.CreatedAtAction(nameof(this.GetById), new { id = created.Id }, ToModel(created));
        }
        catch (ArgumentException ex)
        {
            this.LogInvalidRequest(ex, "create a task");
            return this.BadRequest(ex.Message);
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "create a task", todoListId);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Updates a task (US09).
    /// </summary>
    /// <param name="id">The task identifier.</param>
    /// <param name="model">The new task data.</param>
    /// <returns>204 if the task was updated, or 404 if it does not exist.</returns>
    [HttpPut("api/tasks/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] TodoTaskModel model)
    {
        if (model is null)
        {
            return this.BadRequest();
        }

        try
        {
            var task = ToDomain(model);
            task.Id = id;

            if (!await this.databaseService.UpdateAsync(task, this.User.GetUserId()))
            {
                this.LogTaskNotFound(id);
                return this.NotFound();
            }

            this.LogChanged(id, "updated");
            return this.NoContent();
        }
        catch (ArgumentException ex)
        {
            this.LogInvalidRequest(ex, "update a task");
            return this.BadRequest(ex.Message);
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "update a task", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Changes the status of a task (US14). The task must be assigned to the current user or belong to their to-do list.
    /// </summary>
    /// <param name="id">The task identifier.</param>
    /// <param name="model">The new task status.</param>
    /// <returns>204 if the status was changed, or 404 if the task does not exist.</returns>
    [HttpPut("api/tasks/{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] TaskStatusModel model)
    {
        if (model is null)
        {
            return this.BadRequest();
        }

        try
        {
            if (!await this.databaseService.UpdateStatusAsync(id, model.Status, this.User.GetUserId()))
            {
                this.LogTaskNotFound(id);
                return this.NotFound();
            }

            this.LogChanged(id, "given a new status");
            return this.NoContent();
        }
        catch (ArgumentException ex)
        {
            this.LogInvalidRequest(ex, "change a task status");
            return this.BadRequest(ex.Message);
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "change a task status", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Deletes a task (US08).
    /// </summary>
    /// <param name="id">The task identifier.</param>
    /// <returns>204 if the task was deleted, or 404 if it does not exist.</returns>
    [HttpDelete("api/tasks/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            if (!await this.databaseService.DeleteAsync(id, this.User.GetUserId()))
            {
                this.LogTaskNotFound(id);
                return this.NotFound();
            }

            this.LogChanged(id, "deleted");
            return this.NoContent();
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "delete a task", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>Converts a task to the task model returned by the API.</summary>
    /// <param name="task">The task.</param>
    /// <returns>The task model.</returns>
    internal static TodoTaskModel ToModel(TodoTask task) => new ()
    {
        Id = task.Id,
        TodoListId = task.TodoListId,
        Title = task.Title,
        Description = task.Description,
        CreatedDate = task.CreatedDate,
        DueDate = task.DueDate,
        Status = task.Status,
        AssigneeId = task.AssigneeId,
        IsOverdue = task.IsOverdue,
        Tags = task.Tags,
    };

    // Only the fields a client may change are copied; the rest is set by the server.
    private static TodoTask ToDomain(TodoTaskModel model) => new ()
    {
        Title = model.Title,
        Description = model.Description,
        DueDate = model.DueDate,
        Status = model.Status,
    };

    [LoggerMessage(Level = LogLevel.Warning, Message = "Invalid request to {Operation}.")]
    private partial void LogInvalidRequest(Exception exception, string operation);

    [LoggerMessage(Level = LogLevel.Warning, Message = "To-do list {TodoListId} was not found.")]
    private partial void LogTodoListNotFound(int todoListId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Task {Id} was not found.")]
    private partial void LogTaskNotFound(int id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Task {Id} was {Action}.")]
    private partial void LogChanged(int id, string action);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to {Operation} (id: {Id}).")]
    private partial void LogUnexpectedError(Exception exception, string operation, int id);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to {Operation}.")]
    private partial void LogOperationFailed(Exception exception, string operation);
}
