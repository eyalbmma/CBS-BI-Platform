using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Analytics.UseCases.GetAnalyticsDashboardById;

/// <summary>
/// Use case for retrieving a specific dashboard by ID.
///
/// Only the dashboard owner can retrieve their dashboard.
/// </summary>
public sealed class GetAnalyticsDashboardByIdUseCase
{
    private readonly IAnalyticsDashboardStore _store;

    public GetAnalyticsDashboardByIdUseCase(IAnalyticsDashboardStore store)
    {
  _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public async Task<AnalyticsDashboard?> ExecuteAsync(
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

       return await _store.GetByIdAsync(dashboardId, user, cancellationToken);
   }
}
