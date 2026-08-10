using CBS.BI.Application.Analytics.Models;

namespace CBS.BI.Application.Analytics.Abstractions;

public interface IQueryCostEstimator
{
    Task<QueryCostEstimate> EstimateAsync(
        string query,
     CancellationToken cancellationToken);
}
