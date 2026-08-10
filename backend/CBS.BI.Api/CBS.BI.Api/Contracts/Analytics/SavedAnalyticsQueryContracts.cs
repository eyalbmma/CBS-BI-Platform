namespace CBS.BI.Api.Contracts.Analytics;

public sealed record SaveAnalyticsQueryRequest(
    string Name,
 string Question);

public sealed record SavedAnalyticsQueryResponse(
    string Id,
    string Name,
 string Question,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
