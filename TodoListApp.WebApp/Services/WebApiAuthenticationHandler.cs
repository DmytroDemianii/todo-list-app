using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace TodoListApp.WebApp.Services;

/// <summary>
/// Adds a Bearer access token of the signed-in user to every request sent to the web API.
/// The token is signed with the security key from the web application configuration.
/// </summary>
public class WebApiAuthenticationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly WebApiOptions options;
    private readonly JsonWebTokenHandler tokenHandler = new ();

    /// <summary>
    /// Initializes a new instance of the <see cref="WebApiAuthenticationHandler"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The accessor of the current HTTP context and its signed-in user.</param>
    /// <param name="options">The web API settings.</param>
    public WebApiAuthenticationHandler(IHttpContextAccessor httpContextAccessor, IOptions<WebApiOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        this.httpContextAccessor = httpContextAccessor;
        this.options = options.Value;
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = this.httpContextAccessor.HttpContext?.User;
        var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.CreateToken(userId, user!.FindFirstValue(ClaimTypes.Email)));
        }

        return base.SendAsync(request, cancellationToken);
    }

    private string CreateToken(string userId, string? email)
    {
        var claims = new List<Claim> { new (JwtRegisteredClaimNames.Sub, userId) };
        if (email is not null)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.options.SecurityKey));
        return this.tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = this.options.Issuer,
            Audience = this.options.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(this.options.TokenLifetimeMinutes),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        });
    }
}
