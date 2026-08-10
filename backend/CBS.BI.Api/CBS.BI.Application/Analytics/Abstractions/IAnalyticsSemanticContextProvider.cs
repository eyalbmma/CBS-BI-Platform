using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Analytics.Abstractions;

/// <summary>
/// Represents the capability of retrieving permission-aware semantic context for analytics query generation.
/// 
/// This abstraction is provider-independent and allows the Application layer to remain
/// decoupled from any specific semantic layer, RAG, or vector database implementation.
/// 
/// The semantic context provider is responsible for:
/// - Retrieving relevant semantic information for a natural-language analytics question
/// - Ensuring the context respects the user's data access permissions
/// - Returning provider-independent textual context suitable for AI query generation
/// 
/// Future implementations may retrieve context from:
/// - Semantic metadata and data dictionaries
/// - Business rule definitions
/// - Dataset/table/column descriptions
/// - RAG (Retrieval-Augmented Generation) systems
/// - Vector search and embeddings
/// - Other semantic layer sources
/// 
/// But the Application layer remains independent from these implementation details.
/// </summary>
public interface IAnalyticsSemanticContextProvider
{
    /// <summary>
    /// Retrieves permission-aware semantic context for a natural-language analytics question.
    /// </summary>
    /// <param name="question">A natural-language question about analytics data.</param>
    /// <param name="user">The current authenticated user, used for permission-aware context filtering.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// A provider-independent textual context string that informs query generation.
    /// This context may include table/column definitions, business rules, metadata, or other
  /// semantic information relevant to answering the question, filtered to resources the user can access.
 /// </returns>
    Task<string> GetContextAsync(
        string question,
        CurrentUser user,
        CancellationToken cancellationToken);
}
