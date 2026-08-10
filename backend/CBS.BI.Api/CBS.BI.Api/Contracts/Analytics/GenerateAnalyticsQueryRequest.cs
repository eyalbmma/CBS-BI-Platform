namespace CBS.BI.Api.Contracts.Analytics;

/// <summary>
/// Request contract for the Development analytics query generation endpoint.
/// 
/// This contract allows manual testing of natural-language to SQL generation.
/// Semantic context is now retrieved internally by the Application layer through IAnalyticsSemanticContextProvider.
/// </summary>
public sealed class GenerateAnalyticsQueryRequest
{
    /// <summary>
    /// A natural-language question about analytics data.
    /// Example: "Which city has the largest population?"
    /// </summary>
    public required string Question { get; init; }
}
