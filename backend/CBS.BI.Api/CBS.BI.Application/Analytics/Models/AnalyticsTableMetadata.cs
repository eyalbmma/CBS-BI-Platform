namespace CBS.BI.Application.Analytics.Models;

/// <summary>
/// Represents metadata for an analytics table.
/// </summary>
public record AnalyticsTableMetadata(
    string ProjectId,
    string DatasetId,
    string TableId,
    string? Description,
    IReadOnlyCollection<AnalyticsColumnMetadata> Columns);
