using CBS.BI.Api.Contracts.Analytics;
using CBS.BI.Application.Analytics.UseCases.ExecuteVisualQuery;
using CBS.BI.Application.Security.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CBS.BI.Api.Controllers;

[ApiController]
[Route("api/analytics/visual-query")]
public sealed class VisualQueryController : ControllerBase
{
    private readonly ExecuteVisualQueryUseCase _useCase;
    private readonly ICurrentUserContext _userContext;
    private readonly ILogger<VisualQueryController> _logger;

    public VisualQueryController(
        ExecuteVisualQueryUseCase useCase,
        ICurrentUserContext userContext,
        ILogger<VisualQueryController> logger)
    {
        _useCase = useCase ?? throw new ArgumentNullException(nameof(useCase));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Execute a visual query (structured query generation without SQL).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(VisualQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ExecuteVisualQuery(
        Contracts.Analytics.VisualQueryRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return BadRequest("Request cannot be null");
        }

        var currentUser = _userContext.GetCurrentUser();

        try
        {
            // Map API contract to application model
            var queryDefinition = new global::CBS.BI.Application.Analytics.Models.VisualQueryDefinition(
                Domain: request.Domain,
                Metric: request.Metric,
                Year: request.Year,
                SortDirection: request.SortDirection,
                Limit: request.Limit);

            var (sql, result) = await _useCase.ExecuteAsync(
                queryDefinition,
                currentUser,
                cancellationToken);

            var response = new VisualQueryResponse(Sql: sql, Result: result);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Type = "visual-query-validation-error",
                Title = "Visual query validation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Type = "visual-query-execution-error",
                Title = "Visual query execution failed",
                Status = StatusCodes.Status422UnprocessableEntity,
                Detail = ex.Message
            });
        }
    }
}
