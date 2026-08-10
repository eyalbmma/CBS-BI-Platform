using CBS.BI.Api.Contracts.Analytics;
using CBS.BI.Application.Analytics.UseCases.DeleteSavedAnalyticsQuery;
using CBS.BI.Application.Analytics.UseCases.GetSavedAnalyticsQueries;
using CBS.BI.Application.Analytics.UseCases.SaveAnalyticsQuery;
using CBS.BI.Application.Security.Abstractions;
using CBS.BI.Api.Security;
using Microsoft.AspNetCore.Mvc;

namespace CBS.BI.Api.Controllers;

[ApiController]
[Route("api/analytics/saved-queries")]
public sealed class SavedAnalyticsQueriesController : ControllerBase
{
    private readonly SaveAnalyticsQueryUseCase _saveUseCase;
 private readonly GetSavedAnalyticsQueriesUseCase _getUseCase;
    private readonly DeleteSavedAnalyticsQueryUseCase _deleteUseCase;
    private readonly ICurrentUserContext _userContext;
   private readonly ILogger<SavedAnalyticsQueriesController> _logger;

    public SavedAnalyticsQueriesController(
        SaveAnalyticsQueryUseCase saveUseCase,
  GetSavedAnalyticsQueriesUseCase getUseCase,
        DeleteSavedAnalyticsQueryUseCase deleteUseCase,
      ICurrentUserContext userContext,
   ILogger<SavedAnalyticsQueriesController> logger)
    {
        _saveUseCase = saveUseCase ?? throw new ArgumentNullException(nameof(saveUseCase));
        _getUseCase = getUseCase ?? throw new ArgumentNullException(nameof(getUseCase));
    _deleteUseCase = deleteUseCase ?? throw new ArgumentNullException(nameof(deleteUseCase));
  _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
     _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

  [HttpPost]
  public async Task<IActionResult> SaveQuery(
        SaveAnalyticsQueryRequest request,
   CancellationToken cancellationToken)
    {
        var currentUser = _userContext.GetCurrentUser();

     var savedQuery = await _saveUseCase.ExecuteAsync(
 request.Name,
    request.Question,
     currentUser,
     cancellationToken);

 var response = new SavedAnalyticsQueryResponse(
    Id: savedQuery.Id,
      Name: savedQuery.Name,
       Question: savedQuery.Question,
           CreatedAtUtc: savedQuery.CreatedAtUtc,
         UpdatedAtUtc: savedQuery.UpdatedAtUtc);

        return CreatedAtAction(nameof(SaveQuery), new { id = savedQuery.Id }, response);
  }

   [HttpGet]
    public async Task<IActionResult> GetSavedQueries(
 CancellationToken cancellationToken)
   {
   var currentUser = _userContext.GetCurrentUser();

    var savedQueries = await _getUseCase.ExecuteAsync(currentUser, cancellationToken);

      var responses = savedQueries
            .Select(q => new SavedAnalyticsQueryResponse(
      Id: q.Id,
     Name: q.Name,
   Question: q.Question,
CreatedAtUtc: q.CreatedAtUtc,
    UpdatedAtUtc: q.UpdatedAtUtc))
       .ToList();

        return Ok(responses);
    }

  [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuery(
      string id,
  CancellationToken cancellationToken)
  {
        var currentUser = _userContext.GetCurrentUser();

        var deleted = await _deleteUseCase.ExecuteAsync(id, currentUser, cancellationToken);

      if (!deleted)
       {
           return NotFound();
   }

 return NoContent();
    }
}
