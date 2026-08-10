using CBS.BI.Application.Analytics.Models;

namespace CBS.BI.Application.Analytics.Abstractions;

/// <summary>
/// Provides allowlisted domain/metric mappings for visual query generation.
/// 
/// This abstraction centralizes all valid domain/metric combinations,
/// preventing SQL injection through client-provided table/column names.
/// 
/// Clients can only select from pre-configured mappings registered on the server.
/// </summary>
public interface IVisualQueryCatalog
{
    /// <summary>
    /// Retrieves all supported domains and their metrics.
    /// </summary>
    IReadOnlyCollection<VisualQueryMetricMapping> GetAllMappings();

    /// <summary>
    /// Finds a metric mapping by domain and metric name.
    /// </summary>
    /// <returns>Mapping if found, null otherwise.</returns>
    VisualQueryMetricMapping? FindMapping(string domain, string metric);

    /// <summary>
    /// Gets all metrics supported for a given domain.
    /// </summary>
 IReadOnlyCollection<string> GetMetricsForDomain(string domain);
}
