using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Analytics.UseCases.SaveAnalyticsDashboard;

/// <summary>
/// Use case for creating a new analytics dashboard.
///
/// Input: user-provided Name, IsDynamic flag, and list of Widgets.
/// Each widget references a SavedAnalyticsQuery that must belong to the current user.
///
/// Flow:
/// 1. Validate Name (not empty)
/// 2. Validate each widget has Title, SavedQueryId, VisualizationType
/// 3. Verify each SavedQueryId exists and is owned by current user
/// 4. Create AnalyticsDashboard with current timestamp
/// 5. Store using IAnalyticsDashboardStore
/// 6. Return the saved dashboard with assigned ID
///
/// Dashboard execution (rendering widgets) happens separately via dedicated endpoints.
/// </summary>
public sealed class SaveAnalyticsDashboardUseCase
{
    private readonly IAnalyticsDashboardStore _dashboardStore;
    private readonly ISavedAnalyticsQueryStore _queryStore;

    public SaveAnalyticsDashboardUseCase(
        IAnalyticsDashboardStore dashboardStore,
ISavedAnalyticsQueryStore queryStore)
    {
        _dashboardStore = dashboardStore ?? throw new ArgumentNullException(nameof(dashboardStore));
        _queryStore = queryStore ?? throw new ArgumentNullException(nameof(queryStore));
    }

    public async Task<AnalyticsDashboard> ExecuteAsync(
  string name,
        bool isDynamic,
  IEnumerable<AnalyticsDashboardWidget> widgets,
        CurrentUser user,
      CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
    {
        throw new ArgumentException("Dashboard name must not be empty.", nameof(name));
        }

        if (user == null)
        {
    throw new ArgumentNullException(nameof(user));
    }

        var widgetList = widgets?.ToList() ?? new List<AnalyticsDashboardWidget>();

        // Validate widgets exist and SavedQueries are owned by this user
        foreach (var widget in widgetList)
        {
     if (string.IsNullOrWhiteSpace(widget.SavedQueryId))
     {
      throw new ArgumentException("Each widget must reference a SavedQueryId.", nameof(widgets));
            }

          var savedQuery = await _queryStore.GetByIdAsync(widget.SavedQueryId, user, cancellationToken);
            if (savedQuery == null)
       {
      throw new InvalidOperationException(
        $"Saved query '{widget.SavedQueryId}' not found or does not belong to the current user.");
     }
        }

        var now = DateTime.UtcNow;
        var dashboard = new AnalyticsDashboard(
            Id: Guid.NewGuid().ToString(),
            Name: name.Trim(),
  CreatedByUserId: user.UserId,
      IsDynamic: isDynamic,
      Widgets: widgetList.AsReadOnly(),
    CreatedAtUtc: now,
      UpdatedAtUtc: now);

      await _dashboardStore.SaveAsync(dashboard, cancellationToken);

    return dashboard;
    }
}
