namespace CBS.BI.Api.Contracts.Analytics;

public sealed record DashboardWidgetRequest(
    string Title,
    string SavedQueryId,
    string VisualizationType,
    int DisplayOrder);

public sealed record SaveAnalyticsDashboardRequest(
    string Name,
    bool IsDynamic,
    IEnumerable<DashboardWidgetRequest> Widgets);

public sealed record DashboardWidgetResponse(
    string Id,
  string Title,
    string SavedQueryId,
    string VisualizationType,
    int DisplayOrder);

public sealed record AnalyticsDashboardResponse(
    string Id,
    string Name,
    bool IsDynamic,
    IReadOnlyCollection<DashboardWidgetResponse> Widgets,
   DateTime CreatedAtUtc,
  DateTime UpdatedAtUtc);
