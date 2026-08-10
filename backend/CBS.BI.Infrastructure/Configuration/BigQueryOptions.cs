namespace CBS.BI.Infrastructure.Configuration;

public sealed class BigQueryOptions
{
    public string DatasetId { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public long MaximumBytesProcessed { get; init; } = 0;
}
