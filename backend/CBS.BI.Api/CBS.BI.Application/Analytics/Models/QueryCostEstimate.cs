namespace CBS.BI.Application.Analytics.Models;

public sealed class QueryCostEstimate
{
    public required long EstimatedBytesProcessed { get; init; }
    public required bool IsWithinLimit { get; init; }
}
