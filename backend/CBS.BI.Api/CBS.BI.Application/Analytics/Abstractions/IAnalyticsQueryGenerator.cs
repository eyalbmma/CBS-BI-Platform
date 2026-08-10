namespace CBS.BI.Application.Analytics.Abstractions;

/// <summary>
/// Represents the capability of converting a natural-language analytics question into SQL.
/// 
/// This abstraction is provider-independent and allows the Application layer to remain
/// decoupled from any specific AI implementation (Gemini, OpenAI, etc.).
/// 
/// The query generator CONSUMES semantic context but does NOT retrieve it.
/// Semantic context is provided separately by a RAG/metadata layer.
/// 
/// Generated SQL will flow through the existing secure execution pipeline:
/// Authorization ? Query Validation ? Query Safety ? Cost Guard ? BigQuery Execution
/// </summary>
public interface IAnalyticsQueryGenerator
{
    /// <summary>
    /// Generates SQL from a natural-language analytics question with semantic context.
    /// </summary>
    /// <param name="question">A natural-language question about analytics data.</param>
    /// <param name="semanticContext">
    /// Provider-independent textual context that informs query generation.
    /// This context is produced from metadata, semantic layers, business dictionaries, and RAG retrieval.
    /// The query generator CONSUMES this context but does NOT retrieve it.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A SQL query string generated from the question and semantic context.</returns>
    Task<string> GenerateSqlAsync(
        string question,
        string semanticContext,
        CancellationToken cancellationToken);
}
