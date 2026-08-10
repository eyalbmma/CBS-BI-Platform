using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Analytics.UseCases.DeleteAnalyticsDashboard;

/// <summary>
/// Use case for deleting a dashboard.
///
/// Only the dashboard owner can delete their own dashboard.
/// </summary>
public sealed class DeleteAnalyticsDashboardUseCase
{
   private readonly IAnalyticsDashboardStore _store;

    public DeleteAnalyticsDashboardUseCase(IAnalyticsDashboardStore store)
    {
_store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public async Task<bool> ExecuteAsync(
 string dashboardId,
        CurrentUser user,
        CancellationToken cancellationToken)
   {
    if (string.IsNullOrWhiteSpace(dashboardId))
        {
        throw new ArgumentException("Dashboard ID must not be empty.", nameof(dashboardId));
      }

    if (user == null)
        {
   throw new ArgumentNullException(nameof(user));
     }

        return await _store.DeleteAsync(dashboardId, user, cancellationToken);
    }
}
