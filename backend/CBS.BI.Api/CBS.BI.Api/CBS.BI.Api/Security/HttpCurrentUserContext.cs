using System.Security.Claims;
using CBS.BI.Application.Security.Abstractions;
using CBS.BI.Application.Security.Exceptions;
using CBS.BI.Application.Security.Models;
using Microsoft.AspNetCore.Http;

namespace CBS.BI.Api.Security;

public sealed class HttpCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUser GetCurrentUser()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new UserNotAuthenticatedException();
        }

        var user = httpContext.User;
        if (user == null)
        {
            throw new UserNotAuthenticatedException();
        }

        if (!(user.Identity?.IsAuthenticated ?? false))
        {
            throw new UserNotAuthenticatedException();
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
       ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new UserNotAuthenticatedException();
        }

        var roles = user.FindAll(ClaimTypes.Role)
        .Select(c => c.Value)
         .Distinct()
  .ToList()
    .AsReadOnly();

        return new CurrentUser
        {
     UserId = userId,
   Roles = roles
        };
    }
}
