namespace CBS.BI.Infrastructure.Configuration;

public sealed class GoogleCloudOptions
{
    public const string SectionName = "GoogleCloud";

    public string ProjectId { get; init; } = string.Empty;
    public BigQueryOptions BigQuery { get; init; } = new();
    public GeminiOptions Gemini { get; init; } = new();
}
