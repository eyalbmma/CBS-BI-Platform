using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Infrastructure.Analytics;

/// <summary>
/// Thread-safe in-memory implementation of saved analytics query storage.
///
/// Data persists for the lifetime of the application process.
/// Each user's queries are isolated and cannot be accessed by other users.
///
/// This is a POC implementation. The abstraction allows future replacement with
/// persistent storage (Firestore, Cloud SQL, etc.) without modifying Application layer.
/// </summary>
public sealed class InMemorySavedAnalyticsQueryStore : ISavedAnalyticsQueryStore
{
    private readonly object _lockObject = new object();
    private readonly Dictionary<string, SavedAnalyticsQuery> _queriesById = new();

    public Task<IReadOnlyCollection<SavedAnalyticsQuery>> GetForUserAsync(
 CurrentUser user,
   CancellationToken cancellationToken)
    {
    cancellationToken.ThrowIfCancellationRequested();

     lock (_lockObject)
        {
   var userQueries = _queriesById.Values
  .Where(q => q.CreatedByUserId == user.UserId)
  .OrderByDescending(q => q.CreatedAtUtc)
       .ToList()
                .AsReadOnly();

   return Task.FromResult<IReadOnlyCollection<SavedAnalyticsQuery>>(userQueries);
    }
    }

    public Task<SavedAnalyticsQuery?> GetByIdAsync(
        string id,
   CurrentUser user,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

      if (string.IsNullOrWhiteSpace(id))
      {
            return Task.FromResult<SavedAnalyticsQuery?>(null);
     }

        lock (_lockObject)
    {
     if (_queriesById.TryGetValue(id, out var query))
   {
         if (query.CreatedByUserId == user.UserId)
    {
  return Task.FromResult<SavedAnalyticsQuery?>(query);
    }
        }

            return Task.FromResult<SavedAnalyticsQuery?>(null);
      }
    }

    public Task SaveAsync(
        SavedAnalyticsQuery query,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
     }

        lock (_lockObject)
      {
            _queriesById[query.Id] = query;
        }

 return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(
        string id,
        CurrentUser user,
        CancellationToken cancellationToken)
  {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(id))
        {
      return Task.FromResult(false);
        }

    lock (_lockObject)
   {
        if (_queriesById.TryGetValue(id, out var query))
        {
         if (query.CreatedByUserId == user.UserId)
       {
    _queriesById.Remove(id);
    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
    }
}
