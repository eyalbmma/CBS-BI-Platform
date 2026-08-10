using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Analytics.Abstractions;

/// <summary>
/// Represents the capability of providing a semantic knowledge corpus for analytics query generation.
/// 
/// A semantic knowledge source is responsible for returning all available semantic knowledge items
/// that may be relevant to a user's natural-language analytics question. It represents the
/// complete searchable corpus of semantic knowledge.
/// 
/// The knowledge source decides WHAT KNOWLEDGE EXISTS and is accessible to the current user.
/// 
/// This abstraction separates two concerns:
/// 
/// 1. IAnalyticsSemanticKnowledgeSource:
///    "What semantic knowledge is available?"
///  - Returns the complete corpus of knowledge items
///    - Ensures items respect user permissions
///    - Technology-independent corpus definition
/// 
/// 2. IAnalyticsSemanticRetriever:
///    "Which knowledge items are relevant to this specific question?"
///    - Searches/ranks items based on question relevance
/// - May use embeddings, keyword search, or other ranking algorithms
///    - Selects a subset of the corpus
/// 
/// This separation allows:
/// - The corpus definition to remain generic and reusable
/// - Retrieval algorithms to evolve independently
/// - Future RAG implementations to swap in different retrieval strategies
/// - Efficient caching of corpus separate from ranking logic
/// </summary>
public interface IAnalyticsSemanticKnowledgeSource
{
    /// <summary>
    /// Retrieves all available semantic knowledge items accessible to the current user.
    /// </summary>
    /// <param name="user">The current authenticated user, whose permissions constrain which knowledge items are returned.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
  /// A collection of all semantic knowledge items available to the user.
    /// 
    /// The collection:
    /// - Contains only items the user is authorized to access
    /// - May include various types of items (TableMetadata, ColumnMetadata, BusinessTerms, etc.)
    /// - Is ordered deterministically for reproducibility
    /// - Should represent the complete searchable corpus
    /// </returns>
    Task<IReadOnlyCollection<AnalyticsSemanticKnowledgeItem>> GetItemsAsync(
        CurrentUser user,
  CancellationToken cancellationToken);
}
