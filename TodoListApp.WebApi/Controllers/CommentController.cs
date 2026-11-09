using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi.Controllers;

/// <summary>
/// Provides REST endpoints to view and manage the comments added to tasks.
/// </summary>
[ApiController]
public partial class CommentController : ControllerBase
{
    private readonly ICommentDatabaseService databaseService;
    private readonly ILogger<CommentController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentController"/> class.
    /// </summary>
    /// <param name="databaseService">The service that manages comments in the database.</param>
    /// <param name="logger">The logger.</param>
    public CommentController(ICommentDatabaseService databaseService, ILogger<CommentController> logger)
    {
        this.databaseService = databaseService;
        this.logger = logger;
    }

    /// <summary>
    /// Gets a page of the comments of a task, newest first (US22).
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <param name="pageSize">The number of comments on a page.</param>
    /// <returns>The comments on the requested page, or 404 if the task does not exist.</returns>
    [HttpGet("api/tasks/{taskId:int}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CommentModel>>> GetByTask(int taskId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var comments = await this.databaseService.GetByTaskAsync(taskId, this.User.GetUserId(), pageNumber, pageSize);
            if (comments is null)
            {
                this.LogNotFound("Task", taskId);
                return this.NotFound();
            }

            return this.Ok(comments.Select(ToModel));
        }
        catch (ArgumentException ex)
        {
            this.LogInvalidRequest(ex, "get comments");
            return this.BadRequest(ex.Message);
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "get comments", taskId);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets a comment by its identifier.
    /// </summary>
    /// <param name="id">The comment identifier.</param>
    /// <returns>The comment, or 404 if it does not exist.</returns>
    [HttpGet("api/comments/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommentModel>> GetById(int id)
    {
        try
        {
            var comment = await this.databaseService.GetByIdAsync(id, this.User.GetUserId());
            if (comment is null)
            {
                this.LogNotFound("Comment", id);
                return this.NotFound();
            }

            return this.Ok(ToModel(comment));
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "get a comment", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Adds a comment to a task (US23). The current user becomes the comment author.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="model">The comment text.</param>
    /// <returns>The added comment with its location, or 404 if the task does not exist.</returns>
    [HttpPost("api/tasks/{taskId:int}/comments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommentModel>> Create(int taskId, [FromBody] CommentTextModel model)
    {
        if (model is null)
        {
            return this.BadRequest();
        }

        try
        {
            var created = await this.databaseService.AddAsync(taskId, model.Text, this.User.GetUserId());
            if (created is null)
            {
                this.LogNotFound("Task", taskId);
                return this.NotFound();
            }

            this.LogChanged(created.Id, "added");
            return this.CreatedAtAction(nameof(this.GetById), new { id = created.Id }, ToModel(created));
        }
        catch (ArgumentException ex)
        {
            this.LogInvalidRequest(ex, "add a comment");
            return this.BadRequest(ex.Message);
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "add a comment", taskId);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Changes the text of a comment (US25).
    /// </summary>
    /// <param name="id">The comment identifier.</param>
    /// <param name="model">The new comment text.</param>
    /// <returns>204 if the comment was updated, or 404 if it does not exist.</returns>
    [HttpPut("api/comments/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] CommentTextModel model)
    {
        if (model is null)
        {
            return this.BadRequest();
        }

        try
        {
            if (!await this.databaseService.UpdateAsync(id, model.Text, this.User.GetUserId()))
            {
                this.LogNotFound("Comment", id);
                return this.NotFound();
            }

            this.LogChanged(id, "updated");
            return this.NoContent();
        }
        catch (ArgumentException ex)
        {
            this.LogInvalidRequest(ex, "update a comment");
            return this.BadRequest(ex.Message);
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "update a comment", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Deletes a comment (US24).
    /// </summary>
    /// <param name="id">The comment identifier.</param>
    /// <returns>204 if the comment was deleted, or 404 if it does not exist.</returns>
    [HttpDelete("api/comments/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            if (!await this.databaseService.DeleteAsync(id, this.User.GetUserId()))
            {
                this.LogNotFound("Comment", id);
                return this.NotFound();
            }

            this.LogChanged(id, "deleted");
            return this.NoContent();
        }
#pragma warning disable CA1031 // The requirements ask for 500 on any unexpected exception.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            this.LogUnexpectedError(ex, "delete a comment", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static CommentModel ToModel(TaskComment comment) => new ()
    {
        Id = comment.Id,
        TaskId = comment.TaskId,
        Text = comment.Text,
        AuthorId = comment.AuthorId,
        CreatedDate = comment.CreatedDate,
        UpdatedDate = comment.UpdatedDate,
    };

    [LoggerMessage(Level = LogLevel.Warning, Message = "Invalid request to {Operation}.")]
    private partial void LogInvalidRequest(Exception exception, string operation);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Entity} {Id} was not found.")]
    private partial void LogNotFound(string entity, int id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Comment {Id} was {Action}.")]
    private partial void LogChanged(int id, string action);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to {Operation} (id: {Id}).")]
    private partial void LogUnexpectedError(Exception exception, string operation, int id);
}
