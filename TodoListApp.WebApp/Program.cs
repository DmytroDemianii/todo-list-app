using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Interfaces;
using TodoListApp.Services.WebApi;
using TodoListApp.WebApp.Data;
using TodoListApp.WebApp.Services;
using TodoListApp.WebApp.Services.Interfaces;

namespace TodoListApp.WebApp;

/// <summary>
/// The entry point of the web application.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Configures and runs the web application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Every page requires a signed-in user unless it is marked with [AllowAnonymous] (Epic 7).
        _ = builder.Services.AddControllersWithViews(options => options.Filters.Add(new AuthorizeFilter()));

        AddIdentity(builder);
        AddWebApiClients(builder);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            _ = app.UseExceptionHandler("/Home/Error");

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            _ = app.UseHsts();
        }

        _ = app.UseHttpsRedirection();
        _ = app.UseStaticFiles();

        _ = app.UseRouting();

        _ = app.UseAuthentication();
        _ = app.UseAuthorization();

        _ = app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }

    // ASP.NET Core Identity stores users in the UsersDb database and signs them in with a cookie (US26–US29).
    private static void AddIdentity(WebApplicationBuilder builder)
    {
        _ = builder.Services.AddDbContext<UsersDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("UsersDb"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()));

        _ = builder.Services
            .AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireNonAlphanumeric = false;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<UsersDbContext>()
            .AddDefaultTokenProviders();

        _ = builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/Login";
        });

        _ = builder.Services.AddTransient<IEmailSender, LoggingEmailSender>();
        _ = builder.Services.AddScoped<IUserNameService, UserNameService>();
    }

    // Every web API client sends a Bearer token of the signed-in user, signed with the key from the configuration.
    private static void AddWebApiClients(WebApplicationBuilder builder)
    {
        var webApiSection = builder.Configuration.GetSection(WebApiOptions.SectionName);
        _ = builder.Services.Configure<WebApiOptions>(webApiSection);
        var webApiOptions = webApiSection.Get<WebApiOptions>();
        var webApiBaseUrl = webApiOptions?.BaseUrl
            ?? throw new InvalidOperationException("The 'WebApi:BaseUrl' setting is missing.");

        // The key is a secret and is not stored in appsettings.json: set it with "dotnet user-secrets" (see README).
        if (string.IsNullOrEmpty(webApiOptions.SecurityKey))
        {
            throw new InvalidOperationException("The 'WebApi:SecurityKey' setting is missing.");
        }

        _ = builder.Services.AddHttpContextAccessor();
        _ = builder.Services.AddTransient<WebApiAuthenticationHandler>();

        _ = builder.Services.AddHttpClient<ITodoListWebApiService, TodoListWebApiService>(client =>
            client.BaseAddress = webApiBaseUrl).AddHttpMessageHandler<WebApiAuthenticationHandler>();
        _ = builder.Services.AddHttpClient<ITaskWebApiService, TaskWebApiService>(client =>
            client.BaseAddress = webApiBaseUrl).AddHttpMessageHandler<WebApiAuthenticationHandler>();
        _ = builder.Services.AddHttpClient<ITagWebApiService, TagWebApiService>(client =>
            client.BaseAddress = webApiBaseUrl).AddHttpMessageHandler<WebApiAuthenticationHandler>();
        _ = builder.Services.AddHttpClient<ICommentWebApiService, CommentWebApiService>(client =>
            client.BaseAddress = webApiBaseUrl).AddHttpMessageHandler<WebApiAuthenticationHandler>();
    }
}
