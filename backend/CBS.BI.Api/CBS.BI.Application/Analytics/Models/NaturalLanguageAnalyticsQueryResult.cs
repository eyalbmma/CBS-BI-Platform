namespace CBS.BI.Application.Analytics.Models;

/// <summary>
/// Represents the result of executing a natural-language analytics query.
/// 
/// This model combines:
/// 1. The generated SQL (from GenerateAnalyticsQueryUseCase)
/// 2. The execution result (from ExecuteAnalyticsQueryUseCase)
/// 
/// This allows callers to access both the SQL that was generated and executed,
/// as well as the data results returned by BigQuery.
/// </summary>
public record NaturalLanguageAnalyticsQueryResult(
    string Sql,
    AnalyticsQueryResult Result);
