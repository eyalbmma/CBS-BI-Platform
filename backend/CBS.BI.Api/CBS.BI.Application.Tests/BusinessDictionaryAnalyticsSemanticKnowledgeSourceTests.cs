using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Analytics.SemanticKnowledge;
using CBS.BI.Application.Security.Models;
using Xunit;

namespace CBS.BI.Application.Tests;

public class BusinessDictionaryAnalyticsSemanticKnowledgeSourceTests
{
    [Fact]
    public async Task GetItemsAsync_ProducesOneSemanticItemPerDictionaryEntry()
    {
        // Arrange
        var source = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
      var items = await source.GetItemsAsync(user, CancellationToken.None);

      // Assert
        // 2 (city, population) + 3 (employment) + 3 (housing) + 3 (education) + 1 (year) + 1 (join rule) = 13
Assert.Equal(13, items.Count);
    }

  [Fact]
    public async Task GetItemsAsync_ContainsExistingPopulationEntry()
    {
      // Arrange
        var source = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
    var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

     // Act
   var items = await source.GetItemsAsync(user, CancellationToken.None);

   // Assert
        var populationItem = items.FirstOrDefault(i => i.Id == "business_term:population");
 Assert.NotNull(populationItem);
        Assert.Contains("אוכלוסייה", populationItem.Content);
  }

    [Fact]
    public async Task GetItemsAsync_ContainsExistingCityEntry()
    {
        // Arrange
        var source = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

   // Act
     var items = await source.GetItemsAsync(user, CancellationToken.None);

        // Assert
        var cityItem = items.FirstOrDefault(i => i.Id == "business_term:city");
        Assert.NotNull(cityItem);
     Assert.Contains("עיר", cityItem.Content);
      Assert.Null(cityItem.SourceReference);
    }

    [Fact]
    public async Task GetItemsAsync_CityEntryIncludesPluralVariants()
    {
        // Arrange
        var source = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
        var items = await source.GetItemsAsync(user, CancellationToken.None);

        // Assert
        var cityItem = items.FirstOrDefault(i => i.Id == "business_term:city");
     Assert.NotNull(cityItem);
        // Verify singular Hebrew form
        Assert.Contains("עיר", cityItem.Content);
        // Verify plural Hebrew form (new addition)
        Assert.Contains("ערים", cityItem.Content);
  // Verify English singular
        Assert.Contains("city", cityItem.Content);
  // Verify English plural (new addition)
      Assert.Contains("cities", cityItem.Content);
// Verify other existing synonyms are preserved
      Assert.Contains("municipality", cityItem.Content);
  Assert.Contains("town", cityItem.Content);
        Assert.Contains("יישוב", cityItem.Content);
    }

    [Fact]
    public async Task GetItemsAsync_ContainsEmploymentTerms()
    {
// Arrange
        var source = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
     var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
        var items = await source.GetItemsAsync(user, CancellationToken.None);

     // Assert
        var unemploymentItem = items.FirstOrDefault(i => i.Id == "business_term:unemployment");
        Assert.NotNull(unemploymentItem);
        Assert.Contains("אבטלה", unemploymentItem.Content);
   Assert.Contains("unemployment", unemploymentItem.Content);
        Assert.Equal("cbs-bi-poc.analytics_demo.city_employment_yearly.UnemploymentRatePct", unemploymentItem.SourceReference);

      var employedItem = items.FirstOrDefault(i => i.Id == "business_term:employed");
        Assert.NotNull(employedItem);
      Assert.Contains("מועסקים", employedItem.Content);
  Assert.Contains("employed", employedItem.Content);
        Assert.Equal("cbs-bi-poc.analytics_demo.city_employment_yearly.EmployedPersons", employedItem.SourceReference);

        var salaryItem = items.FirstOrDefault(i => i.Id == "business_term:salary");
        Assert.NotNull(salaryItem);
        Assert.Contains("שכר", salaryItem.Content);
        Assert.Contains("salary", salaryItem.Content);
      Assert.Equal("cbs-bi-poc.analytics_demo.city_employment_yearly.AverageMonthlySalaryNis", salaryItem.SourceReference);
    }

    [Fact]
    public async Task GetItemsAsync_ContainsHousingTerms()
    {
        // Arrange
        var source = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
      var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
var items = await source.GetItemsAsync(user, CancellationToken.None);

        // Assert
    var housingItem = items.FirstOrDefault(i => i.Id == "business_term:housing");
        Assert.NotNull(housingItem);
        Assert.Contains("דיור", housingItem.Content);
      Assert.Contains("housing", housingItem.Content);
      Assert.Null(housingItem.SourceReference);

  var priceItem = items.FirstOrDefault(i => i.Id == "business_term:apartment_price");
        Assert.NotNull(priceItem);
        Assert.Contains("מחיר דירה", priceItem.Content);
        Assert.Contains("apartment price", priceItem.Content);
        Assert.Equal("cbs-bi-poc.analytics_demo.city_housing_yearly.AverageApartmentPriceNis", priceItem.SourceReference);

        var rentItem = items.FirstOrDefault(i => i.Id == "business_term:rent");
        Assert.NotNull(rentItem);
Assert.Contains("שכירות", rentItem.Content);
        Assert.Contains("rent", rentItem.Content);
        Assert.Equal("cbs-bi-poc.analytics_demo.city_housing_yearly.AverageMonthlyRentNis", rentItem.SourceReference);
    }

    [Fact]
    public async Task GetItemsAsync_ContainsEducationTerms()
  {
        // Arrange
        var source = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
  var items = await source.GetItemsAsync(user, CancellationToken.None);

        // Assert
var studentsItem = items.FirstOrDefault(i => i.Id == "business_term:students");
        Assert.NotNull(studentsItem);
        Assert.Contains("תלמידים", studentsItem.Content);
        Assert.Contains("students", studentsItem.Content);
        Assert.Equal("cbs-bi-poc.analytics_demo.city_education_yearly.TotalStudents", studentsItem.SourceReference);

        var matriculationItem = items.FirstOrDefault(i => i.Id == "business_term:matriculation");
        Assert.NotNull(matriculationItem);
  Assert.Contains("זכאות לבגרות", matriculationItem.Content);
        Assert.Contains("matriculation", matriculationItem.Content);
        Assert.Equal("cbs-bi-poc.analytics_demo.city_education_yearly.MatriculationEligibilityPct", matriculationItem.SourceReference);

      var teachersItem = items.FirstOrDefault(i => i.Id == "business_term:teachers");
        Assert.NotNull(teachersItem);
        Assert.Contains("מורים", teachersItem.Content);
     Assert.Contains("teachers", teachersItem.Content);
        Assert.Equal("cbs-bi-poc.analytics_demo.city_education_yearly.TeachersCount", teachersItem.SourceReference);
    }

    [Fact]
    public async Task GetItemsAsync_ContainsTimeDimensionTerm()
    {
        // Arrange
        var source = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
   var items = await source.GetItemsAsync(user, CancellationToken.None);

        // Assert
        var yearItem = items.FirstOrDefault(i => i.Id == "business_term:year");
  Assert.NotNull(yearItem);
  Assert.Contains("שנה", yearItem.Content);
        Assert.Contains("year", yearItem.Content);
        Assert.Null(yearItem.SourceReference);
        Assert.Contains("cross", yearItem.Content.ToLower());
    }

    [Fact]
    public async Task GetItemsAsync_ContainsJoinBusinessRule()
    {
        // Arrange
        var source = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
var items = await source.GetItemsAsync(user, CancellationToken.None);

        // Assert
        var joinRule = items.FirstOrDefault(i => i.Id == "business_rule:city_year_join");
        Assert.NotNull(joinRule);
      Assert.Equal("BusinessDictionary", joinRule.SourceType);
        Assert.Contains("CityCode", joinRule.Content);
        Assert.Contains("Year", joinRule.Content);
 Assert.Contains("city_population", joinRule.Content);
        Assert.Contains("city_employment_yearly", joinRule.Content);
        Assert.Contains("city_housing_yearly", joinRule.Content);
        Assert.Contains("city_education_yearly", joinRule.Content);
      Assert.Null(joinRule.SourceReference);
    }

    [Fact]
    public async Task GetItemsAsync_SemanticItemHasBusinessDictionarySourceType()
    {
   // Arrange
        var source = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
   var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

     // Act
var items = await source.GetItemsAsync(user, CancellationToken.None);

        // Assert
        Assert.All(items, item => Assert.Equal("BusinessDictionary", item.SourceType));
    }

    [Fact]
    public async Task GetItemsAsync_SemanticItemsHaveStableIds()
    {
     // Arrange
        var source1 = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
        var source2 = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

  // Act
    var items1 = await source1.GetItemsAsync(user, CancellationToken.None);
        var items2 = await source2.GetItemsAsync(user, CancellationToken.None);

        // Assert
     var ids1 = items1.Select(i => i.Id).OrderBy(id => id).ToList();
 var ids2 = items2.Select(i => i.Id).OrderBy(id => id).ToList();
  Assert.Equal(ids1, ids2);
    }

    [Fact]
    public async Task GetItemsAsync_ReturnsDeterministicOrdering()
    {
   // Arrange
        var source = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
  var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

      // Act
    var items1 = await source.GetItemsAsync(user, CancellationToken.None);
        var items2 = await source.GetItemsAsync(user, CancellationToken.None);

   // Assert
  var ids1 = items1.Select(i => i.Id).ToList();
        var ids2 = items2.Select(i => i.Id).ToList();
        Assert.Equal(ids1, ids2);
    }

    [Fact]
    public async Task GetItemsAsync_AllItemsHaveContent()
    {
        // Arrange
  var source = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
    var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
        var items = await source.GetItemsAsync(user, CancellationToken.None);

    // Assert
        Assert.All(items, item =>
        {
  Assert.NotNull(item.Content);
     Assert.NotEmpty(item.Content);
        });
    }

    [Fact]
    public async Task GetItemsAsync_RespectsCancellationToken()
    {
        // Arrange
var source = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

     using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
await Assert.ThrowsAsync<OperationCanceledException>(
   () => source.GetItemsAsync(user, cts.Token));
    }

    [Fact]
    public async Task GetItemsAsync_ReturnsReadOnlyCollection()
    {
      // Arrange
      var source = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
var items = await source.GetItemsAsync(user, CancellationToken.None);

        // Assert
    Assert.IsAssignableFrom<IReadOnlyCollection<AnalyticsSemanticKnowledgeItem>>(items);
    }

    [Fact]
    public async Task GetItemsAsync_CityItemIsNowDomainNeutral()
    {
   // Arrange
     var source = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

    // Act
        var items = await source.GetItemsAsync(user, CancellationToken.None);

        // Assert
        var cityItem = items.FirstOrDefault(i => i.Id == "business_term:city");
        Assert.NotNull(cityItem);
        Assert.Contains("עיר", cityItem.Content);
        Assert.Contains("city", cityItem.Content);
      // City now has null SourceReference because it's shared across tables
        Assert.Null(cityItem.SourceReference);
        // Description should mention it's a common dimension
 Assert.Contains("Common dimension", cityItem.Content);
        Assert.Contains("city_population", cityItem.Content);
        Assert.Contains("city_employment_yearly", cityItem.Content);
        Assert.Contains("city_housing_yearly", cityItem.Content);
        Assert.Contains("city_education_yearly", cityItem.Content);
    }

    [Fact]
    public async Task GetItemsAsync_PopulationEntryRemainsDomainSpecific()
    {
    // Arrange
        var source = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
  var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
    var items = await source.GetItemsAsync(user, CancellationToken.None);

        // Assert
        var populationItem = items.FirstOrDefault(i => i.Id == "business_term:population");
        Assert.NotNull(populationItem);
        // Population remains pointed to specific column
        Assert.Equal("cbs-bi-poc.analytics_demo.city_population.Population", populationItem.SourceReference);
    }
}
