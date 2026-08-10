using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CBS.BI.Api.Security.Demo;

/// <summary>
/// Demo authentication handler for Cloud Run POC deployments.
/// 
/// This handler authenticates every request as a fixed server-controlled demo identity.
/// It is NOT intended for production use and must only be enabled through explicit configuration.
/// 
/// The demo identity is:
/// - UserId: demo-user
/// - Role: AnalyticsQueryExecutor
/// 
/// Unlike DevelopmentAuthenticationHandler, this handler does NOT read user identity from HTTP headers.
/// Callers cannot override the demo identity through X-Dev-UserId, X-Dev-Roles, or any other headers.
/// 
/// This ensures consistent, predictable authentication for synthetic POC data demonstrations.
/// </summary>
public sealed class DemoAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Demo";
    private const string DemoUserId = "demo-user";
    private const string DemoRole = "AnalyticsQueryExecutor";

    public DemoAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
   ILoggerFactory logger,
        UrlEncoder encoder)
    : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Always authenticate with the fixed demo identity
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, DemoUserId),
            new Claim(ClaimTypes.Role, DemoRole)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        Logger.LogInformation("Demo authentication successful. UserId={DemoUserId}, Role={DemoRole}", DemoUserId, DemoRole);

     return Task.FromResult(AuthenticateResult.Success(ticket));
  }
}
