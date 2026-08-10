namespace CBS.BI.Api.Contracts.Analytics;

/// <summary>
/// Request contract for the natural-language analytics query endpoint.
/// 
/// Allows clients to ask questions about analytics data using natural language.
/// The question is processed through semantic metadata retrieval, AI-based SQL generation,
/// and secure BigQuery execution.
/// </summary>
public sealed class AskAnalyticsQuestionRequest
{
    /// <summary>
    /// A natural-language question about analytics data.
    /// Example: "Which city has the largest population?"
    /// </summary>
    public required string Question { get; init; }
}
