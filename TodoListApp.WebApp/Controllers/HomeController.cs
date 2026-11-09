using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Controllers;

/// <summary>
/// Handles requests to the application home and error pages; they are available without signing in.
/// </summary>
[AllowAnonymous]
public class HomeController : Controller
{
    /// <summary>
    /// Shows the home page.
    /// </summary>
    /// <returns>The home page view.</returns>
    [HttpGet]
    public IActionResult Index()
    {
        return this.View();
    }

    /// <summary>
    /// Shows the privacy policy page.
    /// </summary>
    /// <returns>The privacy page view.</returns>
    [HttpGet]
    public IActionResult Privacy()
    {
        return this.View();
    }

    /// <summary>
    /// Shows the error page with the current request identifier.
    /// </summary>
    /// <returns>The error page view.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
#pragma warning disable CA5395 // The exception handler re-executes the failed request here with its original HTTP method.
    public IActionResult Error()
#pragma warning restore CA5395
    {
        return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
    }
}
