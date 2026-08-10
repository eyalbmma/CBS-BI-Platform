using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Analytics.UseCases.ExecuteVisualQuery;

/// <summary>
/// Use case for executing visual queries (structured query generation without SQL).
/// 
/// Visual Query flow:
/// 1. Validate request (domain, metric, sort, limit)
/// 2. Look up mapping in server-side allowlist
/// 3. Generate deterministic SQL from validated mapping
/// 4. Execute existing SQL safety validation
/// 5. Execute existing BigQuery dry run for cost estimation
/// 6. If cost is acceptable, execute via existing BigQuery executor
/// 7. Return results
/// 
/// SQL injection prevention: All table/column names come exclusively from the server-side
/// allowlist catalog. Client can only choose from pre-defined domain/metric combinations.
/// </summary>
public sealed class ExecuteVisualQueryUseCase
{
    private const int MinLimit = 1;
    private const int MaxLimit = 100;

    private readonly IVisualQueryCatalog _catalog;
    private readonly IAnalyticsQuerySafetyValidator _safetyValidator;
    private readonly IQueryCostEstimator _costEstimator;
    private readonly IAnalyticsQueryExecutor _executor;

    public ExecuteVisualQueryUseCase(
        IVisualQueryCatalog catalog,
        IAnalyticsQuerySafetyValidator safetyValidator,
        IQueryCostEstimator costEstimator,
IAnalyticsQueryExecutor executor)
    {
    _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _safetyValidator = safetyValidator ?? throw new ArgumentNullException(nameof(safetyValidator));
        _costEstimator = costEstimator ?? throw new ArgumentNullException(nameof(costEstimator));
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
    }

    public async Task<(string Sql, AnalyticsQueryResult Result)> ExecuteAsync(
     VisualQueryDefinition request,
      CurrentUser user,
        CancellationToken cancellationToken)
 {
        if (request == null)
        {
     throw new ArgumentNullException(nameof(request));
        }

        if (user == null)
        {
    throw new ArgumentNullException(nameof(user));
        }

        // Validate domain and metric
      var mapping = _catalog.FindMapping(request.Domain, request.Metric);
      if (mapping == null)
    {
            throw new InvalidOperationException(
$"Unsupported domain or metric: {request.Domain}/{request.Metric}");
     }

        // Validate sort direction
        var sortDirection = ValidateSortDirection(request.SortDirection);

        // Validate limit
        ValidateLimit(request.Limit);

        // Generate SQL with validated parameters
     var sql = GenerateVisualQuerySql(mapping, request.Year, sortDirection, request.Limit);

        // Validate SQL safety (read-only check)
        _safetyValidator.EnsureSafe(sql);

        // Estimate cost
      var costEstimate = await _costEstimator.EstimateAsync(sql, cancellationToken);
        if (!costEstimate.IsWithinLimit)
        {
        throw new InvalidOperationException(
   "Query cost exceeds maximum bytes processed limit.");
    }

        // Execute query
  var result = await _executor.ExecuteAsync(sql, cancellationToken);

        return (sql, result);
    }

    private static string ValidateSortDirection(string sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortDirection))
        {
          throw new ArgumentException("Sort direction cannot be empty.", nameof(sortDirection));
  }

        // Map user-friendly values to SQL keywords
        // Only these allowlisted values are accepted from the client
   return sortDirection.ToLowerInvariant() switch
        {
    "ascending" => "ASC",
      "descending" => "DESC",
      _ => throw new ArgumentException(
      "Sort direction must be 'Ascending' or 'Descending'.", nameof(sortDirection))
   };
    }

    private static void ValidateLimit(int limit)
    {
        if (limit < MinLimit || limit > MaxLimit)
        {
          throw new ArgumentException(
     $"Limit must be between {MinLimit} and {MaxLimit}.", nameof(limit));
        }
    }

    /// <summary>
    /// Generates deterministic SQL for visual query.
    /// 
    /// All table and column identifiers come exclusively from the mapping catalog.
    /// Numeric values (year, limit) are validated before SQL generation and embedded as literals.
    /// No client input is included in the query structure.
    /// </summary>
    private static string GenerateVisualQuerySql(
     VisualQueryMetricMapping mapping,
        int? year,
        string sortDirection,
        int limit)
    {
        var query = year.HasValue
     ? $@"
SELECT City, `{mapping.ColumnName}`
FROM `{mapping.TableReference}`
WHERE Year = {year}
ORDER BY `{mapping.ColumnName}` {sortDirection}
LIMIT {limit}"
     : $@"
SELECT City, `{mapping.ColumnName}`
FROM `{mapping.TableReference}`
WHERE Year = (SELECT MAX(Year) FROM `{mapping.TableReference}`)
ORDER BY `{mapping.ColumnName}` {sortDirection}
LIMIT {limit}";

        return query;
    }
}
