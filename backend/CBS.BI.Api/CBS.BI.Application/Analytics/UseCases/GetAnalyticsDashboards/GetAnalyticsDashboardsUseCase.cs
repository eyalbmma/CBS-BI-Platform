using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Analytics.UseCases.GetAnalyticsDashboards;

/// <summary>
/// Use case for retrieving all dashboards for the current user.
///
/// Returns dashboards ordered newest first.
/// </summary>
public sealed class GetAnalyticsDashboardsUseCase
{
    private readonly IAnalyticsDashboardStore _store;

    public GetAnalyticsDashboardsUseCase(IAnalyticsDashboardStore store)
    {
   _store = store ?? throw new ArgumentNullException(nameof(store));
    }

   public async Task<IReadOnlyCollection<AnalyticsDashboard>> ExecuteAsync(
 CurrentUser user,
   CancellationToken cancellationToken)
    {
  if (user == null)
 {
throw new ArgumentNullException(nameof(user));
        }

  return await _store.GetForUserAsync(user, cancellationToken);
    }
}
