namespace CBS.BI.Infrastructure.Configuration;

/// <summary>
/// Configuration options for Gemini / Vertex AI integration.
/// 
/// This options class is provider-independent and configures only the essential
/// parameters needed for query generation: the model name and location.
/// 
/// Authentication uses Application Default Credentials (ADC), kept external to
/// the application configuration and code.
/// </summary>
public sealed class GeminiOptions
{
    /// <summary>
    /// The Gemini model to use for query generation.
    /// Example: "gemini-3-flash-preview"
    /// 
    /// This must remain configuration-driven to allow model selection
    /// without code changes.
    /// </summary>
    public string Model { get; init; } = string.Empty;

    /// <summary>
    /// The Vertex AI location where the Gemini model is available.
/// Example: "global", "us-central1", "europe-west4"
    /// 
    /// Different models and organizations may require different regions.
    /// </summary>
  public string Location { get; init; } = string.Empty;
}
