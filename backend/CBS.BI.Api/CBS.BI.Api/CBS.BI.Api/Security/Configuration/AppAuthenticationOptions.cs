namespace CBS.BI.Api.Security.Configuration;

/// <summary>
/// Configuration options for authentication mode selection.
/// 
/// Supported modes:
/// - Development: Uses X-Dev-* headers for local development (existing DevelopmentAuthenticationHandler)
/// - Demo: Fixed server-controlled demo identity for Cloud Run POC (new DemoAuthenticationHandler)
/// - Production: No fake authentication (intended for future Government Identity Provider integration)
/// </summary>
public sealed class AppAuthenticationOptions
{
    public const string SectionName = "Authentication";

    /// <summary>
    /// Authentication mode.
    /// 
    /// Valid values: Development, Demo, Production
    /// Default: Development (if not configured)
    /// </summary>
    public string Mode { get; init; } = string.Empty;
}
