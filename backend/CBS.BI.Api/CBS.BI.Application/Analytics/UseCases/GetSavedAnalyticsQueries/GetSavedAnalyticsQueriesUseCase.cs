using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Analytics.UseCases.GetSavedAnalyticsQueries;

/// <summary>
/// Use case for retrieving all saved analytics queries for the current user.
///
/// Returns queries ordered newest first.
/// </summary>
public sealed class GetSavedAnalyticsQueriesUseCase
{
    private readonly ISavedAnalyticsQueryStore _store;

    public GetSavedAnalyticsQueriesUseCase(ISavedAnalyticsQueryStore store)
    {
 _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public async Task<IReadOnlyCollection<SavedAnalyticsQuery>> ExecuteAsync(
        CurrentUser user,
        CancellationToken cancellationToken)
    {
        if (user == null)
        {
throw new ArgumentNullException(nameof(user));
        }

  return await _store.GetForUserAsync(user, cancellationToken);
    }
}
