using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Infrastructure.Analytics;

/// <summary>
/// Thread-safe in-memory implementation of analytics dashboard storage.
///
/// Data persists for the lifetime of the application process.
/// Each user's dashboards are isolated and cannot be accessed by other users.
///
/// This is a POC implementation. The abstraction allows future replacement with
/// persistent storage (Firestore, Cloud SQL, etc.) without modifying Application layer.
/// </summary>
public sealed class InMemoryAnalyticsDashboardStore : IAnalyticsDashboardStore
{
  private readonly object _lockObject = new object();
  private readonly Dictionary<string, AnalyticsDashboard> _dashboardsById = new();

    public Task<IReadOnlyCollection<AnalyticsDashboard>> GetForUserAsync(
  CurrentUser user,
    CancellationToken cancellationToken)
  {
        cancellationToken.ThrowIfCancellationRequested();

  lock (_lockObject)
      {
   var userDashboards = _dashboardsById.Values
  .Where(d => d.CreatedByUserId == user.UserId)
   .OrderByDescending(d => d.CreatedAtUtc)
    .ToList()
           .AsReadOnly();

   return Task.FromResult<IReadOnlyCollection<AnalyticsDashboard>>(userDashboards);
  }
    }

    public Task<AnalyticsDashboard?> GetByIdAsync(
  string id,
        CurrentUser user,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

   if (string.IsNullOrWhiteSpace(id))
 {
     return Task.FromResult<AnalyticsDashboard?>(null);
  }

        lock (_lockObject)
      {
     if (_dashboardsById.TryGetValue(id, out var dashboard))
       {
  if (dashboard.CreatedByUserId == user.UserId)
    {
     return Task.FromResult<AnalyticsDashboard?>(dashboard);
          }
        }

         return Task.FromResult<AnalyticsDashboard?>(null);
    }
    }

    public Task SaveAsync(
        AnalyticsDashboard dashboard,
        CancellationToken cancellationToken)
    {
       cancellationToken.ThrowIfCancellationRequested();

        if (dashboard == null)
        {
      throw new ArgumentNullException(nameof(dashboard));
        }

 lock (_lockObject)
        {
       _dashboardsById[dashboard.Id] = dashboard;
        }

   return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(
     string id,
        CurrentUser user,
        CancellationToken cancellationToken)
    {
   cancellationToken.ThrowIfCancellationRequested();

    if (string.IsNullOrWhiteSpace(id))
        {
          return Task.FromResult(false);
      }

        lock (_lockObject)
     {
        if (_dashboardsById.TryGetValue(id, out var dashboard))
      {
 if (dashboard.CreatedByUserId == user.UserId)
   {
  _dashboardsById.Remove(id);
              return Task.FromResult(true);
         }
    }

        return Task.FromResult(false);
      }
    }
}
