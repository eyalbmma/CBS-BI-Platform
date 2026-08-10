using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Analytics.Abstractions;

/// <summary>
/// Represents the capability of retrieving analytics metadata.
/// 
/// This abstraction is responsible for returning structured metadata
/// for analytics resources that the current user is allowed to access.
/// 
/// The metadata catalog does not perform question-based filtering or RAG.
/// Its responsibility is only to provide authorized analytics metadata.
/// Question-based retrieval and semantic context enhancement will be
/// separate concerns handled by other components.
/// </summary>
public interface IAnalyticsMetadataCatalog
{
    /// <summary>
    /// Retrieves analytics table metadata that the user is authorized to access.
 /// </summary>
    /// <param name="user">The current authenticated user.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only collection of authorized analytics table metadata.</returns>
    Task<IReadOnlyCollection<AnalyticsTableMetadata>> GetTablesAsync(
     CurrentUser user,
        CancellationToken cancellationToken);
}
