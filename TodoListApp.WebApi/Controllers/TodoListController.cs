using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi.Controllers;

/// <summary>
/// Provides REST endpoints to manage the to-do lists of the current user.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public partial class TodoListController : ControllerBase
{
    private readonly ITodoListDatabaseService databaseService;
    private readonly ILogger<TodoListController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TodoListController"/> class.
    /// </summary>
    /// <param name="databaseService">The service that manages to-do lists in the database.</param>
    /// <param name="logger">The logger.</param>
    public TodoListController(ITodoListDatabaseService databaseService, ILogger<TodoListController> logger)
    {
        this.databaseService = databaseService;
        this.logger = logger;
    }

    /// <summary>
    /// Gets a page of the current user's to-do lists.
    /// </summary>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of to-do lists on a page.</param>
    /// <returns>The to-do lists on the requested page.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TodoListModel>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var todoLists = await this.databaseService.GetAllAsync(this.User.GetUserId(), pageNumber, pageSize);
            return this.Ok(todoLists.Select(ToModel));
        }
        catch (ArgumentException ex)
        {
            this.LogInvalidPaging(ex, pageNumber, pageSize);
            return this.BadRequest(ex.Message);
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "get to-do lists", null);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets a to-do list by its identifier.
    /// </summary>
    /// <param name="id">The to-do list identifier.</param>
    /// <returns>The to-do list, or 404 if it does not exist.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TodoListModel>> GetById(int id)
    {
        try
        {
            var todoList = await this.databaseService.GetByIdAsync(id, this.User.GetUserId());
            if (todoList is null)
            {
                this.LogNotFound(id);
                return this.NotFound();
            }

            return this.Ok(ToModel(todoList));
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "get a to-do list", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Creates a new to-do list.
    /// </summary>
    /// <param name="model">The to-do list data.</param>
    /// <returns>The created to-do list with its location.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TodoListModel>> Create([FromBody] TodoListModel model)
    {
        if (model is null)
        {
            return this.BadRequest();
        }

        try
        {
            var created = await this.databaseService.AddAsync(ToDomain(model), this.User.GetUserId());
            this.LogChanged(created.Id, "created");
            return this.CreatedAtAction(nameof(this.GetById), new { id = created.Id }, ToModel(created));
        }
        catch (ArgumentException ex)
        {
            this.LogInvalidData(ex, null);
            return this.BadRequest(ex.Message);
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "create a to-do list", null);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Updates an existing to-do list.
    /// </summary>
    /// <param name="id">The to-do list identifier.</param>
    /// <param name="model">The new to-do list data.</param>
    /// <returns>204 if the to-do list was updated, or 404 if it does not exist.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] TodoListModel model)
    {
        if (model is null)
        {
            return this.BadRequest();
        }

        try
        {
            var todoList = ToDomain(model);
            todoList.Id = id;

            if (!await this.databaseService.UpdateAsync(todoList, this.User.GetUserId()))
            {
                this.LogNotFound(id);
                return this.NotFound();
            }

            this.LogChanged(id, "updated");
            return this.NoContent();
        }
        catch (ArgumentException ex)
        {
            this.LogInvalidData(ex, id);
            return this.BadRequest(ex.Message);
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "update a to-do list", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Deletes a to-do list.
    /// </summary>
    /// <param name="id">The to-do list identifier.</param>
    /// <returns>204 if the to-do list was deleted, or 404 if it does not exist.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            if (!await this.databaseService.DeleteAsync(id, this.User.GetUserId()))
            {
                this.LogNotFound(id);
                return this.NotFound();
            }

            this.LogChanged(id, "deleted");
            return this.NoContent();
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "delete a to-do list", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static TodoListModel ToModel(TodoList todoList) => new ()
    {
        Id = todoList.Id,
        Title = todoList.Title,
        Description = todoList.Description,
    };

    private static TodoList ToDomain(TodoListModel model) => new ()
    {
        Id = model.Id,
        Title = model.Title,
        Description = model.Description,
    };

    [LoggerMessage(Level = LogLevel.Warning, Message = "Invalid paging parameters: page {PageNumber}, size {PageSize}.")]
    private partial void LogInvalidPaging(Exception exception, int pageNumber, int pageSize);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Invalid data for to-do list {Id}.")]
    private partial void LogInvalidData(Exception exception, int? id);

    [LoggerMessage(Level = LogLevel.Warning, Message = "To-do list {Id} was not found.")]
    private partial void LogNotFound(int id);

    [LoggerMessage(Level = LogLevel.Information, Message = "To-do list {Id} was {Action}.")]
    private partial void LogChanged(int id, string action);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to {Operation} (id: {Id}).")]
    private partial void LogUnexpectedError(Exception exception, string operation, int? id);
}
