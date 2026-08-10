using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Analytics.Abstractions;

/// <summary>
/// Technology-neutral abstraction for persisting analytics dashboards.
///
/// Implementations must enforce user isolation: each user can only access
/// their own dashboards.
/// </summary>
public interface IAnalyticsDashboardStore
{
    /// <summary>
    /// Retrieves all dashboards for the given user.
  /// </summary>
    /// <param name="user">The current authenticated user.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Collection of dashboards belonging to the user, ordered newest first.</returns>
   Task<IReadOnlyCollection<AnalyticsDashboard>> GetForUserAsync(
        CurrentUser user,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a specific dashboard by ID, if it belongs to the given user.
    /// </summary>
   /// <param name="id">The dashboard ID.</param>
    /// <param name="user">The current authenticated user.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The dashboard, or null if not found or not owned by the user.</returns>
    Task<AnalyticsDashboard?> GetByIdAsync(
 string id,
     CurrentUser user,
        CancellationToken cancellationToken);

    /// <summary>
    /// Saves a new or updated dashboard.
    /// </summary>
    /// <param name="dashboard">The dashboard to persist.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
  Task SaveAsync(
      AnalyticsDashboard dashboard,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a dashboard if it belongs to the given user.
    /// </summary>
    /// <param name="id">The dashboard ID to delete.</param>
    /// <param name="user">The current authenticated user.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if deleted, false if not found or not owned by user.</returns>
   Task<bool> DeleteAsync(
      string id,
        CurrentUser user,
        CancellationToken cancellationToken);
}
