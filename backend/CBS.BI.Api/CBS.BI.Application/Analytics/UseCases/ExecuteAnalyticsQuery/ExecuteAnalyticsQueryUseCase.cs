using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Exceptions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Abstractions;

namespace CBS.BI.Application.Analytics.UseCases.ExecuteAnalyticsQuery;

public class ExecuteAnalyticsQueryUseCase
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAnalyticsAuthorizationService _authorizationService;
    private readonly IAnalyticsQuerySafetyValidator _safetyValidator;
    private readonly IQueryCostEstimator _costEstimator;
    private readonly IAnalyticsQueryExecutor _queryExecutor;

    public ExecuteAnalyticsQueryUseCase(
        ICurrentUserContext currentUserContext,
        IAnalyticsAuthorizationService authorizationService,
        IAnalyticsQuerySafetyValidator safetyValidator,
        IQueryCostEstimator costEstimator,
        IAnalyticsQueryExecutor queryExecutor)
    {
        _currentUserContext = currentUserContext;
        _authorizationService = authorizationService;
        _safetyValidator = safetyValidator;
        _costEstimator = costEstimator;
        _queryExecutor = queryExecutor;
    }

    public async Task<AnalyticsQueryResult> ExecuteAsync(
        string query,
        CancellationToken cancellationToken)
    {
        // Get current user
        var currentUser = _currentUserContext.GetCurrentUser();

        // Ensure user can execute analytics query
        _authorizationService.EnsureCanExecuteQuery(currentUser);

        // Validate query
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query cannot be null, empty, or whitespace.", nameof(query));
        }

        // Ensure query is safe (read-only)
        _safetyValidator.EnsureSafe(query);

        // Estimate query cost
        var estimate = await _costEstimator.EstimateAsync(query, cancellationToken);

        // Check cost limit
        if (!estimate.IsWithinLimit)
        {
            throw new QueryCostLimitExceededException(estimate.EstimatedBytesProcessed);
        }

        // Execute analytics query
        return await _queryExecutor.ExecuteAsync(query, cancellationToken);
    }
}
