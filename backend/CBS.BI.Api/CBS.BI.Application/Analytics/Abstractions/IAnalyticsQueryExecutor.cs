using CBS.BI.Application.Analytics.Models;

namespace CBS.BI.Application.Analytics.Abstractions;

public interface IAnalyticsQueryExecutor
{
    Task<AnalyticsQueryResult> ExecuteAsync(
  string query,
        CancellationToken cancellationToken);
}
