using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Analytics.Abstractions;

/// <summary>
/// Technology-neutral abstraction for persisting saved analytics queries.
/// 
/// Implementations must enforce user isolation: each user can only access
/// their own saved queries.
/// </summary>
public interface ISavedAnalyticsQueryStore
{
    /// <summary>
    /// Retrieves all saved queries for the given user.
    /// </summary>
    /// <param name="user">The current authenticated user.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Collection of saved queries belonging to the user, ordered newest first.</returns>
    Task<IReadOnlyCollection<SavedAnalyticsQuery>> GetForUserAsync(
        CurrentUser user,
        CancellationToken cancellationToken);

  /// <summary>
    /// Retrieves a specific saved query by ID, if it belongs to the given user.
    /// </summary>
    /// <param name="id">The saved query ID.</param>
    /// <param name="user">The current authenticated user.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The saved query, or null if not found or not owned by the user.</returns>
    Task<SavedAnalyticsQuery?> GetByIdAsync(
   string id,
        CurrentUser user,
        CancellationToken cancellationToken);

    /// <summary>
    /// Saves a new or updated analytics query.
    /// </summary>
    /// <param name="query">The saved query to persist.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task SaveAsync(
 SavedAnalyticsQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a saved query if it belongs to the given user.
    /// </summary>
  /// <param name="id">The saved query ID to delete.</param>
    /// <param name="user">The current authenticated user.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if deleted, false if not found or not owned by user.</returns>
    Task<bool> DeleteAsync(
        string id,
  CurrentUser user,
        CancellationToken cancellationToken);
}
