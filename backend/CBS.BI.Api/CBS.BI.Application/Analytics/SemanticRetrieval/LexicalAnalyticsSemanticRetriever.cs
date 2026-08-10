using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CBS.BI.Application.Analytics.SemanticRetrieval;

/// <summary>
/// Baseline lexical semantic retriever for analytics knowledge items.
/// 
/// This retriever uses deterministic lexical/token-based ranking to identify relevant knowledge items
/// from the complete semantic corpus. It is intentionally simple and technology-neutral, designed as
/// a baseline implementation that can be replaced or complemented by more sophisticated retrieval
/// strategies (embeddings, vector search, etc.) in future versions.
/// 
/// Retrieval Flow:
/// 1. Load complete corpus from IAnalyticsSemanticKnowledgeSource
/// 2. Tokenize the natural-language question
/// 3. For each knowledge item, calculate relevance based on token overlap
/// 4. Return top items ordered by relevance score, then original corpus order
/// 
/// Limitations:
/// - Lexical matching only (no semantic understanding)
/// - No cross-language support (Hebrew questions vs English metadata)
/// - Single-language context (English)
/// - No stemming or lemmatization
/// - Deterministic ordering ensures reproducible results
/// 
/// Future enhancements will handle language, embeddings, and advanced retrieval strategies
/// as separate implementations or complementary layers.
/// </summary>
public sealed class LexicalAnalyticsSemanticRetriever : IAnalyticsSemanticRetriever
{
    private const int MaxResults = 10;
    private const int MinimumContainmentTokenLength = 4;

    private readonly IAnalyticsSemanticKnowledgeSource _knowledgeSource;
    private readonly ILogger<LexicalAnalyticsSemanticRetriever> _logger;

    /// <summary>
    /// Initializes a new instance of the LexicalAnalyticsSemanticRetriever.
    /// </summary>
    /// <param name="knowledgeSource">Service providing the complete semantic knowledge corpus.</param>
    /// <param name="logger">Optional logger for diagnostic information. Uses NullLogger if not provided.</param>
    public LexicalAnalyticsSemanticRetriever(
        IAnalyticsSemanticKnowledgeSource knowledgeSource,
        ILogger<LexicalAnalyticsSemanticRetriever>? logger = null)
    {
        _knowledgeSource = knowledgeSource ?? throw new ArgumentNullException(nameof(knowledgeSource));
        _logger = logger ?? NullLogger<LexicalAnalyticsSemanticRetriever>.Instance;
    }

    /// <summary>
    /// Retrieves semantically relevant knowledge items using lexical token overlap.
    /// </summary>
    public async Task<IReadOnlyCollection<AnalyticsSemanticKnowledgeItem>> RetrieveAsync(
        string question,
        CurrentUser user,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Load the complete corpus from the knowledge source
        var corpus = await _knowledgeSource.GetItemsAsync(user, cancellationToken);

        // Normalize and tokenize the question
        var questionTokens = TokenizeQuestion(question);

        _logger.LogInformation("SEMANTIC_DIAGNOSTIC: LexicalAnalyticsSemanticRetriever tokenized question into {TokenCount} tokens: {Tokens}",
            questionTokens.Count, string.Join(", ", questionTokens.OrderBy(t => t)));

        // If no tokens extracted, return empty collection
        if (questionTokens.Count == 0)
        {
            _logger.LogWarning("SEMANTIC_DIAGNOSTIC: LexicalAnalyticsSemanticRetriever extracted zero tokens from question");
            return Array.Empty<AnalyticsSemanticKnowledgeItem>().AsReadOnly();
        }

        // Score each item based on lexical relevance
        var scoredItems = new List<(AnalyticsSemanticKnowledgeItem Item, int Score, int CorpusIndex)>();

        int corpusIndex = 0;
        foreach (var item in corpus)
        {
            int score = CalculateRelevanceScore(item, questionTokens);

            if (score > 0)
            {
                scoredItems.Add((item, score, corpusIndex));
            }

            corpusIndex++;
        }

        // Sort by relevance descending, then by original corpus order ascending
        var rankedItems = scoredItems
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.CorpusIndex)
            .Take(MaxResults)
            .Select(x => x.Item)
            .ToList();

        var matchedIds = rankedItems.Select(item => item.Id).ToList();
        _logger.LogInformation("SEMANTIC_DIAGNOSTIC: LexicalAnalyticsSemanticRetriever matched {MatchCount} items from corpus of {CorpusSize}: {ItemIds}",
            rankedItems.Count, corpus.Count, string.Join(", ", matchedIds));

        return rankedItems.AsReadOnly();
    }

    /// <summary>
    /// Tokenizes a question into normalized lowercase tokens.
    /// </summary>
    /// <remarks>
    /// This is a simple baseline tokenization:
    /// - Lowercases using invariant culture
    /// - Splits on whitespace and common punctuation
    /// - Filters out empty tokens
    /// 
    /// More sophisticated tokenization can be added in future implementations.
    /// </remarks>
    private static ISet<string> TokenizeQuestion(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return new HashSet<string>();
        }

        // Normalize to lowercase using invariant culture
        var normalized = question.ToLowerInvariant();

        // Split on whitespace and common punctuation
        var delimiters = new[] { ' ', '\t', '\n', '\r', '.', ',', '?', '!', ':', ';', '-', '_', '(', ')' };
        var tokens = normalized.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

        return new HashSet<string>(tokens);
    }

    /// <summary>
    /// Calculates the relevance score for a knowledge item against question tokens.
    /// </summary>
    /// <remarks>
    /// Scoring uses two levels of matching:
    /// 
    /// 1. Exact Token Match (higher weight):
    ///    Question token exactly equals an item token.
    /// 
    /// 2. Containment Match (lower weight):
    ///    One token contains another, and the shorter token meets minimum length.
    ///    This handles cases like Hebrew morphology (e.g., "אוכלוסייה" containing "האוכלוסייה")
    ///    without language-specific rules.
    /// 
    /// Scoring: exact_matches * 2 + containment_matches * 1
    /// This ensures exact matches rank above containment-only matches.
    /// 
    /// The algorithm is deterministic and technology-neutral.
    /// </remarks>
    private static int CalculateRelevanceScore(AnalyticsSemanticKnowledgeItem item, ISet<string> questionTokens)
    {
        // Build searchable text from multiple fields
        var searchableText = BuildSearchableText(item);

        // Tokenize the searchable text
        var itemTokens = new HashSet<string>(TokenizeQuestion(searchableText));

        int exactMatches = 0;
        int containmentMatches = 0;

        foreach (var questionToken in questionTokens)
        {
            // Check for exact match
            if (itemTokens.Contains(questionToken))
            {
                exactMatches++;
                continue;
            }

            // Check for containment match (weaker signal)
            foreach (var itemToken in itemTokens)
            {
                if (TokensMatch(questionToken, itemToken))
                {
                    containmentMatches++;
                    break;
                }
            }
        }

        return exactMatches * 2 + containmentMatches * 1;
    }

    /// <summary>
    /// Determines if two tokens match via containment matching.
    /// </summary>
    /// <remarks>
    /// Returns true if:
    /// - Tokens are not already equal (exact match handled separately)
    /// - One token contains the other
    /// - The shorter token meets minimum containment length
    /// 
    /// This avoids false positives from very short tokens like "a", "in", "to".
    /// </remarks>
    private static bool TokensMatch(string token1, string token2)
    {
        if (token1 == token2)
        {
            return false;
        }

        // Determine which is shorter
        string shorter = token1.Length < token2.Length ? token1 : token2;
        string longer = token1.Length < token2.Length ? token2 : token1;

        // Only apply containment matching if shorter token meets minimum length
        if (shorter.Length < MinimumContainmentTokenLength)
        {
            return false;
        }

        // Check if one contains the other
        return longer.Contains(shorter);
    }

    /// <summary>
    /// Builds searchable text from multiple fields of a knowledge item.
    /// </summary>
    private static string BuildSearchableText(AnalyticsSemanticKnowledgeItem item)
    {
        var parts = new List<string>
        {
            item.Content,
            item.SourceType
        };

        if (!string.IsNullOrWhiteSpace(item.SourceReference))
        {
            parts.Add(item.SourceReference);
        }

        return string.Join(" ", parts);
    }
}
