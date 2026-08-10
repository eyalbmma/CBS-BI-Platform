using CBS.BI.Api.Contracts.Analytics;
using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Abstractions;
using CBS.BI.Application.Analytics.UseCases.GenerateAnalyticsQuery;
using CBS.BI.Application.Analytics.UseCases.ExecuteNaturalLanguageAnalyticsQuery;

namespace CBS.BI.Api.Development;

/// <summary>
/// Development-only endpoint mappings for manual testing of analytics features.
/// 
/// These endpoints are strictly for POC/Development use and must NOT be exposed in Production or Staging environments.
/// 
/// The endpoints in this class:
/// - Call GenerateAnalyticsQueryUseCase to generate SQL from natural language
/// - Call IAnalyticsMetadataCatalog to retrieve BigQuery metadata
/// - Call ExecuteNaturalLanguageAnalyticsQueryUseCase to execute the complete end-to-end flow
/// - Do NOT perform RAG or embeddings
/// - Validate generated SQL with IAnalyticsQuerySafetyValidator before returning
/// - Are useful for inspecting Gemini/Vertex AI behavior and diagnosing generation issues
/// </summary>
public static class DevelopmentAnalyticsEndpoints
{
    /// <summary>
    /// Maps Development-only analytics endpoints to the route builder.
    /// 
    /// This method should only be called when app.Environment.IsDevelopment() is true.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    public static void MapDevelopmentAnalyticsEndpoints(this IEndpointRouteBuilder endpoints)
 {
      endpoints.MapPost(
       "/api/development/analytics/generate-sql",
       GenerateSql)
       .WithName("DevelopmentGenerateAnalyticsSql")
.Produces<GeneratedSqlResponse>(StatusCodes.Status200OK)
       .Produces(StatusCodes.Status400BadRequest)
       .Produces(StatusCodes.Status500InternalServerError);

 endpoints.MapGet(
  "/api/development/analytics/metadata",
 GetMetadata)
        .WithName("DevelopmentGetAnalyticsMetadata")
            .Produces<IReadOnlyCollection<AnalyticsTableMetadata>>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status500InternalServerError);

 endpoints.MapPost(
  "/api/development/analytics/ask",
             AskAnalytics)
         .WithName("DevelopmentAskAnalytics")
         .Produces<NaturalLanguageAnalyticsQueryResult>(StatusCodes.Status200OK)
     .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);

   endpoints.MapPost(
        "/api/development/analytics/retrieve-semantic",
            RetrieveSemantic)
         .WithName("DevelopmentRetrieveSemantic")
         .Produces<IReadOnlyCollection<AnalyticsSemanticKnowledgeItem>>(StatusCodes.Status200OK)
  .Produces(StatusCodes.Status400BadRequest)
  .Produces(StatusCodes.Status401Unauthorized)
         .Produces(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Generates SQL from a natural-language analytics question.
    /// 
    /// Development/POC endpoint for manually testing natural-language to SQL generation.
    /// Semantic context is retrieved internally by the Application layer.
    /// Does NOT execute the generated SQL in BigQuery.
    /// </summary>
    /// <param name="request">The request containing the natural-language question.</param>
    /// <param name="useCase">The use case for generating safe analytics SQL.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>HTTP 200 with generated and validated SQL.</returns>
    private static async Task<IResult> GenerateSql(
   GenerateAnalyticsQueryRequest request,
        GenerateAnalyticsQueryUseCase useCase,
   CancellationToken cancellationToken)
    {
        var generatedSql = await useCase.ExecuteAsync(
            request.Question,
            cancellationToken);

   return Results.Ok(new GeneratedSqlResponse { Sql = generatedSql });
    }

  /// <summary>
    /// Retrieves BigQuery metadata for the configured dataset.
    /// 
    /// Development/POC endpoint for manually verifying BigQueryAnalyticsMetadataCatalog
    /// against the real BigQuery environment. Returns table and column metadata including
  /// descriptions where available.
    /// 
    /// Does NOT perform RAG, call Gemini, or execute business-data queries.
    /// </summary>
    /// <param name="currentUserContext">Service to obtain the authenticated current user.</param>
  /// <param name="metadataCatalog">Service to retrieve analytics metadata from BigQuery.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>HTTP 200 with table and column metadata.</returns>
    private static async Task<IResult> GetMetadata(
      ICurrentUserContext currentUserContext,
        IAnalyticsMetadataCatalog metadataCatalog,
        CancellationToken cancellationToken)
  {
        var currentUser = currentUserContext.GetCurrentUser();
        var metadata = await metadataCatalog.GetTablesAsync(currentUser, cancellationToken);
 return Results.Ok(metadata);
    }

  /// <summary>
    /// Executes a natural-language analytics question end-to-end.
    /// 
    /// Development/POC endpoint for runtime verification of the complete flow:
    /// Question ? SQL Generation ? Execution ? Results
  /// 
    /// This endpoint orchestrates:
    /// 1. Question validation
    /// 2. SQL generation from semantic context
    /// 3. Authorization validation
    /// 4. SQL safety validation (defense-in-depth)
    /// 5. Query cost estimation and limit validation
    /// 6. BigQuery execution
    /// 7. Result return
    /// 
    /// All business logic is delegated to ExecuteNaturalLanguageAnalyticsQueryUseCase.
    /// </summary>
    /// <param name="request">The request containing the natural-language question.</param>
    /// <param name="useCase">The use case for the complete end-to-end flow.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>HTTP 200 with generated SQL and execution results.</returns>
    private static async Task<IResult> AskAnalytics(
        GenerateAnalyticsQueryRequest request,
     ExecuteNaturalLanguageAnalyticsQueryUseCase useCase,
     CancellationToken cancellationToken)
    {
  var result = await useCase.ExecuteAsync(request.Question, cancellationToken);
  return Results.Ok(result);
    }

    /// <summary>
    /// Retrieves BigQuery metadata and ranks it by lexical relevance to a question.
    /// 
    /// Development/POC endpoint for independently testing semantic retrieval.
    /// This endpoint demonstrates the retrieval layer in isolation, before integration
    /// with Gemini SQL generation.
    /// 
    /// Does NOT:
    /// - Call Gemini or any AI service
    /// - Generate or execute SQL
    /// - Perform business-data queries against BigQuery
    /// 
    /// Only metadata catalog access is used (via IAnalyticsMetadataCatalog).
    /// </summary>
    /// <param name="request">The request containing the natural-language question.</param>
    /// <param name="currentUserContext">Service to obtain the authenticated current user.</param>
    /// <param name="retriever">The semantic retriever for ranking knowledge items.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>HTTP 200 with array of relevant semantic knowledge items.</returns>
    private static async Task<IResult> RetrieveSemantic(
        GenerateAnalyticsQueryRequest request,
        ICurrentUserContext currentUserContext,
        IAnalyticsSemanticRetriever retriever,
        CancellationToken cancellationToken)
    {
     var currentUser = currentUserContext.GetCurrentUser();
        var result = await retriever.RetrieveAsync(request.Question, currentUser, cancellationToken);
        return Results.Ok(result);
    }

    /// <summary>
  /// Response containing generated SQL.
    /// </summary>
    private sealed class GeneratedSqlResponse
    {
    /// <summary>
 /// The generated and validated SQL query.
        /// </summary>
        public required string Sql { get; init; }
    }
}
