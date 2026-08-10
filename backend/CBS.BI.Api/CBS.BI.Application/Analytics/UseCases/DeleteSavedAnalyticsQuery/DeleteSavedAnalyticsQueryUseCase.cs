using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Analytics.UseCases.DeleteSavedAnalyticsQuery;

/// <summary>
/// Use case for deleting a saved analytics query.
///
/// Only the query owner can delete their own saved queries.
/// </summary>
public sealed class DeleteSavedAnalyticsQueryUseCase
{
    private readonly ISavedAnalyticsQueryStore _store;

    public DeleteSavedAnalyticsQueryUseCase(ISavedAnalyticsQueryStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

   public async Task<bool> ExecuteAsync(
  string queryId,
        CurrentUser user,
   CancellationToken cancellationToken)
  {
        if (string.IsNullOrWhiteSpace(queryId))
      {
        throw new ArgumentException("Query ID must not be empty.", nameof(queryId));
  }

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return await _store.DeleteAsync(queryId, user, cancellationToken);
    }
}
