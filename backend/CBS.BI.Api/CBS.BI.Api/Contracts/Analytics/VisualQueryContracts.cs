namespace CBS.BI.Api.Contracts.Analytics;

/// <summary>
/// Visual query request contract.
/// 
/// Clients specify what they want (domain, metric, filters, sort, limit)
/// rather than writing SQL.
/// </summary>
public sealed record VisualQueryRequest(
  /// <summary>
    /// Analytics domain: "Population", "Employment", "Housing", or "Education"
    /// </summary>
    string Domain,

    /// <summary>
    /// Metric within the domain
    /// </summary>
    string Metric,

    /// <summary>
    /// Optional year filter. If null, uses latest available year.
    /// </summary>
    int? Year,

  /// <summary>
    /// Sort direction: "Ascending" or "Descending"
    /// </summary>
    string SortDirection,

    /// <summary>
    /// Number of results (1-100)
    /// </summary>
    int Limit);

/// <summary>
/// Visual query response contract.
/// 
/// Returns generated SQL and query results in the same format as natural language queries,
/// allowing frontend to reuse ResultTable component.
/// </summary>
public sealed record VisualQueryResponse(
    /// <summary>
    /// The generated SQL query
    /// </summary>
    string Sql,

    /// <summary>
    /// Query result: columns and rows
    /// </summary>
    object Result);
