using System.Security.Claims;

namespace TodoListApp.WebApi.Services;

/// <summary>
/// Provides helpers to read the authenticated user from the Bearer token claims.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>Gets the identifier of the authenticated user: the "sub" claim of the access token.</summary>
    /// <param name="user">The authenticated user.</param>
    /// <returns>The user identifier.</returns>
    /// <exception cref="InvalidOperationException">The user has no identifier claim.</exception>
    public static string GetUserId(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        // The JWT handler maps the "sub" claim to ClaimTypes.NameIdentifier by default.
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub")
            ?? throw new InvalidOperationException("The access token has no user identifier.");
    }
}
