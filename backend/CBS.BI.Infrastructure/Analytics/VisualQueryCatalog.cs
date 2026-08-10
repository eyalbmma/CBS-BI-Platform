using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;

namespace CBS.BI.Infrastructure.Analytics;

/// <summary>
/// Allowlisted visual query metric mappings for the POC.
/// 
/// This explicitly registers all valid domain/metric combinations.
/// No client-provided table or column names are accepted.
/// All table references and column names come exclusively from this server-side catalog.
/// 
/// This prevents SQL injection and ensures queries only access intended data.
/// </summary>
public sealed class VisualQueryCatalog : IVisualQueryCatalog
{
    private readonly IReadOnlyCollection<VisualQueryMetricMapping> _mappings;

    public VisualQueryCatalog()
    {
        _mappings = InitializeMappings();
    }

    public IReadOnlyCollection<VisualQueryMetricMapping> GetAllMappings() => _mappings;

    public VisualQueryMetricMapping? FindMapping(string domain, string metric)
    {
        if (string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(metric))
{
            return null;
        }

return _mappings.FirstOrDefault(m =>
    m.Domain.Equals(domain, StringComparison.OrdinalIgnoreCase) &&
      m.Metric.Equals(metric, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyCollection<string> GetMetricsForDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            return Array.Empty<string>().AsReadOnly();
   }

   return _mappings
     .Where(m => m.Domain.Equals(domain, StringComparison.OrdinalIgnoreCase))
      .Select(m => m.Metric)
  .Distinct(StringComparer.OrdinalIgnoreCase)
       .ToList()
  .AsReadOnly();
    }

/// <summary>
    /// Initializes the allowlisted metrics for supported domains.
    /// 
    /// Each entry explicitly maps:
    /// - Domain (business area)
    /// - Metric (measure name)
    /// - BigQuery table reference (project.dataset.table)
    /// - BigQuery column name
    /// 
    /// Additions here expand visual query support. All names are from actual BigQuery schema.
  /// </summary>
    private static IReadOnlyCollection<VisualQueryMetricMapping> InitializeMappings()
  {
        return new List<VisualQueryMetricMapping>
    {
            // Demographics domain
     new("Population", "Population", "cbs-bi-poc.analytics_demo.city_population", "Population"),

         // Employment domain
  new("Employment", "UnemploymentRatePct", "cbs-bi-poc.analytics_demo.city_employment_yearly", "UnemploymentRatePct"),
     new("Employment", "EmployedPersons", "cbs-bi-poc.analytics_demo.city_employment_yearly", "EmployedPersons"),
        new("Employment", "AverageMonthlySalaryNis", "cbs-bi-poc.analytics_demo.city_employment_yearly", "AverageMonthlySalaryNis"),

            // Housing domain
         new("Housing", "AverageApartmentPriceNis", "cbs-bi-poc.analytics_demo.city_housing_yearly", "AverageApartmentPriceNis"),
            new("Housing", "AverageMonthlyRentNis", "cbs-bi-poc.analytics_demo.city_housing_yearly", "AverageMonthlyRentNis"),

  // Education domain
      new("Education", "TotalStudents", "cbs-bi-poc.analytics_demo.city_education_yearly", "TotalStudents"),
     new("Education", "MatriculationEligibilityPct", "cbs-bi-poc.analytics_demo.city_education_yearly", "MatriculationEligibilityPct"),
            new("Education", "TeachersCount", "cbs-bi-poc.analytics_demo.city_education_yearly", "TeachersCount")
   }.AsReadOnly();
    }
}
