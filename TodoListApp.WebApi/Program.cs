using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TodoListApp.Services.Database;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi;

/// <summary>
/// The entry point of the web API application.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Configures and runs the web API application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        _ = builder.Services.AddControllers();

        _ = builder.Services.AddDbContext<TodoListDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("TodoListDb"),
                sqlOptions => sqlOptions.EnableRetryOnFailure())); // LocalDB may still be starting up after an idle shutdown.

        _ = builder.Services.AddScoped<ITodoListDatabaseService, TodoListDatabaseService>();
        _ = builder.Services.AddScoped<ITaskDatabaseService, TaskDatabaseService>();
        _ = builder.Services.AddScoped<ITagDatabaseService, TagDatabaseService>();
        _ = builder.Services.AddScoped<ICommentDatabaseService, CommentDatabaseService>();

        AddBearerAuthentication(builder);
        AddSwagger(builder);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            _ = app.UseSwagger();
            _ = app.UseSwaggerUI();
        }

        _ = app.UseHttpsRedirection();

        _ = app.UseAuthentication();
        _ = app.UseAuthorization();

        // Every endpoint requires a valid Bearer token issued by the web application.
        _ = app.MapControllers().RequireAuthorization();

        app.Run();
    }

    // The web application signs the tokens with the same key that is used here to validate them (Epic 7).
    private static void AddBearerAuthentication(WebApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection("Authentication");
        var securityKey = section["SecurityKey"]
            ?? throw new InvalidOperationException("The 'Authentication:SecurityKey' setting is missing.");

        _ = builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = section["Issuer"],
                ValidateAudience = true,
                ValidAudience = section["Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
            });

        _ = builder.Services.AddAuthorization();
    }

    // Swagger UI gets the "Authorize" button to send a Bearer token with the requests.
    private static void AddSwagger(WebApplicationBuilder builder)
    {
        _ = builder.Services.AddEndpointsApiExplorer();
        _ = builder.Services.AddSwaggerGen(options =>
        {
            var scheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Paste an access token issued by the web application.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme },
            };

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, scheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
        });
    }
}
