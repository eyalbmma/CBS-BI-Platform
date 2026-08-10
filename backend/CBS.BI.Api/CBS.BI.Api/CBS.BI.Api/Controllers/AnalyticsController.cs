using CBS.BI.Api.Contracts.Analytics;
using CBS.BI.Application.Analytics.UseCases.ExecuteAnalyticsQuery;
using CBS.BI.Application.Analytics.UseCases.ExecuteNaturalLanguageAnalyticsQuery;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CBS.BI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly ExecuteAnalyticsQueryUseCase _executeAnalyticsQueryUseCase;
    private readonly ExecuteNaturalLanguageAnalyticsQueryUseCase _executeNaturalLanguageAnalyticsQueryUseCase;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        ExecuteAnalyticsQueryUseCase executeAnalyticsQueryUseCase,
        ExecuteNaturalLanguageAnalyticsQueryUseCase executeNaturalLanguageAnalyticsQueryUseCase,
        ILogger<AnalyticsController> logger)
    {
        _executeAnalyticsQueryUseCase = executeAnalyticsQueryUseCase;
        _executeNaturalLanguageAnalyticsQueryUseCase = executeNaturalLanguageAnalyticsQueryUseCase;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("query")]
    [RequestTimeout("AnalyticsExecution")]
    public async Task<IActionResult> ExecuteQuery(
        ExecuteAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _executeAnalyticsQueryUseCase.ExecuteAsync(
            request.Query,
            HttpContext.RequestAborted);

        return Ok(result);
    }

    /// <summary>
    /// Executes a natural-language analytics question end-to-end.
    /// 
    /// Orchestrates:
    /// 1. Question validation
    /// 2. SQL generation from semantic context via Gemini/Vertex AI
    /// 3. Authorization validation
    /// 4. SQL safety validation
    /// 5. Query cost estimation and limit validation
    /// 6. BigQuery execution
    /// 7. Result return
    /// 
    /// All business logic is delegated to ExecuteNaturalLanguageAnalyticsQueryUseCase.
    /// </summary>
    /// <param name="request">The request containing the natural-language question.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Generated SQL and query results.</returns>
    [HttpPost("ask")]
    [RequestTimeout("AnalyticsExecution")]
    public async Task<IActionResult> AskQuestion(
        AskAnalyticsQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await _executeNaturalLanguageAnalyticsQueryUseCase.ExecuteAsync(
                request.Question,
                cancellationToken);

            return Ok(result);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Analytics timing: AskQuestion total duration {DurationMs} ms", stopwatch.ElapsedMilliseconds);
        }
    }
}
