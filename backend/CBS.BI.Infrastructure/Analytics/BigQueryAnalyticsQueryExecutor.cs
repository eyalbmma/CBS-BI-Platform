using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Infrastructure.Configuration;
using Google.Cloud.BigQuery.V2;
using Google.Apis.Bigquery.v2.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace CBS.BI.Infrastructure.Analytics;

public sealed class BigQueryAnalyticsQueryExecutor : IAnalyticsQueryExecutor
{
    private readonly IOptions<GoogleCloudOptions> _options;
    private readonly ILogger<BigQueryAnalyticsQueryExecutor> _logger;

    public BigQueryAnalyticsQueryExecutor(
        IOptions<GoogleCloudOptions> options,
        ILogger<BigQueryAnalyticsQueryExecutor> logger)
    {
        _options = options;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AnalyticsQueryResult> ExecuteAsync(
        string query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query cannot be null, empty, or whitespace.", nameof(query));
        }

        var options = _options.Value;
        var client = BigQueryClient.Create(options.ProjectId);

        // Configure query options with execution-time bytes billed limit
        var queryOptions = new QueryOptions
        {
            MaximumBytesBilled = options.BigQuery.MaximumBytesProcessed > 0
                ? options.BigQuery.MaximumBytesProcessed
                : null
        };

        // Measure BigQuery execution timing
        var executionStopwatch = Stopwatch.StartNew();

        var queryJob = await client.CreateQueryJobAsync(query, null, queryOptions, cancellationToken);

        var result = await queryJob.GetQueryResultsAsync(null, cancellationToken);

        executionStopwatch.Stop();

        var columns = result.Schema.Fields.Select(f => f.Name).ToList().AsReadOnly();

        var rows = result
            .Select(row => ConvertRowToDictionary(row, result.Schema.Fields))
            .ToList()
            .AsReadOnly();

        _logger.LogInformation("Analytics timing: BigQueryExecution completed in {DurationMs} ms. RowCount={RowCount}",
            executionStopwatch.ElapsedMilliseconds, rows.Count);

        return new AnalyticsQueryResult
        {
            Columns = columns,
            Rows = rows
        };
    }

    private static IReadOnlyDictionary<string, object?> ConvertRowToDictionary(
        BigQueryRow row,
        IList<TableFieldSchema> fields)
    {
        var dictionary = new Dictionary<string, object?>();

        foreach (var field in fields)
        {
            var value = row[field.Name];
            dictionary[field.Name] = value;
        }

        return dictionary.AsReadOnly();
    }
}
