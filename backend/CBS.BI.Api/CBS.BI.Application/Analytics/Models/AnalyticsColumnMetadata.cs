namespace CBS.BI.Application.Analytics.Models;

/// <summary>
/// Represents metadata for a single analytics column.
/// </summary>
public record AnalyticsColumnMetadata(
    string Name,
    string DataType,
    string? Description);
