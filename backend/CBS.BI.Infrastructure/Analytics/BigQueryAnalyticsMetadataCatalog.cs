using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;
using CBS.BI.Infrastructure.Configuration;
using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Options;

namespace CBS.BI.Infrastructure.Analytics;

/// <summary>
/// Retrieves analytics metadata from Google BigQuery using INFORMATION_SCHEMA queries.
/// 
/// This implementation queries BigQuery's INFORMATION_SCHEMA views to retrieve
/// table and column metadata for the configured dataset, including descriptions
/// where available.
/// 
/// Uses a fixed two-query pattern regardless of table count:
/// Query 1: Retrieves all BASE TABLE metadata with descriptions
/// Query 2: Retrieves all top-level columns with data types and descriptions
/// </summary>
public sealed class BigQueryAnalyticsMetadataCatalog : IAnalyticsMetadataCatalog
{
    private readonly IOptions<GoogleCloudOptions> _options;

    public BigQueryAnalyticsMetadataCatalog(IOptions<GoogleCloudOptions> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Retrieves analytics table metadata from the configured BigQuery dataset.
    /// 
    /// Note: CurrentUser is passed for consistency with the Application contract
    /// for future permission-aware authorization. At this stage, all tables in the
    /// configured dataset are returned without authorization filtering.
    /// </summary>
    public async Task<IReadOnlyCollection<AnalyticsTableMetadata>> GetTablesAsync(
        CurrentUser user,
        CancellationToken cancellationToken)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var options = _options.Value;
        var client = BigQueryClient.Create(options.ProjectId);
        var projectId = options.ProjectId;
        var datasetId = options.BigQuery.DatasetId;

        // Query 1: Retrieve all BASE TABLE metadata with table descriptions
        var tablesQuery = $@"
SELECT
  t.table_name,
    t.table_type,
    topt.option_value AS description
FROM `{projectId}.{datasetId}.INFORMATION_SCHEMA.TABLES` t
LEFT JOIN `{projectId}.{datasetId}.INFORMATION_SCHEMA.TABLE_OPTIONS` topt
    ON t.table_name = topt.table_name
    AND topt.option_name = 'description'
WHERE t.table_type = 'BASE TABLE'
ORDER BY t.table_name ASC";

        var tableQueryJob = await client.CreateQueryJobAsync(tablesQuery, null, null, cancellationToken);
        var tableRows = await tableQueryJob.GetQueryResultsAsync(null, cancellationToken);

        var tableMetadataLookup = new Dictionary<string, (string ProjectId, string DatasetId, string TableId, string? Description)>();

        foreach (var row in tableRows)
        {
            var tableName = row["table_name"]?.ToString();
  
         if (!string.IsNullOrWhiteSpace(tableName))
  {
         var description = row["description"]?.ToString();
  tableMetadataLookup[tableName] = (projectId, datasetId, tableName, description);
      }
        }

   // Query 2: Retrieve all top-level columns for all tables with column descriptions
        var columnsQuery = $@"
SELECT
    c.table_name,
    c.column_name,
    c.data_type,
    c.ordinal_position,
  cfp.description
FROM `{projectId}.{datasetId}.INFORMATION_SCHEMA.COLUMNS` c
LEFT JOIN `{projectId}.{datasetId}.INFORMATION_SCHEMA.COLUMN_FIELD_PATHS` cfp
    ON c.table_name = cfp.table_name
    AND c.column_name = cfp.column_name
    AND cfp.field_path = c.column_name
ORDER BY c.table_name ASC, c.ordinal_position ASC";

    var columnQueryJob = await client.CreateQueryJobAsync(columnsQuery, null, null, cancellationToken);
        var columnRows = await columnQueryJob.GetQueryResultsAsync(null, cancellationToken);

        // Group columns by table_name
   var columnsByTable = new Dictionary<string, List<AnalyticsColumnMetadata>>();

        foreach (var row in columnRows)
  {
    var tableName = row["table_name"]?.ToString();
            var columnName = row["column_name"]?.ToString();
var dataType = row["data_type"]?.ToString();
  var description = row["description"]?.ToString();

     if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(columnName))
{
                continue;
       }

            if (!columnsByTable.ContainsKey(tableName))
            {
       columnsByTable[tableName] = new List<AnalyticsColumnMetadata>();
            }

            var columnMetadata = new AnalyticsColumnMetadata(
      Name: columnName,
 DataType: dataType ?? string.Empty,
           Description: description);

     columnsByTable[tableName].Add(columnMetadata);
        }

        // Build final result with tables and their columns
        var tables = new List<AnalyticsTableMetadata>();

        foreach (var (tableName, (projId, dsetId, tblId, desc)) in tableMetadataLookup)
        {
        var columns = columnsByTable.ContainsKey(tableName)
          ? columnsByTable[tableName].AsReadOnly()
         : new List<AnalyticsColumnMetadata>().AsReadOnly();

          var tableMetadata = new AnalyticsTableMetadata(
    ProjectId: projId,
          DatasetId: dsetId,
   TableId: tblId,
      Description: desc,
                Columns: columns);

      tables.Add(tableMetadata);
        }

        return tables.AsReadOnly();
    }
}
