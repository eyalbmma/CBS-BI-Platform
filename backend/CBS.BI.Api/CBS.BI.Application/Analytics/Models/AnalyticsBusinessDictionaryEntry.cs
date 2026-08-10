namespace CBS.BI.Application.Analytics.Models;

/// <summary>
/// Represents a business dictionary entry for analytics domain terminology.
/// 
/// Business dictionary entries define canonical business terms, their synonyms,
/// and their relationships to analytics resources. These entries enrich the semantic
/// corpus with cross-language and cross-terminology knowledge that enables
/// more sophisticated retrieval and context generation.
/// 
/// This model is immutable and technology-neutral, containing only business
/// semantics without implementation-specific concerns.
/// </summary>
public record AnalyticsBusinessDictionaryEntry(
    /// <summary>
/// Stable, unique identifier for this dictionary entry.
    /// 
    /// Format: "business_term:{term_id}"
    /// Example: "business_term:population_count"
    /// </summary>
    string Id,

    /// <summary>
    /// The canonical business term in its original language.
    /// 
    /// Examples:
    /// - "אוכלוסייה" (Hebrew for population)
    /// - "עיר" (Hebrew for city)
    /// </summary>
    string Term,

    /// <summary>
    /// Collection of equivalent terms, synonyms, and common expressions for this business term.
    /// 
    /// Includes:
    /// - Translations to other languages (e.g., English equivalents for Hebrew terms)
    /// - Common synonyms within the domain
    /// - Abbreviations or alternative phrasings
    /// 
    /// Example for אוכלוסייה:
    /// - "population"
    /// - "תושבים" (residents in Hebrew)
    /// - "מספר תושבים" (number of residents)
    /// - "residents"
    /// </summary>
    IReadOnlyCollection<string> Synonyms,

    /// <summary>
    /// Optional business meaning and context for this term.
    /// 
    /// Provides human-readable explanation of what this term represents
    /// in the business domain.
    /// 
    /// Example: "Population / number of residents in a city"
    /// </summary>
    string? Description,

  /// <summary>
    /// Optional logical reference to related analytics metadata.
    /// 
    /// Points to specific analytics resources (tables, columns) that
    /// this business term relates to.
    /// 
    /// Examples:
    /// - "cbs-bi-poc.analytics_demo.city_population.Population"
    /// - "cbs-bi-poc.analytics_demo.city_population.City"
    /// </summary>
    string? SourceReference);
