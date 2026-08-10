namespace CBS.BI.Application.Analytics.Models;

public class AnalyticsQueryResult
{
    public required IReadOnlyList<string> Columns { get; init; }
    public required IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows { get; init; }
}
