namespace CBS.BI.Application.Analytics.Models;

/// <summary>
/// Represents a single retrievable semantic knowledge item for analytics query generation.
/// 
/// Semantic knowledge items are the fundamental units that may be selected by a semantic
/// retriever/RAG system to inform AI-based SQL generation. They represent:
/// - Table and column metadata
/// - Business term definitions
/// - Business rules
/// - Data dictionary entries
/// - Domain-specific semantic information
/// 
/// This model is technology-neutral and does not depend on any specific retrieval mechanism,
/// vector database, or embedding implementation. It allows the Application layer to remain
/// decoupled from such infrastructure concerns.
/// 
/// Future semantic retrieval systems (including RAG/vector search) will work with collections
/// of these items, selecting and ranking them based on relevance to a user's question.
/// </summary>
public record AnalyticsSemanticKnowledgeItem(
    /// <summary>
    /// Stable, unique identifier for this semantic knowledge item.
    /// 
    /// Examples:
    /// - "table:cbs-bi-poc.analytics_demo.city_population"
    /// - "column:cbs-bi-poc.analytics_demo.city_population.Population"
/// - "business_term:population_count"
    /// 
  /// The format is left to the implementation to define, but should be consistent
    /// and suitable for tracking and auditing which semantic items informed query generation.
    /// </summary>
    string Id,

    /// <summary>
    /// The semantic/business text content of this knowledge item.
    /// 
    /// This is the textual representation that will be supplied to the SQL generation model
    /// (e.g., Gemini/Vertex AI). It should be human-readable and clearly communicate the
    /// semantic meaning or business context of the underlying data or concept.
    /// 
    /// Examples:
    /// - "Table: city_population - Contains city names and their populations in thousands"
    /// - "Column: Population - Integer representing city population count"
    /// - "Business Rule: Population figures are updated quarterly"
    /// </summary>
    string Content,

    /// <summary>
    /// Identifies the category or type of semantic knowledge this item represents.
    /// 
    /// Common types include:
    /// - "TableMetadata"
    /// - "ColumnMetadata"
    /// - "BusinessTerm"
    /// - "BusinessRule"
 /// - "DictionaryEntry"
    /// 
  /// The specific values are determined by the semantic retriever implementation.
    /// This field allows semantic context providers to handle different item types
    /// differently if needed (e.g., formatting, prioritization).
    /// </summary>
    string SourceType,

/// <summary>
    /// Optional logical reference to the source of this semantic knowledge.
    /// 
  /// This may point to:
    /// - A BigQuery resource: "cbs-bi-poc.analytics_demo.city_population" or "cbs-bi-poc.analytics_demo.city_population.Population"
    /// - A business glossary entry: "glossary:population"
    /// - A rule definition: "rule:quarterly_update_policy"
    /// - Any other persistent identifier managed by the semantic knowledge system
    /// 
    /// This reference enables tracking and auditing of which source data informed
  /// the selection of this item by a retriever.
    /// </summary>
    string? SourceReference);
