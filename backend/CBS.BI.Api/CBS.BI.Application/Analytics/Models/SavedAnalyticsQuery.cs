namespace CBS.BI.Application.Analytics.Models;

/// <summary>
/// Represents a saved analytics question that can be reused.
/// 
/// The natural-language Question is the source of truth and durable user intent.
/// SQL is regenerated from the question as needed, allowing the question to remain
/// valid even if metadata, semantic rules, or Gemini prompts evolve.
/// </summary>
public sealed record SavedAnalyticsQuery(
    /// <summary>
    /// Unique identifier for this saved query.
    /// </summary>
    string Id,

    /// <summary>
    /// User-provided name for this saved query.
  /// </summary>
    string Name,

    /// <summary>
    /// The natural-language analytics question.
    /// This is the durable user intent, not the generated SQL.
    /// </summary>
    string Question,

    /// <summary>
    /// User ID of the person who created this saved query.
    /// </summary>
    string CreatedByUserId,

    /// <summary>
    /// When this saved query was created (UTC).
    /// </summary>
    DateTime CreatedAtUtc,

    /// <summary>
    /// When this saved query was last updated (UTC).
    /// </summary>
    DateTime UpdatedAtUtc);
