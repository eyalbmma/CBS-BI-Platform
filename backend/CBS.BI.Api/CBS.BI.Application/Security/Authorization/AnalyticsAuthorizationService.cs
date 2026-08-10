using CBS.BI.Application.Security.Abstractions;
using CBS.BI.Application.Security.Exceptions;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Security.Authorization;

public sealed class AnalyticsAuthorizationService : IAnalyticsAuthorizationService
{
    private const string RequiredRole = "AnalyticsQueryExecutor";

    public void EnsureCanExecuteQuery(CurrentUser user)
    {
        if (!user.Roles.Any(role => role.Equals(RequiredRole, StringComparison.OrdinalIgnoreCase)))
{
       throw new AnalyticsAuthorizationException();
        }
    }
}
