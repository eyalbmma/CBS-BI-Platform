namespace CBS.BI.Application.Analytics.Models;

/// <summary>
/// Represents a visualization widget in an analytics dashboard.
///
/// Each widget displays the results of a saved analytics query using a
/// specified visualization type.
/// </summary>
public sealed record AnalyticsDashboardWidget(
    /// <summary>
/// Unique identifier for this widget within the dashboard.
    /// </summary>
    string Id,

    /// <summary>
    /// Display title for the widget.
    /// </summary>
  string Title,

    /// <summary>
    /// ID of the SavedAnalyticsQuery this widget executes.
    /// </summary>
    string SavedQueryId,

    /// <summary>
    /// How this widget's data should be visualized.
    /// </summary>
    string VisualizationType,

    /// <summary>
    /// Display order within the dashboard (lower numbers appear first).
    /// </summary>
    int DisplayOrder);
