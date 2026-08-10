namespace CBS.BI.Application.Analytics.Models;

/// <summary>
/// Represents a supported domain and metric combination for visual query generation.
/// 
/// Domain: Business area (Population, Employment, Housing, Education)
/// Metric: Specific measure (e.g., Population, UnemploymentRatePct)
/// </summary>
public sealed record VisualQueryMetricMapping(
    /// <summary>
    /// Business domain name (e.g., "Population", "Employment")
    /// </summary>
    string Domain,

    /// <summary>
    /// Metric name as referenced in the API (e.g., "Population", "UnemploymentRatePct")
    /// </summary>
    string Metric,

    /// <summary>
    /// BigQuery project.dataset.table reference
    /// </summary>
    string TableReference,

    /// <summary>
    /// BigQuery column name for this metric
    /// </summary>
    string ColumnName);
