using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Exceptions;
using CBS.BI.Application.Security.Abstractions;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Analytics.UseCases.GenerateAnalyticsQuery;

/// <summary>
/// Use case for generating safe analytics SQL from a natural-language question.
/// 
/// This use case orchestrates:
/// 1. Natural-language question validation
/// 2. Current user resolution
/// 3. Permission-aware semantic context retrieval via IAnalyticsSemanticContextProvider
/// 4. AI query generation via IAnalyticsQueryGenerator
/// 5. Application-level SQL safety validation via IAnalyticsQuerySafetyValidator
/// 6. Return of the validated generated SQL
/// 
/// The generated SQL is NOT executed by this use case.
/// It will be passed through the existing secure execution pipeline in a separate orchestration step.
/// 
/// Security Principle:
/// AI output is treated as untrusted input and must pass through the existing
/// IAnalyticsQuerySafetyValidator before being returned to the caller.
/// Semantic context is retrieved with the current user to ensure permission-aware filtering.
/// </summary>
public sealed class GenerateAnalyticsQueryUseCase
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAnalyticsSemanticContextProvider _semanticContextProvider;
    private readonly IAnalyticsQueryGenerator _queryGenerator;
    private readonly IAnalyticsQuerySafetyValidator _safetyValidator;

  /// <summary>
    /// Initializes a new instance of the GenerateAnalyticsQueryUseCase.
    /// </summary>
    /// <param name="currentUserContext">Provides the current authenticated user.</param>
    /// <param name="semanticContextProvider">Retrieves permission-aware semantic context for query generation.</param>
    /// <param name="queryGenerator">The AI query generator for natural-language to SQL conversion.</param>
    /// <param name="safetyValidator">The validator for ensuring generated SQL is read-only.</param>
    public GenerateAnalyticsQueryUseCase(
   ICurrentUserContext currentUserContext,
        IAnalyticsSemanticContextProvider semanticContextProvider,
   IAnalyticsQueryGenerator queryGenerator,
        IAnalyticsQuerySafetyValidator safetyValidator)
    {
        _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));
     _semanticContextProvider = semanticContextProvider ?? throw new ArgumentNullException(nameof(semanticContextProvider));
        _queryGenerator = queryGenerator ?? throw new ArgumentNullException(nameof(queryGenerator));
        _safetyValidator = safetyValidator ?? throw new ArgumentNullException(nameof(safetyValidator));
    }

    /// <summary>
    /// Generates safe analytics SQL from a natural-language question.
    /// 
    /// Execution flow:
    /// 1. Validate question
    /// 2. Get current user
    /// 3. Retrieve permission-aware semantic context
    /// 4. Validate semantic context is available
    /// 5. Generate SQL via IAnalyticsQueryGenerator
    /// 6. Validate generated SQL via IAnalyticsQuerySafetyValidator
    /// 7. Return validated SQL
    /// </summary>
    /// <param name="question">A natural-language question about analytics data.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A validated, safe SQL query string generated from the question.</returns>
    /// <exception cref="ArgumentException">If question is null, empty, or whitespace.</exception>
    /// <exception cref="OperationCanceledException">If the operation is cancelled.</exception>
    /// <exception cref="AnalyticsQueryGenerationException">
    /// If no semantic context is available, or if the AI generation fails.
    /// </exception>
    /// <exception cref="UnsafeAnalyticsQueryException">If the generated SQL fails safety validation.</exception>
    public async Task<string> ExecuteAsync(
        string question,
        CancellationToken cancellationToken)
    {
        // Validate input before making external calls
        if (string.IsNullOrWhiteSpace(question))
 {
throw new ArgumentException("Question must not be null, empty, or whitespace.", nameof(question));
        }

        // Get current user for permission-aware context retrieval
        var currentUser = _currentUserContext.GetCurrentUser();

        // Retrieve permission-aware semantic context from provider
        var semanticContext = await _semanticContextProvider.GetContextAsync(
       question,
            currentUser,
            cancellationToken);

        // Validate semantic context is available
        if (string.IsNullOrWhiteSpace(semanticContext))
        {
            throw new AnalyticsQueryGenerationException(
              "No semantic context is available for the requested analytics question.");
        }

        // Generate SQL from natural-language question
     var generatedSql = await _queryGenerator.GenerateSqlAsync(
    question,
          semanticContext,
 cancellationToken);

// Validate that generated SQL is safe (read-only)
   // Treat AI output as untrusted input
        _safetyValidator.EnsureSafe(generatedSql);

        return generatedSql;
    }
}
