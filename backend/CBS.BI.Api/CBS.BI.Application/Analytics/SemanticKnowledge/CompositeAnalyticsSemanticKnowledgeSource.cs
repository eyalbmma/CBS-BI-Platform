using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CBS.BI.Application.Analytics.SemanticKnowledge;

/// <summary>
/// Composite semantic knowledge source that combines multiple knowledge sources into one corpus.
/// 
/// This source aggregates semantic knowledge items from multiple providers, creating a unified
/// searchable corpus that can be used for retrieval and ranking by IAnalyticsSemanticRetriever.
/// 
/// For the POC, combines:
/// - MetadataAnalyticsSemanticKnowledgeSource (BigQuery table/column metadata)
/// - BusinessDictionaryAnalyticsSemanticKnowledgeSource (cross-language business terms)
/// 
/// The composite does not perform retrieval, ranking, or filtering. It simply combines the
/// results from its component sources in a deterministic order.
/// 
/// Future versions may include:
/// - Additional knowledge sources (business rules, data lineage, documentation)
/// - Lazy loading or streaming of items
/// - Deduplication strategies
/// - Knowledge source prioritization
/// 
/// But for now, the composite is a simple transparent aggregator.
/// </summary>
public sealed class CompositeAnalyticsSemanticKnowledgeSource : IAnalyticsSemanticKnowledgeSource
{
    private readonly MetadataAnalyticsSemanticKnowledgeSource _metadataSource;
    private readonly BusinessDictionaryAnalyticsSemanticKnowledgeSource _businessDictionarySource;
 private readonly ILogger<CompositeAnalyticsSemanticKnowledgeSource> _logger;

    /// <summary>
    /// Initializes a new instance of the CompositeAnalyticsSemanticKnowledgeSource.
    /// </summary>
    /// <param name="metadataSource">Provider of metadata-based semantic knowledge items.</param>
    /// <param name="businessDictionarySource">Provider of business dictionary semantic knowledge items.</param>
    /// <param name="logger">Optional logger for diagnostic information. Uses NullLogger if not provided.</param>
    public CompositeAnalyticsSemanticKnowledgeSource(
        MetadataAnalyticsSemanticKnowledgeSource metadataSource,
        BusinessDictionaryAnalyticsSemanticKnowledgeSource businessDictionarySource,
   ILogger<CompositeAnalyticsSemanticKnowledgeSource>? logger = null)
    {
      _metadataSource = metadataSource ?? throw new ArgumentNullException(nameof(metadataSource));
        _businessDictionarySource = businessDictionarySource ?? throw new ArgumentNullException(nameof(businessDictionarySource));
        _logger = logger ?? NullLogger<CompositeAnalyticsSemanticKnowledgeSource>.Instance;
    }

    /// <summary>
    /// Retrieves the combined semantic knowledge corpus from all sources.
    /// 
    /// Execution:
    /// 1. Call metadataSource.GetItemsAsync(user, cancellationToken)
/// 2. Call businessDictionarySource.GetItemsAsync(user, cancellationToken)
    /// 3. Combine results: metadata items first, then business dictionary items
    /// 4. Return as read-only collection
    /// 
    /// Ordering is deterministic: items from each source appear in the order they were returned
    /// by that source, and sources are combined in a fixed order (metadata, then business dictionary).
    /// </summary>
    public async Task<IReadOnlyCollection<AnalyticsSemanticKnowledgeItem>> GetItemsAsync(
        CurrentUser user,
        CancellationToken cancellationToken)
    {
        // Retrieve items from metadata source
     var metadataItems = await _metadataSource.GetItemsAsync(user, cancellationToken);

    // Retrieve items from business dictionary source
        var businessDictionaryItems = await _businessDictionarySource.GetItemsAsync(user, cancellationToken);

        _logger.LogInformation("SEMANTIC_DIAGNOSTIC: CompositeAnalyticsSemanticKnowledgeSource combining {MetadataCount} metadata items + {BusinessDictCount} dictionary items = {TotalCount} total items",
 metadataItems.Count, businessDictionaryItems.Count, metadataItems.Count + businessDictionaryItems.Count);

        // Combine results: metadata first, then business dictionary
  var combinedItems = new List<AnalyticsSemanticKnowledgeItem>(
metadataItems.Count + businessDictionaryItems.Count);

        foreach (var item in metadataItems)
        {
combinedItems.Add(item);
        }

        foreach (var item in businessDictionaryItems)
    {
            combinedItems.Add(item);
      }

        return combinedItems.AsReadOnly();
    }
}
