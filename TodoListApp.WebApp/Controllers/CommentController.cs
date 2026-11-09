using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Controllers;

/// <summary>
/// Handles adding, editing and deleting task comments; the comments are shown on the task details and edit pages.
/// </summary>
public partial class CommentController : Controller
{
    private readonly ICommentWebApiService commentService;
    private readonly ILogger<CommentController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentController"/> class.
    /// </summary>
    /// <param name="commentService">The service that manages task comments via the web API.</param>
    /// <param name="logger">The logger.</param>
    public CommentController(ICommentWebApiService commentService, ILogger<CommentController> logger)
    {
        this.commentService = commentService;
        this.logger = logger;
    }

    /// <summary>Adds a comment to a task from the task edit page (US23).</summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="model">The comment form data.</param>
    /// <returns>A redirect back to the task edit page, or 404.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int taskId, CommentViewModel model)
    {
        if (model is null)
        {
            return this.BadRequest();
        }

        if (!this.ModelState.IsValid)
        {
            this.TempData[TaskController.CommentErrorKey] = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).FirstOrDefault();
            return this.RedirectToTaskEdit(taskId);
        }

        try
        {
            if (!await this.commentService.AddAsync(taskId, model.Text))
            {
                return this.NotFound();
            }

            return this.RedirectToTaskEdit(taskId);
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "add a comment");
            return this.ApiErrorView();
        }
    }

    /// <summary>Shows the form to edit a comment (US25).</summary>
    /// <param name="id">The comment identifier.</param>
    /// <returns>The comment edit page, or 404.</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var comment = await this.commentService.GetByIdAsync(id);
            return comment is null ? this.NotFound() : this.View(TaskController.ToCommentViewModel(comment));
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "get a comment");
            return this.ApiErrorView();
        }
    }

    /// <summary>Saves the new text of a comment (US25).</summary>
    /// <param name="id">The comment identifier.</param>
    /// <param name="model">The comment form data.</param>
    /// <returns>A redirect back to the task edit page, the form with errors, or 404.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CommentViewModel model)
    {
        if (model is null)
        {
            return this.BadRequest();
        }

        if (id != model.Id)
        {
            return this.BadRequest();
        }

        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        try
        {
            if (!await this.commentService.UpdateAsync(id, model.Text))
            {
                return this.NotFound();
            }

            return this.RedirectToTaskEdit(model.TaskId);
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "update a comment");
            return this.ApiErrorView();
        }
    }

    /// <summary>Deletes a comment (US24).</summary>
    /// <param name="id">The comment identifier.</param>
    /// <param name="taskId">The identifier of the task to return to.</param>
    /// <returns>A redirect back to the task edit page, or 404.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int taskId)
    {
        try
        {
            if (!await this.commentService.DeleteAsync(id))
            {
                return this.NotFound();
            }

            return this.RedirectToTaskEdit(taskId);
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "delete a comment");
            return this.ApiErrorView();
        }
    }

    private RedirectToActionResult RedirectToTaskEdit(int taskId) =>
        this.RedirectToAction(nameof(TaskController.Edit), "Task", new { id = taskId }, "comments");

    private ViewResult ApiErrorView()
    {
        this.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return this.View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Web API call failed: {Operation}.")]
    private partial void LogApiError(Exception exception, string operation);
}
