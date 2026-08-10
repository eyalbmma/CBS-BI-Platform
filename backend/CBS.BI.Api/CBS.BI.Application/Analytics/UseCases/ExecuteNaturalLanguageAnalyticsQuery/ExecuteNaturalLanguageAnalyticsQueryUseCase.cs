using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Analytics.UseCases.ExecuteAnalyticsQuery;
using CBS.BI.Application.Analytics.UseCases.GenerateAnalyticsQuery;

namespace CBS.BI.Application.Analytics.UseCases.ExecuteNaturalLanguageAnalyticsQuery;

/// <summary>
/// Orchestration use case that combines natural-language question answering with query execution.
/// 
/// This use case bridges two distinct responsibilities:
/// 1. GenerateAnalyticsQueryUseCase — converts natural language to safe SQL
/// 2. ExecuteAnalyticsQueryUseCase — executes SQL and returns results
/// 
/// Flow:
/// Natural Language Question
///     ?
/// GenerateAnalyticsQueryUseCase (validates question, retrieves semantic context, calls Gemini, validates SQL)
///     ?
/// Safe SQL
///     ?
/// ExecuteAnalyticsQueryUseCase (validates user authorization, re-validates SQL, estimates cost, executes query)
///     ?
/// BigQuery Results
/// 
/// Security Principles:
/// - Both use cases independently handle their own authentication and authorization
/// - Generated SQL is validated by GenerateAnalyticsQueryUseCase
/// - Generated SQL is re-validated by ExecuteAnalyticsQueryUseCase (defense in depth)
/// - Cost limits are enforced before BigQuery execution
/// - All exceptions propagate naturally without conversion or swallowing
/// 
/// This use case is ONLY an orchestrator and does NOT:
/// - Duplicate authentication, authorization, or validation logic
/// - Make direct BigQuery calls
/// - Call Gemini or other AI services
/// - Handle semantic context retrieval
/// - Estimate query costs
/// - Apply cost limits
/// 
/// All of these responsibilities remain with their respective existing services and use cases.
/// </summary>
public sealed class ExecuteNaturalLanguageAnalyticsQueryUseCase
{
    private readonly GenerateAnalyticsQueryUseCase _generateAnalyticsQueryUseCase;
    private readonly ExecuteAnalyticsQueryUseCase _executeAnalyticsQueryUseCase;

    /// <summary>
    /// Initializes a new instance of the ExecuteNaturalLanguageAnalyticsQueryUseCase.
    /// </summary>
    /// <param name="generateAnalyticsQueryUseCase">Use case for generating safe SQL from natural language.</param>
    /// <param name="executeAnalyticsQueryUseCase">Use case for executing SQL and retrieving results.</param>
    public ExecuteNaturalLanguageAnalyticsQueryUseCase(
        GenerateAnalyticsQueryUseCase generateAnalyticsQueryUseCase,
        ExecuteAnalyticsQueryUseCase executeAnalyticsQueryUseCase)
    {
        _generateAnalyticsQueryUseCase = generateAnalyticsQueryUseCase ?? throw new ArgumentNullException(nameof(generateAnalyticsQueryUseCase));
  _executeAnalyticsQueryUseCase = executeAnalyticsQueryUseCase ?? throw new ArgumentNullException(nameof(executeAnalyticsQueryUseCase));
    }

    /// <summary>
    /// Executes a natural-language analytics question end-to-end.
    /// 
    /// Execution flow:
    /// 1. Generate safe SQL from the natural-language question
    /// 2. Execute the generated SQL against BigQuery
    /// 3. Return both the SQL and the execution result
    /// 
    /// If SQL generation fails, execution does not occur.
    /// If execution fails, the original exception propagates.
    /// Both use cases perform their own validation and authorization independently.
 /// </summary>
    /// <param name="question">A natural-language question about analytics data.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A result containing the generated SQL and the BigQuery execution results.</returns>
    /// <exception cref="ArgumentException">If question is null, empty, or whitespace.</exception>
    /// <exception cref="OperationCanceledException">If the operation is cancelled.</exception>
    /// <exception cref="AnalyticsQueryGenerationException">If SQL generation fails.</exception>
    /// <exception cref="UnsafeAnalyticsQueryException">If generated SQL fails safety validation.</exception>
    /// <exception cref="UserNotAuthenticatedException">If the user is not authenticated.</exception>
    /// <exception cref="AnalyticsAuthorizationException">If the user is not authorized to execute queries.</exception>
    /// <exception cref="QueryCostLimitExceededException">If the query cost exceeds the limit.</exception>
    public async Task<NaturalLanguageAnalyticsQueryResult> ExecuteAsync(
     string question,
    CancellationToken cancellationToken)
  {
        // Step 1: Generate safe SQL from natural-language question
        // This validates the question, retrieves semantic context, calls Gemini, and validates the SQL.
        var generatedSql = await _generateAnalyticsQueryUseCase.ExecuteAsync(
       question,
    cancellationToken);

        // Step 2: Execute the generated SQL
        // This validates user authorization, re-validates SQL, estimates cost, and executes the query.
   var executionResult = await _executeAnalyticsQueryUseCase.ExecuteAsync(
  generatedSql,
   cancellationToken);

      // Step 3: Return both the SQL and the execution result
        return new NaturalLanguageAnalyticsQueryResult(
    Sql: generatedSql,
   Result: executionResult);
    }
}
