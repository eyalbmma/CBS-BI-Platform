namespace CBS.BI.Application.Analytics.Models;

/// <summary>
/// Represents a saved analytics dashboard configuration.
///
/// A dashboard is a collection of widgets, each executing a saved query and
/// displaying results using a specified visualization type.
///
/// Dashboards can be static (fixed widgets) or dynamic (widgets can accept
/// runtime filters such as Year or City).
/// </summary>
public sealed record AnalyticsDashboard(
    /// <summary>
    /// Unique identifier for this dashboard.
    /// </summary>
    string Id,

    /// <summary>
    /// User-provided name for this dashboard.
    /// </summary>
    string Name,

    /// <summary>
    /// User ID of the person who created this dashboard.
    /// </summary>
    string CreatedByUserId,

    /// <summary>
 /// Whether this dashboard supports runtime filters (Year, City, etc.)
    /// applied to all widgets.
    /// </summary>
    bool IsDynamic,

    /// <summary>
    /// Collection of widgets in this dashboard, ordered by DisplayOrder.
    /// </summary>
    IReadOnlyCollection<AnalyticsDashboardWidget> Widgets,

    /// <summary>
    /// When this dashboard was created (UTC).
    /// </summary>
    DateTime CreatedAtUtc,

    /// <summary>
    /// When this dashboard was last updated (UTC).
    /// </summary>
    DateTime UpdatedAtUtc);
