using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Analytics.Abstractions;

/// <summary>
/// Represents the capability of retrieving semantically relevant knowledge items for analytics query generation.
/// 
/// This abstraction enables future Retrieval-Augmented Generation (RAG) implementations while maintaining
/// a clean separation of concerns between knowledge retrieval and context formatting.
/// 
/// The semantic retriever is responsible for:
/// - Analyzing a natural-language question to identify semantic relevance
/// - Selecting knowledge items (from metadata, business glossaries, rules, dictionaries, etc.)
/// - Respecting the current user's data access permissions
/// - Returning a ranked or curated collection of relevant items
/// 
/// The retriever does NOT:
/// - Generate SQL or build prompts
/// - Call AI models directly
/// - Execute BigQuery or other databases
/// - Make authorization decisions beyond respecting CurrentUser
/// - Implement vector search, embeddings, or RAG infrastructure in this interface
/// 
/// Implementation patterns may include:
/// - Rule-based selection based on question keywords
/// - Future: Semantic similarity via embeddings and vector search
/// - Future: RAG systems combining multiple knowledge sources
/// - Hybrid approaches combining multiple retrieval strategies
/// 
/// The returned knowledge items are later transformed into formatted context text by
/// IAnalyticsSemanticContextProvider, which handles HOW the knowledge is represented for the LLM.
/// </summary>
public interface IAnalyticsSemanticRetriever
{
    /// <summary>
    /// Retrieves semantically relevant knowledge items for a natural-language analytics question.
    /// </summary>
    /// <param name="question">A natural-language question about analytics data.</param>
    /// <param name="user">The current authenticated user, whose permissions constrain which knowledge items are returned.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// A collection of semantically relevant AnalyticsSemanticKnowledgeItem objects.
    /// 
    /// The collection:
    /// - Contains only items the user is authorized to access
    /// - May be ordered by relevance (implementation-dependent)
    /// - May be empty if no relevant knowledge is found
    /// - Should represent the most pertinent semantic information for generating SQL to answer the question
    /// </returns>
    /// <remarks>
    /// Future implementations may accept additional parameters (e.g., TopK for result limiting,
    /// threshold for relevance scoring, or filtering criteria). For now, the implementation
    /// determines ranking and filtering internally.
    /// </remarks>
    Task<IReadOnlyCollection<AnalyticsSemanticKnowledgeItem>> RetrieveAsync(
        string question,
        CurrentUser user,
 CancellationToken cancellationToken);
}
