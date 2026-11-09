using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Controllers;

/// <summary>
/// Provides pages to view and manage the current user's to-do lists.
/// </summary>
public partial class TodoListController : Controller
{
    private const int PageSize = 10;

    private readonly ITodoListWebApiService todoListService;
    private readonly ILogger<TodoListController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TodoListController"/> class.
    /// </summary>
    /// <param name="todoListService">The service that manages to-do lists via the web API.</param>
    /// <param name="logger">The logger.</param>
    public TodoListController(ITodoListWebApiService todoListService, ILogger<TodoListController> logger)
    {
        this.todoListService = todoListService;
        this.logger = logger;
    }

    /// <summary>Shows a page of the user's to-do lists (US01).</summary>
    /// <param name="pageNumber">The page number, starting from 1.</param>
    /// <returns>The to-do list page.</returns>
    [HttpGet]
    public async Task<IActionResult> Index(int pageNumber = 1)
    {
        if (pageNumber < 1)
        {
            return this.BadRequest();
        }

        try
        {
            var todoLists = await this.todoListService.GetAllAsync(pageNumber, PageSize);
            this.ViewData["PageNumber"] = pageNumber;
            this.ViewData["HasNextPage"] = todoLists.Count == PageSize;
            return this.View(todoLists.Select(ToViewModel).ToList());
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "get to-do lists");
            return this.ApiErrorView();
        }
    }

    /// <summary>Shows the form to create a to-do list (US02).</summary>
    /// <returns>The create page.</returns>
    [HttpGet]
    public IActionResult Create() => this.View(new TodoListViewModel());

    /// <summary>Creates a to-do list (US02).</summary>
    /// <param name="model">The form data.</param>
    /// <returns>A redirect to the list page, or the form with validation errors.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TodoListViewModel model)
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
            _ = await this.todoListService.AddAsync(ToDomain(model));
            return this.RedirectToAction(nameof(this.Index));
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "create a to-do list");
            return this.ApiErrorView();
        }
    }

    /// <summary>Shows the form to edit a to-do list (US04).</summary>
    /// <param name="id">The to-do list identifier.</param>
    /// <returns>The edit page, or 404.</returns>
    [HttpGet]
    public Task<IActionResult> Edit(int id) => this.ShowTodoList(id);

    /// <summary>Updates a to-do list (US04).</summary>
    /// <param name="id">The to-do list identifier.</param>
    /// <param name="model">The form data.</param>
    /// <returns>A redirect to the list page, the form with errors, or 404.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TodoListViewModel model)
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
            if (!await this.todoListService.UpdateAsync(ToDomain(model)))
            {
                return this.NotFound();
            }

            return this.RedirectToAction(nameof(this.Index));
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "update a to-do list");
            return this.ApiErrorView();
        }
    }

    /// <summary>Shows the delete confirmation page (US03).</summary>
    /// <param name="id">The to-do list identifier.</param>
    /// <returns>The confirmation page, or 404.</returns>
    [HttpGet]
    public Task<IActionResult> Delete(int id) => this.ShowTodoList(id);

    /// <summary>Deletes a to-do list (US03).</summary>
    /// <param name="id">The to-do list identifier.</param>
    /// <returns>A redirect to the list page, or 404.</returns>
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            if (!await this.todoListService.DeleteAsync(id))
            {
                return this.NotFound();
            }

            return this.RedirectToAction(nameof(this.Index));
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "delete a to-do list");
            return this.ApiErrorView();
        }
    }

    private static TodoListViewModel ToViewModel(TodoList todoList) => new ()
    {
        Id = todoList.Id,
        Title = todoList.Title,
        Description = todoList.Description,
    };

    private static TodoList ToDomain(TodoListViewModel model) => new ()
    {
        Id = model.Id,
        Title = model.Title,
        Description = model.Description,
    };

    private async Task<IActionResult> ShowTodoList(int id)
    {
        try
        {
            var todoList = await this.todoListService.GetByIdAsync(id);
            return todoList is null ? this.NotFound() : this.View(ToViewModel(todoList));
        }
        catch (HttpRequestException ex)
        {
            this.LogApiError(ex, "get a to-do list");
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
