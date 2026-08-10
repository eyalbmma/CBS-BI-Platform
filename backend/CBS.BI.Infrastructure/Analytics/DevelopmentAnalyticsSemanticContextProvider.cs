using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Security.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace CBS.BI.Infrastructure.Analytics;

/// <summary>
/// Development/POC implementation of IAnalyticsSemanticContextProvider.
/// 
/// This provider uses semantic retrieval to identify relevant knowledge items
/// and converts them into semantic context text for use by Gemini or other LLMs.
/// 
/// This implementation integrates the retrieval-augmented generation (RAG) baseline:
/// 1. Retrieves relevant semantic knowledge items using IAnalyticsSemanticRetriever
/// 2. Formats those items into human-readable context text
/// 3. Returns the formatted context to GenerateAnalyticsQueryUseCase
/// 
/// It does NOT hardcode table or column information.
/// It does NOT query BigQuery directly.
/// It does NOT implement embeddings or vector search.
/// 
/// This implementation MUST be registered only in Development environment.
/// </summary>
public sealed class DevelopmentAnalyticsSemanticContextProvider : IAnalyticsSemanticContextProvider
{
    private readonly IAnalyticsSemanticRetriever _semanticRetriever;
    private readonly ILogger<DevelopmentAnalyticsSemanticContextProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the DevelopmentAnalyticsSemanticContextProvider.
    /// </summary>
    /// <param name="semanticRetriever">Service to retrieve relevant semantic knowledge items.</param>
    /// <param name="logger">Logger for diagnostic timing information.</param>
    public DevelopmentAnalyticsSemanticContextProvider(
      IAnalyticsSemanticRetriever semanticRetriever,
      ILogger<DevelopmentAnalyticsSemanticContextProvider> logger)
    {
        _semanticRetriever = semanticRetriever ?? throw new ArgumentNullException(nameof(semanticRetriever));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves semantic context by fetching relevant knowledge items and formatting them for the LLM.
    /// 
    /// Flow:
    /// 1. Validate inputs
    /// 2. Call IAnalyticsSemanticRetriever.RetrieveAsync to get relevant knowledge items
    /// 3. Format those items into semantic context text
    /// 4. Return formatted context to be used by Gemini
    /// 
    /// If retrieval returns zero items, returns an empty string.
    /// GenerateAnalyticsQueryUseCase will reject empty context.
    /// </summary>
    /// <param name="question">A natural-language question about analytics data.</param>
    /// <param name="user">The current authenticated user.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Semantic context built from retrieved knowledge items.</returns>
    /// <exception cref="ArgumentException">If question is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">If user is null.</exception>
    public async Task<string> GetContextAsync(
        string question,
        CurrentUser user,
        CancellationToken cancellationToken)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("Question must not be null, empty, or whitespace.", nameof(question));
        }

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "CurrentUser must not be null.");
        }

        // Measure semantic retrieval
        var retrievalStopwatch = Stopwatch.StartNew();

        // Retrieve relevant semantic knowledge items
        var retrievedItems = await _semanticRetriever.RetrieveAsync(question, user, cancellationToken);

        retrievalStopwatch.Stop();
        _logger.LogInformation("Analytics timing: SemanticRetrieval completed in {DurationMs} ms. Retrieved {ItemCount} semantic items.",
          retrievalStopwatch.ElapsedMilliseconds, retrievedItems.Count);

        // If no items retrieved, return empty context
        if (retrievedItems.Count == 0)
        {
            return string.Empty;
        }

        // Format retrieved items into semantic context text
        var context = new StringBuilder();

        foreach (var item in retrievedItems)
        {
            context.AppendLine("Semantic item:");
            context.AppendLine($"Type: {item.SourceType}");

            if (!string.IsNullOrWhiteSpace(item.SourceReference))
            {
                context.AppendLine($"Reference: {item.SourceReference}");
            }

            context.AppendLine("Content:");
            context.AppendLine(item.Content);
            context.AppendLine();
        }

        return context.ToString();
    }
}
