using CBS.BI.Api.Contracts.Analytics;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Analytics.UseCases.DeleteAnalyticsDashboard;
using CBS.BI.Application.Analytics.UseCases.GetAnalyticsDashboards;
using CBS.BI.Application.Analytics.UseCases.GetAnalyticsDashboardById;
using CBS.BI.Application.Analytics.UseCases.SaveAnalyticsDashboard;
using CBS.BI.Application.Security.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CBS.BI.Api.Controllers;

[ApiController]
[Route("api/analytics/dashboards")]
public sealed class AnalyticsDashboardsController : ControllerBase
{
    private readonly SaveAnalyticsDashboardUseCase _saveUseCase;
   private readonly GetAnalyticsDashboardsUseCase _getUseCase;
    private readonly GetAnalyticsDashboardByIdUseCase _getByIdUseCase;
    private readonly DeleteAnalyticsDashboardUseCase _deleteUseCase;
private readonly ICurrentUserContext _userContext;
  private readonly ILogger<AnalyticsDashboardsController> _logger;

    public AnalyticsDashboardsController(
       SaveAnalyticsDashboardUseCase saveUseCase,
      GetAnalyticsDashboardsUseCase getUseCase,
        GetAnalyticsDashboardByIdUseCase getByIdUseCase,
 DeleteAnalyticsDashboardUseCase deleteUseCase,
  ICurrentUserContext userContext,
    ILogger<AnalyticsDashboardsController> logger)
    {
        _saveUseCase = saveUseCase ?? throw new ArgumentNullException(nameof(saveUseCase));
     _getUseCase = getUseCase ?? throw new ArgumentNullException(nameof(getUseCase));
 _getByIdUseCase = getByIdUseCase ?? throw new ArgumentNullException(nameof(getByIdUseCase));
     _deleteUseCase = deleteUseCase ?? throw new ArgumentNullException(nameof(deleteUseCase));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
 _logger = logger ?? throw new ArgumentNullException(nameof(logger));
}

    [HttpPost]
    public async Task<IActionResult> SaveDashboard(
  SaveAnalyticsDashboardRequest request,
    CancellationToken cancellationToken)
    {
       var currentUser = _userContext.GetCurrentUser();

      var widgets = request.Widgets
        .Select(w => new AnalyticsDashboardWidget(
      Id: Guid.NewGuid().ToString(),
     Title: w.Title,
   SavedQueryId: w.SavedQueryId,
          VisualizationType: w.VisualizationType,
  DisplayOrder: w.DisplayOrder))
        .ToList();

  var savedDashboard = await _saveUseCase.ExecuteAsync(
            request.Name,
  request.IsDynamic,
     widgets,
   currentUser,
   cancellationToken);

var response = MapToResponse(savedDashboard);

      return CreatedAtAction(nameof(SaveDashboard), new { id = savedDashboard.Id }, response);
    }

 [HttpGet]
    public async Task<IActionResult> GetDashboards(
      CancellationToken cancellationToken)
    {
        var currentUser = _userContext.GetCurrentUser();

 var dashboards = await _getUseCase.ExecuteAsync(currentUser, cancellationToken);

        var responses = dashboards
.Select(MapToResponse)
           .ToList();

    return Ok(responses);
   }

   [HttpGet("{id}")]
   public async Task<IActionResult> GetDashboard(
     string id,
     CancellationToken cancellationToken)
  {
 var currentUser = _userContext.GetCurrentUser();

        var dashboard = await _getByIdUseCase.ExecuteAsync(id, currentUser, cancellationToken);

       if (dashboard == null)
       {
       return NotFound();
     }

        return Ok(MapToResponse(dashboard));
   }

  [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDashboard(
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

  private static AnalyticsDashboardResponse MapToResponse(AnalyticsDashboard dashboard)
    {
      var widgets = dashboard.Widgets
         .Select(w => new DashboardWidgetResponse(
    Id: w.Id,
       Title: w.Title,
    SavedQueryId: w.SavedQueryId,
      VisualizationType: w.VisualizationType,
   DisplayOrder: w.DisplayOrder))
      .ToList()
.AsReadOnly();

   return new AnalyticsDashboardResponse(
        Id: dashboard.Id,
  Name: dashboard.Name,
       IsDynamic: dashboard.IsDynamic,
      Widgets: widgets,
      CreatedAtUtc: dashboard.CreatedAtUtc,
   UpdatedAtUtc: dashboard.UpdatedAtUtc);
    }
}
