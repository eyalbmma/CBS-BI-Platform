using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Analytics.SemanticKnowledge;

/// <summary>
/// Metadata-backed semantic knowledge source that converts structured analytics metadata
/// into semantic knowledge items suitable for retrieval and LLM-based SQL generation.
/// 
/// This implementation transforms:
/// - IAnalyticsMetadataCatalog structured metadata (tables, columns, descriptions)
/// - Into AnalyticsSemanticKnowledgeItem objects representing semantic concepts
/// 
/// Knowledge item types generated:
/// 1. TableMetadata items - one per table with table-level semantic information
/// 2. ColumnMetadata items - one per column with column-level semantic information
/// 
/// The resulting semantic corpus is deterministic and suitable for:
/// - Future retrieval-augmented generation (RAG)
/// - Semantic search with embeddings
/// - Keyword-based retrieval
/// - Any algorithm that ranks items by relevance to a natural-language question
/// 
/// The transformation preserves table and column ordering from the metadata source
/// for reproducible, deterministic output.
/// </summary>
public sealed class MetadataAnalyticsSemanticKnowledgeSource : IAnalyticsSemanticKnowledgeSource
{
    private readonly IAnalyticsMetadataCatalog _metadataCatalog;

    /// <summary>
    /// Initializes a new instance of the MetadataAnalyticsSemanticKnowledgeSource.
    /// </summary>
    /// <param name="metadataCatalog">Service providing structured analytics metadata from BigQuery.</param>
    public MetadataAnalyticsSemanticKnowledgeSource(IAnalyticsMetadataCatalog metadataCatalog)
    {
    _metadataCatalog = metadataCatalog ?? throw new ArgumentNullException(nameof(metadataCatalog));
    }

    /// <summary>
    /// Retrieves the complete semantic knowledge corpus by transforming structured metadata.
    /// </summary>
    public async Task<IReadOnlyCollection<AnalyticsSemanticKnowledgeItem>> GetItemsAsync(
        CurrentUser user,
        CancellationToken cancellationToken)
    {
        // Retrieve structured metadata from BigQuery
      var tables = await _metadataCatalog.GetTablesAsync(user, cancellationToken);

        var items = new List<AnalyticsSemanticKnowledgeItem>();

    // Transform metadata into semantic knowledge items
    foreach (var table in tables)
        {
            // Add table metadata item
    var tableItem = CreateTableMetadataItem(table);
      items.Add(tableItem);

       // Add column metadata items
            foreach (var column in table.Columns)
   {
             var columnItem = CreateColumnMetadataItem(table, column);
                items.Add(columnItem);
            }
        }

        return items.AsReadOnly();
    }

 /// <summary>
    /// Creates a semantic knowledge item from table metadata.
    /// </summary>
    private static AnalyticsSemanticKnowledgeItem CreateTableMetadataItem(AnalyticsTableMetadata table)
    {
      var fullyQualifiedName = $"{table.ProjectId}.{table.DatasetId}.{table.TableId}";
        var id = $"table:{fullyQualifiedName}";

        var contentLines = new List<string>
        {
    $"Table: {table.TableId}",
            $"Project: {table.ProjectId}",
 $"Dataset: {table.DatasetId}",
         $"Fully qualified table: `{fullyQualifiedName}`"
        };

     if (!string.IsNullOrWhiteSpace(table.Description))
        {
contentLines.Add($"Description: {table.Description}");
        }

        var content = string.Join("\n", contentLines);

        return new AnalyticsSemanticKnowledgeItem(
 Id: id,
    Content: content,
   SourceType: "TableMetadata",
            SourceReference: fullyQualifiedName);
    }

    /// <summary>
    /// Creates a semantic knowledge item from column metadata.
    /// </summary>
    private static AnalyticsSemanticKnowledgeItem CreateColumnMetadataItem(
        AnalyticsTableMetadata table,
        AnalyticsColumnMetadata column)
    {
     var fullyQualifiedName = $"{table.ProjectId}.{table.DatasetId}.{table.TableId}.{column.Name}";
        var id = $"column:{fullyQualifiedName}";

     var contentLines = new List<string>
 {
       $"Column: {column.Name}",
          $"Data type: {column.DataType}",
    $"Table: {table.TableId}",
            $"Dataset: {table.DatasetId}",
            $"Project: {table.ProjectId}"
        };

        if (!string.IsNullOrWhiteSpace(column.Description))
   {
   contentLines.Add($"Description: {column.Description}");
        }

        var content = string.Join("\n", contentLines);

 return new AnalyticsSemanticKnowledgeItem(
     Id: id,
            Content: content,
 SourceType: "ColumnMetadata",
            SourceReference: fullyQualifiedName);
    }
}
