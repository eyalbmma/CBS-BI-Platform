using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Infrastructure.Configuration;
using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace CBS.BI.Infrastructure.Analytics;

public sealed class BigQueryQueryCostEstimator : IQueryCostEstimator
{
    private readonly IOptions<GoogleCloudOptions> _options;
    private readonly ILogger<BigQueryQueryCostEstimator> _logger;

    public BigQueryQueryCostEstimator(
  IOptions<GoogleCloudOptions> options,
   ILogger<BigQueryQueryCostEstimator> logger)
    {
        _options = options;
     _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

public async Task<QueryCostEstimate> EstimateAsync(
    string query,
 CancellationToken cancellationToken)
    {
 if (string.IsNullOrWhiteSpace(query))
      {
          throw new ArgumentException("Query cannot be null, empty, or whitespace.", nameof(query));
  }

     var options = _options.Value;
        var client = BigQueryClient.Create(options.ProjectId);

        var queryOptions = new QueryOptions
 {
      DryRun = true,
            UseQueryCache = false
      };

        // Measure BigQuery dry run timing
  var dryRunStopwatch = Stopwatch.StartNew();

        var queryJob = await client.CreateQueryJobAsync(
          query,
       null,
     queryOptions,
      cancellationToken);

        dryRunStopwatch.Stop();

        var totalBytesProcessed = queryJob.Resource?.Statistics?.Query?.TotalBytesProcessed ?? 0L;

        _logger.LogInformation("Analytics timing: BigQueryDryRun completed in {DurationMs} ms. EstimatedBytes={EstimatedBytes}",
    dryRunStopwatch.ElapsedMilliseconds, totalBytesProcessed);

       var maximumBytesProcessed = options.BigQuery.MaximumBytesProcessed;
   var isWithinLimit = maximumBytesProcessed <= 0 || totalBytesProcessed <= maximumBytesProcessed;

        return new QueryCostEstimate
 {
      EstimatedBytesProcessed = totalBytesProcessed,
      IsWithinLimit = isWithinLimit
 };
    }
}
