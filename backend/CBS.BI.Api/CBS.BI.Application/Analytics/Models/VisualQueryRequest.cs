namespace CBS.BI.Application.Analytics.Models;

/// <summary>
/// Represents a visual query definition for execution within the application layer.
/// 
/// This is the internal application model, separate from the API HTTP contract.
/// It contains all information needed to generate and execute a visual query.
/// </summary>
public sealed record VisualQueryDefinition(
    /// <summary>
    /// Analytics domain (e.g., "Population", "Employment", "Housing", "Education")
    /// </summary>
    string Domain,

    /// <summary>
    /// Metric within the domain (e.g., "Population", "UnemploymentRatePct")
    /// </summary>
    string Metric,

    /// <summary>
    /// Optional year for filtering. If null, uses latest available year.
    /// </summary>
    int? Year,

    /// <summary>
    /// Sort direction for the metric: "Ascending" or "Descending"
    /// </summary>
    string SortDirection,

    /// <summary>
    /// Number of results to return (1-100)
    /// </summary>
    int Limit);
