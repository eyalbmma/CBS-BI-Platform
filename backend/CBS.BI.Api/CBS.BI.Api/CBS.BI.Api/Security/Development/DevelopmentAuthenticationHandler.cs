using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CBS.BI.Api.Security.Development;

public sealed class DevelopmentAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
  public const string SchemeName = "Development";

    private readonly IWebHostEnvironment _environment;

  public DevelopmentAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IWebHostEnvironment environment)
    : base(options, logger, encoder)
    {
        _environment = environment;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Only authenticate in Development environment
        if (!_environment.IsDevelopment())
        {
        return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Read the development user ID from the X-Dev-UserId header
if (!Request.Headers.TryGetValue("X-Dev-UserId", out var userIdValue))
    {
 return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = userIdValue.ToString();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Create claims for the user
        var claims = new List<Claim>
        {
     new Claim(ClaimTypes.NameIdentifier, userId)
};

        // Read optional roles from X-Dev-Roles header
        if (Request.Headers.TryGetValue("X-Dev-Roles", out var rolesValue))
        {
    var rolesString = rolesValue.ToString();
            if (!string.IsNullOrWhiteSpace(rolesString))
            {
            // Parse roles: split on comma, trim, remove empty, deduplicate case-insensitively
          var roles = rolesString
          .Split(',')
    .Select(r => r.Trim())
   .Where(r => !string.IsNullOrWhiteSpace(r))
     .Distinct(StringComparer.OrdinalIgnoreCase)
     .ToList();

    foreach (var role in roles)
       {
          claims.Add(new Claim(ClaimTypes.Role, role));
      }
            }
     }

        // Create ClaimsIdentity with Development scheme so IsAuthenticated returns true
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
  var ticket = new AuthenticationTicket(principal, SchemeName);

     return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
