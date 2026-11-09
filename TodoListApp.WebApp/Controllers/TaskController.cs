using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApp.Models;
using TodoListApp.WebApp.Services.Interfaces;

namespace TodoListApp.WebApp.Controllers;

/// <summary>
/// Provides pages to view and manage the tasks of the current user's to-do lists.
/// </summary>
public partial class TaskController : Controller
{
    /// <summary>The <see cref="Controller.TempData"/> key of the error shown in the tags section of the edit page.</summary>
    internal const string TagErrorKey = "TagError";

    /// <summary>The <see cref="Controller.TempData"/> key of the error shown in the comments section of the edit page.</summary>
    internal const string CommentErrorKey = "CommentError";

    private const int PageSize = 10;
    private const int CommentPageSize = 10;

    private readonly ITaskWebApiService taskService;
    private readonly ITodoListWebApiService todoListService;
    private readonly ITagWebApiService tagService;
    private readonly ICommentWebApiService commentService;
    private readonly IUserNameService userNameService;
    private readonly ILogger<TaskController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskController"/> class.
    /// </summary>
    /// <param name="taskService">The service that manages tasks via the web API.</param>
    /// <param name="todoListService">The service that manages to-do lists via the web API.</param>
    /// <param name="tagService">The service that manages task tags via the web API.</param>
    /// <param name="commentService">The service that manages task comments via the web API.</param>
    /// <param name="userNameService">The service that finds the emails of the assignee and the comment authors.</param>
    /// <param name="logger">The logger.</param>
    public TaskController(
        ITaskWebApiService taskService,
        ITodoListWebApiService todoListService,
        ITagWebApiService tagService,
        ICommentWebApiService commentService,
        IUserNameService userNameService,
        ILogger<TaskController> logger)
    {
        this.taskService = taskService;
        this.todoListService = todoListService;
        this.tagService = tagService;
        this.commentService = commentService;
        this.userNameService = userNameService;
        this.logger = logger;
    }

    /// <summary>Shows a page of tasks of a to-do list (US05).</summary>
    /// <param name="todoListId">The to-do list identifier.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <returns>The task list page, or 404.</returns>
    [HttpGet]
    public async Task<IActionResult> Index(int todoListId, int pageNumber = 1)
    {
        if (pageNumber < 1)
        {
            return this.BadRequest();
        }

        try
        {
            var todoList = await this.todoListService.GetByIdAsync(todoListId);
            var tasks = await this.taskService.GetByTodoListAsync(todoListId, pageNumber, PageSize);
            if (todoList is null || tasks is null)
            {
                return this.NotFound();
            }

            return this.View(new TaskListViewModel
            {
                TodoListId = todoListId,
                TodoListTitle = todoList.Title,
                Tasks = tasks.Select(ToViewModel).ToList(),
                PageNumber = pageNumber,
                HasNextPage = tasks.Count == PageSize,
            });
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "get tasks");
            return this.ApiErrorView();
        }
    }

    /// <summary>Shows a page of tasks assigned to the current user (US11) filtered by status (US12) and sorted (US13).</summary>
    /// <param name="filter">The status filter; only active tasks are shown by default.</param>
    /// <param name="sort">The order of the tasks.</param>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <returns>The assigned tasks page.</returns>
    [HttpGet]
    public async Task<IActionResult> Assigned(AssignedTaskFilter filter = AssignedTaskFilter.Active, AssignedTaskSort sort = AssignedTaskSort.Default, int pageNumber = 1)
    {
        if (pageNumber < 1 || !Enum.IsDefined(filter) || !Enum.IsDefined(sort))
        {
            return this.BadRequest();
        }

        try
        {
            var tasks = await this.taskService.GetAssignedAsync(filter.ToStatuses(), sort.ToSortBy(), sort.IsDescending(), pageNumber, PageSize);
            return this.View(new AssignedTaskListViewModel
            {
                Tasks = tasks.Select(ToViewModel).ToList(),
                Filter = filter,
                Sort = sort,
                PageNumber = pageNumber,
                HasNextPage = tasks.Count == PageSize,
            });
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "get assigned tasks");
            return this.ApiErrorView();
        }
    }

    /// <summary>Searches tasks by title, creation date or due date (US15); overdue tasks are highlighted in the view (US16).</summary>
    /// <param name="model">The search criteria and the page number from the query string.</param>
    /// <returns>The search page with the found tasks; only the form is shown if no criteria are specified.</returns>
    [HttpGet]
    public async Task<IActionResult> Search(TaskSearchViewModel model)
    {
        if (model is null)
        {
            return this.BadRequest();
        }

        if (model.PageNumber < 1)
        {
            return this.BadRequest();
        }

        if (!this.ModelState.IsValid || !model.HasCriteria)
        {
            return this.View(model);
        }

        try
        {
            var tasks = await this.taskService.SearchAsync(model.Title, model.CreatedDate, model.DueDate, model.PageNumber, PageSize);
            model.Tasks = tasks.Select(ToViewModel).ToList();
            model.HasNextPage = tasks.Count == PageSize;
            return this.View(model);
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "search tasks");
            return this.ApiErrorView();
        }
    }

    /// <summary>Changes the status of a task from the assigned tasks page (US14).</summary>
    /// <param name="id">The task identifier.</param>
    /// <param name="status">The new task status.</param>
    /// <param name="filter">The status filter of the page to return to.</param>
    /// <param name="sort">The sort order of the page to return to.</param>
    /// <param name="pageNumber">The page number to return to.</param>
    /// <returns>A redirect back to the assigned tasks page, or 404.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, TaskItemStatus status, AssignedTaskFilter filter, AssignedTaskSort sort, int pageNumber = 1)
    {
        if (!this.ModelState.IsValid || !Enum.IsDefined(status))
        {
            return this.BadRequest();
        }

        try
        {
            if (!await this.taskService.UpdateStatusAsync(id, status))
            {
                return this.NotFound();
            }

            return this.RedirectToAction(nameof(this.Assigned), new { filter, sort, pageNumber });
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "change a task status");
            return this.ApiErrorView();
        }
    }

    /// <summary>Shows the task details page (US06) with its tags (US17) and comments (US22).</summary>
    /// <param name="id">The task identifier.</param>
    /// <param name="commentPage">The comment page number, starting from 1.</param>
    /// <returns>The details page, or 404.</returns>
    [HttpGet]
    public Task<IActionResult> Details(int id, int commentPage = 1) => this.ShowTask(id, commentPage);

    /// <summary>Shows the form to add a task to a to-do list (US07).</summary>
    /// <param name="todoListId">The to-do list identifier.</param>
    /// <returns>The create page.</returns>
    [HttpGet]
    public IActionResult Create(int todoListId) => this.View(new TaskViewModel { TodoListId = todoListId });

    /// <summary>Adds a task to a to-do list (US07).</summary>
    /// <param name="model">The form data.</param>
    /// <returns>A redirect to the task list, the form with validation errors, or 404.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskViewModel model)
    {
        if (model is null)
        {
            return this.BadRequest();
        }

        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        try
        {
            if (await this.taskService.AddAsync(model.TodoListId, ToDomain(model)) is null)
            {
                return this.NotFound();
            }

            return this.RedirectToAction(nameof(this.Index), new { todoListId = model.TodoListId });
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "create a task");
            return this.ApiErrorView();
        }
    }

    /// <summary>Shows the form to edit a task (US09) with its tags (US20, US21) and comments (US23–US25).</summary>
    /// <param name="id">The task identifier.</param>
    /// <param name="commentPage">The comment page number, starting from 1.</param>
    /// <returns>The edit page, or 404.</returns>
    [HttpGet]
    public Task<IActionResult> Edit(int id, int commentPage = 1) => this.ShowTask(id, commentPage);

    /// <summary>Updates a task (US09).</summary>
    /// <param name="id">The task identifier.</param>
    /// <param name="model">The form data.</param>
    /// <returns>A redirect to the task details, the form with errors, or 404.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TaskViewModel model)
    {
        if (model is null)
        {
            return this.BadRequest();
        }

        if (id != model.Id)
        {
            return this.BadRequest();
        }

        try
        {
            if (!this.ModelState.IsValid)
            {
                // Tags and comments are not part of the form, so they are loaded again for their sections.
                model.Tags = (await this.taskService.GetByIdAsync(id))?.Tags ?? Array.Empty<string>();
                await this.LoadCommentsAsync(model, commentPage: 1);
                await this.LoadUserNamesAsync(model);
                return this.View(model);
            }

            if (!await this.taskService.UpdateAsync(ToDomain(model)))
            {
                return this.NotFound();
            }

            return this.RedirectToAction(nameof(this.Details), new { id });
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "update a task");
            return this.ApiErrorView();
        }
    }

    /// <summary>Shows the delete confirmation page (US08).</summary>
    /// <param name="id">The task identifier.</param>
    /// <returns>The confirmation page, or 404.</returns>
    [HttpGet]
    public Task<IActionResult> Delete(int id) => this.ShowTask(id);

    /// <summary>Deletes a task (US08).</summary>
    /// <param name="id">The task identifier.</param>
    /// <param name="todoListId">The identifier of the to-do list to return to.</param>
    /// <returns>A redirect to the task list, or 404.</returns>
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, int todoListId)
    {
        try
        {
            if (!await this.taskService.DeleteAsync(id))
            {
                return this.NotFound();
            }

            return this.RedirectToAction(nameof(this.Index), new { todoListId });
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "delete a task");
            return this.ApiErrorView();
        }
    }

    /// <summary>Adds a tag to a task on the task edit page (US20).</summary>
    /// <param name="id">The task identifier.</param>
    /// <param name="tag">The tag form data.</param>
    /// <returns>A redirect back to the task edit page, or 404.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTag(int id, AddTagViewModel tag)
    {
        if (tag is null)
        {
            return this.BadRequest();
        }

        if (!this.ModelState.IsValid)
        {
            this.TempData[TagErrorKey] = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).FirstOrDefault();
            return this.RedirectToAction(nameof(this.Edit), new { id });
        }

        try
        {
            if (!await this.tagService.AddAsync(id, tag.Name.Trim()))
            {
                return this.NotFound();
            }

            return this.RedirectToAction(nameof(this.Edit), new { id });
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "add a tag");
            return this.ApiErrorView();
        }
    }

    /// <summary>Removes a tag from a task on the task edit page (US21).</summary>
    /// <param name="id">The task identifier.</param>
    /// <param name="name">The tag name.</param>
    /// <returns>A redirect back to the task edit page, or 404.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveTag(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return this.BadRequest();
        }

        try
        {
            if (!await this.tagService.RemoveAsync(id, name))
            {
                return this.NotFound();
            }

            return this.RedirectToAction(nameof(this.Edit), new { id });
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "remove a tag");
            return this.ApiErrorView();
        }
    }

    /// <summary>Converts a task to the task model used by the views.</summary>
    /// <param name="task">The task.</param>
    /// <returns>The task view model.</returns>
    internal static TaskViewModel ToViewModel(TodoTask task) => new ()
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

    /// <summary>Converts a comment to the comment model used by the views.</summary>
    /// <param name="comment">The comment.</param>
    /// <returns>The comment view model.</returns>
    internal static CommentViewModel ToCommentViewModel(TaskComment comment) => new ()
    {
        Id = comment.Id,
        TaskId = comment.TaskId,
        Text = comment.Text,
        AuthorId = comment.AuthorId,
        CreatedDate = comment.CreatedDate,
        UpdatedDate = comment.UpdatedDate,
    };

    private static TodoTask ToDomain(TaskViewModel model) => new ()
    {
        Id = model.Id,
        TodoListId = model.TodoListId,
        Title = model.Title,
        Description = model.Description,
        DueDate = model.DueDate,
        Status = model.Status,
    };

    // The comments are loaded only for the pages that show them (details and edit).
    private async Task<IActionResult> ShowTask(int id, int? commentPage = null)
    {
        if (commentPage < 1)
        {
            return this.BadRequest();
        }

        try
        {
            var task = await this.taskService.GetByIdAsync(id);
            if (task is null)
            {
                return this.NotFound();
            }

            var model = ToViewModel(task);
            if (commentPage.HasValue)
            {
                await this.LoadCommentsAsync(model, commentPage.Value);
            }

            await this.LoadUserNamesAsync(model);
            return this.View(model);
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "get a task");
            return this.ApiErrorView();
        }
    }

    private async Task LoadCommentsAsync(TaskViewModel model, int commentPage)
    {
        var comments = await this.commentService.GetByTaskAsync(model.Id, commentPage, CommentPageSize) ?? Array.Empty<TaskComment>();
        model.Comments = comments.Select(ToCommentViewModel).ToList();
        model.CommentPageNumber = commentPage;
        model.HasNextCommentPage = comments.Count == CommentPageSize;
    }

    // The web API stores user identifiers; the pages show the users' emails instead.
    private async Task LoadUserNamesAsync(TaskViewModel model)
    {
        var emails = await this.userNameService.GetEmailsAsync(model.Comments.Select(x => x.AuthorId).Append(model.AssigneeId));
        model.AssigneeName = emails.GetValueOrDefault(model.AssigneeId);
        foreach (var comment in model.Comments)
        {
            comment.AuthorName = emails.GetValueOrDefault(comment.AuthorId);
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
