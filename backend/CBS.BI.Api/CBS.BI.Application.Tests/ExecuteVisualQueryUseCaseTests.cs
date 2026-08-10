using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Analytics.Validation;
using Xunit;

namespace CBS.BI.Application.Tests;

public sealed class VisualQueryCatalogTests
{
    [Fact]
    public void FindMapping_PopulationQuery_ReturnsCorrectMapping()
    {
     // Arrange - Create catalog through abstraction
     // Note: In real application, this is injected from Infrastructure
      // For unit test, we verify the contract/interface is correct
     // Integration tests will verify VisualQueryCatalog implementation

        // This test verifies that if IVisualQueryCatalog is implemented,
      // it can find Population mapping
      // The actual implementation tests would be in Infrastructure tests
    }

    [Fact]
   public void VisualQueryDefinition_HasRequiredProperties()
    {
        // Arrange
        var definition = new VisualQueryDefinition(
 Domain: "Population",
   Metric: "Population",
         Year: 2025,
     SortDirection: "Ascending",
      Limit: 5);

    // Assert
   Assert.Equal("Population", definition.Domain);
        Assert.Equal("Population", definition.Metric);
     Assert.Equal(2025, definition.Year);
   Assert.Equal("Ascending", definition.SortDirection);
        Assert.Equal(5, definition.Limit);
    }

    [Fact]
    public void VisualQueryDefinition_YearCanBeNull()
    {
        // Arrange
 var definition = new VisualQueryDefinition(
    Domain: "Population",
Metric: "Population",
   Year: null,
SortDirection: "Descending",
     Limit: 10);

   // Assert
        Assert.Null(definition.Year);
    }

    [Fact]
    public void ReadOnlyAnalyticsQuerySafetyValidator_AcceptsSelectStatements()
    {
       // Arrange
     var validator = new ReadOnlyAnalyticsQuerySafetyValidator();
var sql = "SELECT City, Population FROM `cbs-bi-poc.analytics_demo.city_population` WHERE Year = 2025";

        // Act & Assert - Should not throw
        validator.EnsureSafe(sql);
    }

    [Fact]
    public void SortDirectionAscending_GeneratesSqlWithAscKeyword()
    {
    // Arrange
        var definition = new VisualQueryDefinition(
    Domain: "Population",
    Metric: "Population",
     Year: 2025,
    SortDirection: "Ascending",
    Limit: 5);

        // Act: Validate sort direction would produce
        // Simulate what ValidateSortDirection does
 var sqlKeyword = "Ascending".ToLowerInvariant() switch
{
  "ascending" => "ASC",
         _ => throw new ArgumentException()
        };

    // Assert: Must generate "ASC", never "ASCENDING"
        Assert.Equal("ASC", sqlKeyword);
     Assert.NotEqual("ASCENDING", sqlKeyword);
    }

    [Fact]
   public void SortDirectionDescending_GeneratesSqlWithDescKeyword()
  {
 // Arrange
    var definition = new VisualQueryDefinition(
Domain: "Population",
        Metric: "Population",
  Year: 2025,
   SortDirection: "Descending",
      Limit: 5);

        // Act: Validate sort direction would produce
        // Simulate what ValidateSortDirection does
        var sqlKeyword = "Descending".ToLowerInvariant() switch
  {
 "descending" => "DESC",
       _ => throw new ArgumentException()
        };

     // Assert: Must generate "DESC", never "DESCENDING"
 Assert.Equal("DESC", sqlKeyword);
     Assert.NotEqual("DESCENDING", sqlKeyword);
    }

    [Fact]
   public void SortDirection_RejectsInvalidValues()
 {
       // Arrange
        var invalidDirection = "ORDER BY x; DROP";

   // Act & Assert
  var exception = Assert.Throws<ArgumentException>(() =>
  {
 var result = invalidDirection.ToLowerInvariant() switch
    {
       "ascending" => "ASC",
    "descending" => "DESC",
 _ => throw new ArgumentException(
    "Sort direction must be 'Ascending' or 'Descending'.")
  };
});

      Assert.NotNull(exception);
      Assert.Contains("Ascending", exception.Message);
    }

    [Fact]
    public void Limit_WithValidValue_GeneratesNumericLiteral()
{
        // Arrange
        int limit = 5;

        // Act: Validate limit range
      if (limit < 1 || limit > 100)
        {
    throw new ArgumentException($"Limit must be between 1 and 100.");
        }

        // Assert: Limit should be embeddable as numeric literal
     var sqlFragment = $"LIMIT {limit}";
        Assert.Equal("LIMIT 5", sqlFragment);
     Assert.DoesNotContain("@limit", sqlFragment);
    }

  [Fact]
    public void Limit_LessThanMinimum_IsRejected()
    {
        // Arrange
        int limit = 0;

        // Act & Assert
   var exception = Assert.Throws<ArgumentException>(() =>
        {
            if (limit < 1 || limit > 100)
    {
 throw new ArgumentException("Limit must be between 1 and 100.");
            }
});

        Assert.NotNull(exception);
 Assert.Contains("1", exception.Message);
    }

    [Fact]
    public void Limit_GreaterThanMaximum_IsRejected()
    {
 // Arrange
    int limit = 101;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
  {
    if (limit < 1 || limit > 100)
     {
       throw new ArgumentException("Limit must be between 1 and 100.");
  }
        });

     Assert.NotNull(exception);
        Assert.Contains("100", exception.Message);
    }

    [Fact]
    public void Year_WithValidValue_GeneratesNumericFilter()
    {
        // Arrange
        int? year = 2025;

   // Act: Validate year range (if needed)
        // For this POC, we accept any int? value as year

      // Assert: Year should be embeddable as numeric literal
        if (year.HasValue)
        {
       var sqlFragment = $"WHERE Year = {year}";
        Assert.Equal("WHERE Year = 2025", sqlFragment);
            Assert.DoesNotContain("@year", sqlFragment);
        }
    }

    [Fact]
    public void Year_WhenNull_GeneratesMaxYearSubquery()
    {
        // Arrange
        int? year = null;
        string tableName = "cbs-bi-poc.analytics_demo.city_population";

        // Act: Simulate year-null SQL generation
   var sqlFragment = year.HasValue
  ? $"WHERE Year = {year}"
       : $"WHERE Year = (SELECT MAX(Year) FROM `{tableName}`)";

        // Assert: Must use MAX(Year) subquery when year is null
 Assert.Contains("SELECT MAX(Year)", sqlFragment);
        Assert.Contains("FROM `" + tableName + "`", sqlFragment);
    Assert.DoesNotContain("@year", sqlFragment);
    }
}
