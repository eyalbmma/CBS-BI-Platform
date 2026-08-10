using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;

namespace CBS.BI.Application.Analytics.SemanticKnowledge;

/// <summary>
/// Business dictionary semantic knowledge source for analytics.
/// 
/// Provides semantically enriched knowledge items derived from business dictionary entries.
/// This source bridges the gap between natural language (including non-English languages)
/// and analytics metadata by providing canonical business terms, synonyms, and their
/// relationships to analytics resources.
/// 
/// This implementation:
/// - Maintains a curated in-memory business dictionary
/// - Converts dictionary entries to semantic knowledge items
/// - Enables cross-language retrieval through synonyms
/// - Remains independent of retrieval algorithms
/// 
/// For POC purposes, includes only entries relevant to the city_population dataset.
/// Future versions will load from persistent business glossaries, terminology databases, or
/// knowledge management systems.
/// </summary>
public sealed class BusinessDictionaryAnalyticsSemanticKnowledgeSource : IAnalyticsSemanticKnowledgeSource
{
    private readonly IReadOnlyCollection<AnalyticsBusinessDictionaryEntry> _dictionary;

    /// <summary>
    /// Initializes a new instance of the BusinessDictionaryAnalyticsSemanticKnowledgeSource.
    /// </summary>
    public BusinessDictionaryAnalyticsSemanticKnowledgeSource()
    {
        _dictionary = InitializeBusinessDictionary();
    }

    /// <summary>
    /// Retrieves semantic knowledge items from the business dictionary.
    /// 
    /// All dictionary entries are available to all users (permission filtering
    /// is a responsibility of the knowledge source caller, not the dictionary itself).
    /// </summary>
    public Task<IReadOnlyCollection<AnalyticsSemanticKnowledgeItem>> GetItemsAsync(
      CurrentUser user,
     CancellationToken cancellationToken)
    {
     cancellationToken.ThrowIfCancellationRequested();

        var items = _dictionary
  .Select(ConvertEntryToKnowledgeItem)
            .ToList()
       .AsReadOnly();

        return Task.FromResult<IReadOnlyCollection<AnalyticsSemanticKnowledgeItem>>(items);
    }

    /// <summary>
    /// Converts a business dictionary entry to a semantic knowledge item.
    /// </summary>
    private static AnalyticsSemanticKnowledgeItem ConvertEntryToKnowledgeItem(
        AnalyticsBusinessDictionaryEntry entry)
    {
        var contentLines = new List<string>
        {
   $"Business term: {entry.Term}"
};

        if (entry.Synonyms.Count > 0)
        {
            contentLines.Add($"Synonyms: {string.Join(", ", entry.Synonyms)}");
  }

        if (!string.IsNullOrWhiteSpace(entry.Description))
        {
            contentLines.Add($"Description: {entry.Description}");
     }

   if (!string.IsNullOrWhiteSpace(entry.SourceReference))
        {
 contentLines.Add($"Relevant analytics reference: {entry.SourceReference}");
    }

        var content = string.Join("\n", contentLines);

        return new AnalyticsSemanticKnowledgeItem(
        Id: entry.Id,
            Content: content,
         SourceType: "BusinessDictionary",
            SourceReference: entry.SourceReference);
    }

    /// <summary>
    /// Initializes the POC business dictionary with entries for analytics domains:
    /// - city_population (demographics)
    /// - city_employment_yearly (employment and wages)
    /// - city_housing_yearly (housing and real estate)
    /// - city_education_yearly (education statistics)
    /// 
    /// This dictionary is curated, focused, and intentionally labeled as POC.
    /// Future versions will integrate with persistent business glossaries and terminology systems.
    /// </summary>
    private static IReadOnlyCollection<AnalyticsBusinessDictionaryEntry> InitializeBusinessDictionary()
    {
        return new List<AnalyticsBusinessDictionaryEntry>
        {
  // Demographics domain
 new(
  Id: "business_term:population",
       Term: "אוכלוסייה",
     Synonyms: new[]
              {
          "population",
              "תושבים",
             "מספר תושבים",
         "residents",
            "inhabitants"
           }.AsReadOnly(),
       Description: "Population / number of residents in a city or municipality. Count of inhabitants.",
       SourceReference: "cbs-bi-poc.analytics_demo.city_population.Population"),

     new(
 Id: "business_term:city",
        Term: "עיר",
      Synonyms: new[]
          {
        "city",
         "ערים",
  "יישוב",
   "municipality",
 "town",
    "urban_area",
 "cities"
      }.AsReadOnly(),
       Description: "City or municipality. Common dimension across analytics tables (city_population, city_employment_yearly, city_housing_yearly, city_education_yearly). City name dimension exists in all city-level annual tables. CityCode is the stable city identifier. When a question requests only metrics from a single table, use City from that table; do not join city_population merely to obtain the city name.",
    SourceReference: null),

            // Employment domain
         new(
    Id: "business_term:unemployment",
  Term: "אבטלה",
         Synonyms: new[]
            {
           "unemployment",
              "שיעור אבטלה",
    "מובטלים",
        "unemployed",
        "unemployment rate",
   "jobless"
 }.AsReadOnly(),
      Description: "Unemployment rate as a percentage. Percentage of the labor force that is unemployed.",
SourceReference: "cbs-bi-poc.analytics_demo.city_employment_yearly.UnemploymentRatePct"),

  new(
    Id: "business_term:employed",
  Term: "מועסקים",
   Synonyms: new[]
    {
           "employed",
           "employment",
            "employed persons",
         "מועסק",
            "תעסוקה",
            "workers"
    }.AsReadOnly(),
      Description: "Employed persons. Count of individuals who are employed.",
 SourceReference: "cbs-bi-poc.analytics_demo.city_employment_yearly.EmployedPersons"),

            new(
  Id: "business_term:salary",
             Term: "שכר",
     Synonyms: new[]
       {
        "salary",
        "wage",
        "average salary",
            "שכר ממוצע",
   "משכורת",
         "monthly salary",
      "income"
             }.AsReadOnly(),
         Description: "Average monthly salary in NIS (Israeli New Shekel). Monetary amount representing average monthly income.",
    SourceReference: "cbs-bi-poc.analytics_demo.city_employment_yearly.AverageMonthlySalaryNis"),

 // Housing domain
         new(
     Id: "business_term:housing",
         Term: "דיור",
      Synonyms: new[]
        {
         "housing",
     "real estate",
   "מגורים",
         "נדלן",
 "housing units",
    "residential"
  }.AsReadOnly(),
 Description: "Housing and real estate. Domain covering residential properties and housing metrics.",
             SourceReference: null),

    new(
   Id: "business_term:apartment_price",
         Term: "מחיר דירה",
      Synonyms: new[]
          {
  "apartment price",
        "house price",
            "average apartment price",
         "מחיר דירה ממוצע",
       "מחירי דירות",
          "property price",
        "real estate price"
    }.AsReadOnly(),
          Description: "Average apartment price in NIS. Monetary amount representing average residential property value.",
    SourceReference: "cbs-bi-poc.analytics_demo.city_housing_yearly.AverageApartmentPriceNis"),

            new(
Id: "business_term:rent",
       Term: "שכירות",
 Synonyms: new[]
    {
    "rent",
  "monthly rent",
            "שכר דירה",
     "rental",
     "lease",
      "average rent"
       }.AsReadOnly(),
  Description: "Average monthly rent in NIS. Monetary amount representing average monthly rental cost.",
       SourceReference: "cbs-bi-poc.analytics_demo.city_housing_yearly.AverageMonthlyRentNis"),

            // Education domain
            new(
         Id: "business_term:students",
  Term: "תלמידים",
 Synonyms: new[]
           {
   "students",
     "student count",
   "מספר תלמידים",
      "pupils",
          "learners"
     }.AsReadOnly(),
         Description: "Total students. Count of students enrolled in educational institutions.",
          SourceReference: "cbs-bi-poc.analytics_demo.city_education_yearly.TotalStudents"),

          new(
        Id: "business_term:matriculation",
        Term: "זכאות לבגרות",
         Synonyms: new[]
          {
 "matriculation",
          "matriculation eligibility",
           "בגרות",
          "אחוז זכאות לבגרות",
         "matriculation rate",
         "eligibility for matriculation"
     }.AsReadOnly(),
    Description: "Matriculation eligibility as a percentage. Percentage of students eligible for high school matriculation examination.",
    SourceReference: "cbs-bi-poc.analytics_demo.city_education_yearly.MatriculationEligibilityPct"),

        new(
  Id: "business_term:teachers",
         Term: "מורים",
      Synonyms: new[]
      {
"teachers",
         "teacher count",
        "מספר מורים",
           "educators",
       "teaching staff"
            }.AsReadOnly(),
       Description: "Teachers count. Number of teachers employed in educational institutions.",
    SourceReference: "cbs-bi-poc.analytics_demo.city_education_yearly.TeachersCount"),

            // Time dimension (cross-domain)
      new(
       Id: "business_term:year",
                Term: "שנה",
  Synonyms: new[]
          {
         "year",
          "שנת",
    "לפי שנה",
           "annual",
"yearly"
         }.AsReadOnly(),
                Description: "Year. Common analytics dimension used across all demo tables for temporal aggregation and comparison.",
    SourceReference: null),

            // Business rules
         new(
         Id: "business_rule:city_year_join",
     Term: "כללי הצטרפות טבלאות עיר ושנה",
           Synonyms: new[]
      {
              "city year join",
    "join city tables by year",
 "combining city tables"
     }.AsReadOnly(),
      Description: "The annual city analytics tables (city_population, city_employment_yearly, city_housing_yearly, city_education_yearly) represent city-level yearly statistics. When combining measures across these tables, join them using both CityCode and Year. CityCode identifies the city. Year identifies the reference year. Do not join only by City name when CityCode is available.",
           SourceReference: null)
        }.AsReadOnly();
    }
}
