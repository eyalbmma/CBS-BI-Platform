using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Analytics.SemanticKnowledge;
using CBS.BI.Application.Security.Models;
using Xunit;

namespace CBS.BI.Application.Tests;

public class MetadataAnalyticsSemanticKnowledgeSourceTests
{
    private sealed class FakeAnalyticsMetadataCatalog : IAnalyticsMetadataCatalog
    {
        private IReadOnlyCollection<AnalyticsTableMetadata> _metadataToReturn = Array.Empty<AnalyticsTableMetadata>();
        private bool _shouldThrow = false;
        private Exception _exceptionToThrow = null!;

 public bool WasCalled { get; private set; }
      public CurrentUser? ReceivedUser { get; private set; }
      public CancellationToken ReceivedCancellationToken { get; private set; }

        public void SetMetadataToReturn(IReadOnlyCollection<AnalyticsTableMetadata> metadata)
        {
            _metadataToReturn = metadata;
    _shouldThrow = false;
        }

  public void SetExceptionToThrow(Exception exception)
   {
         _shouldThrow = true;
     _exceptionToThrow = exception;
        }

        public Task<IReadOnlyCollection<AnalyticsTableMetadata>> GetTablesAsync(
            CurrentUser user,
  CancellationToken cancellationToken)
        {
   WasCalled = true;
            ReceivedUser = user;
        ReceivedCancellationToken = cancellationToken;

          if (_shouldThrow)
   {
     throw _exceptionToThrow;
     }

     return Task.FromResult(_metadataToReturn);
        }
    }

    [Fact]
    public async Task GetItemsAsync_WithOneTableAndTwoColumns_ProducesThreeKnowledgeItems()
    {
      // Arrange
        var fakeMetadataCatalog = new FakeAnalyticsMetadataCatalog();
        var knowledgeSource = new MetadataAnalyticsSemanticKnowledgeSource(fakeMetadataCatalog);

  var columns = new List<AnalyticsColumnMetadata>
        {
       new("City", "STRING", "city name"),
  new("Population", "INT64", "population count")
        }.AsReadOnly();

     var table = new AnalyticsTableMetadata(
  ProjectId: "cbs-bi-poc",
   DatasetId: "analytics_demo",
  TableId: "city_population",
     Description: "City population data",
     Columns: columns);

     fakeMetadataCatalog.SetMetadataToReturn(new[] { table }.AsReadOnly());
  var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
        var items = await knowledgeSource.GetItemsAsync(user, CancellationToken.None);

   // Assert
        Assert.Equal(3, items.Count);
    Assert.True(fakeMetadataCatalog.WasCalled);
        Assert.Same(user, fakeMetadataCatalog.ReceivedUser);
    }

    [Fact]
    public async Task GetItemsAsync_GeneratesStableTableIdAndSourceReference()
    {
 // Arrange
        var fakeMetadataCatalog = new FakeAnalyticsMetadataCatalog();
        var knowledgeSource = new MetadataAnalyticsSemanticKnowledgeSource(fakeMetadataCatalog);

        var table = new AnalyticsTableMetadata(
            ProjectId: "cbs-bi-poc",
      DatasetId: "analytics_demo",
            TableId: "city_population",
   Description: null,
    Columns: Array.Empty<AnalyticsColumnMetadata>().AsReadOnly());

        fakeMetadataCatalog.SetMetadataToReturn(new[] { table }.AsReadOnly());
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
        var items = await knowledgeSource.GetItemsAsync(user, CancellationToken.None);

        // Assert
    var tableItem = items.First();
 Assert.Equal("table:cbs-bi-poc.analytics_demo.city_population", tableItem.Id);
     Assert.Equal("cbs-bi-poc.analytics_demo.city_population", tableItem.SourceReference);
        Assert.Equal("TableMetadata", tableItem.SourceType);
    }

    [Fact]
    public async Task GetItemsAsync_GeneratesStableColumnIdAndSourceReference()
    {
        // Arrange
        var fakeMetadataCatalog = new FakeAnalyticsMetadataCatalog();
    var knowledgeSource = new MetadataAnalyticsSemanticKnowledgeSource(fakeMetadataCatalog);

        var columns = new List<AnalyticsColumnMetadata>
   {
            new("Population", "INT64", null)
  }.AsReadOnly();

 var table = new AnalyticsTableMetadata(
    ProjectId: "cbs-bi-poc",
        DatasetId: "analytics_demo",
            TableId: "city_population",
          Description: null,
       Columns: columns);

        fakeMetadataCatalog.SetMetadataToReturn(new[] { table }.AsReadOnly());
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
        var items = await knowledgeSource.GetItemsAsync(user, CancellationToken.None);

        // Assert
      var columnItem = items.Last();
    Assert.Equal("column:cbs-bi-poc.analytics_demo.city_population.Population", columnItem.Id);
        Assert.Equal("cbs-bi-poc.analytics_demo.city_population.Population", columnItem.SourceReference);
        Assert.Equal("ColumnMetadata", columnItem.SourceType);
    }

    [Fact]
    public async Task GetItemsAsync_IncludesTableDescriptionInContent_WhenPresent()
    {
        // Arrange
  var fakeMetadataCatalog = new FakeAnalyticsMetadataCatalog();
      var knowledgeSource = new MetadataAnalyticsSemanticKnowledgeSource(fakeMetadataCatalog);

        var table = new AnalyticsTableMetadata(
        ProjectId: "cbs-bi-poc",
      DatasetId: "analytics_demo",
   TableId: "city_population",
     Description: "Contains city population statistics",
      Columns: Array.Empty<AnalyticsColumnMetadata>().AsReadOnly());

     fakeMetadataCatalog.SetMetadataToReturn(new[] { table }.AsReadOnly());
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
        var items = await knowledgeSource.GetItemsAsync(user, CancellationToken.None);

   // Assert
      var tableItem = items.First();
        Assert.Contains("Description: Contains city population statistics", tableItem.Content);
    }

    [Fact]
  public async Task GetItemsAsync_OmitsTableDescriptionFromContent_WhenNull()
    {
        // Arrange
        var fakeMetadataCatalog = new FakeAnalyticsMetadataCatalog();
        var knowledgeSource = new MetadataAnalyticsSemanticKnowledgeSource(fakeMetadataCatalog);

        var table = new AnalyticsTableMetadata(
  ProjectId: "cbs-bi-poc",
            DatasetId: "analytics_demo",
 TableId: "city_population",
          Description: null,
       Columns: Array.Empty<AnalyticsColumnMetadata>().AsReadOnly());

     fakeMetadataCatalog.SetMetadataToReturn(new[] { table }.AsReadOnly());
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
   var items = await knowledgeSource.GetItemsAsync(user, CancellationToken.None);

  // Assert
        var tableItem = items.First();
        Assert.DoesNotContain("Description:", tableItem.Content);
    }

    [Fact]
    public async Task GetItemsAsync_IncludesColumnDescriptionInContent_WhenPresent()
    {
  // Arrange
    var fakeMetadataCatalog = new FakeAnalyticsMetadataCatalog();
      var knowledgeSource = new MetadataAnalyticsSemanticKnowledgeSource(fakeMetadataCatalog);

        var columns = new List<AnalyticsColumnMetadata>
        {
  new("Population", "INT64", "city population in thousands")
   }.AsReadOnly();

var table = new AnalyticsTableMetadata(
            ProjectId: "cbs-bi-poc",
    DatasetId: "analytics_demo",
  TableId: "city_population",
 Description: null,
        Columns: columns);

        fakeMetadataCatalog.SetMetadataToReturn(new[] { table }.AsReadOnly());
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
var items = await knowledgeSource.GetItemsAsync(user, CancellationToken.None);

        // Assert
var columnItem = items.Last();
        Assert.Contains("Description: city population in thousands", columnItem.Content);
    }

    [Fact]
    public async Task GetItemsAsync_OmitsColumnDescriptionFromContent_WhenNull()
    {
        // Arrange
        var fakeMetadataCatalog = new FakeAnalyticsMetadataCatalog();
        var knowledgeSource = new MetadataAnalyticsSemanticKnowledgeSource(fakeMetadataCatalog);

        var columns = new List<AnalyticsColumnMetadata>
      {
            new("Population", "INT64", null)
      }.AsReadOnly();

        var table = new AnalyticsTableMetadata(
  ProjectId: "cbs-bi-poc",
      DatasetId: "analytics_demo",
         TableId: "city_population",
 Description: null,
            Columns: columns);

        fakeMetadataCatalog.SetMetadataToReturn(new[] { table }.AsReadOnly());
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
     var items = await knowledgeSource.GetItemsAsync(user, CancellationToken.None);

     // Assert
    var columnItem = items.Last();
        Assert.DoesNotContain("Description:", columnItem.Content);
    }

    [Fact]
    public async Task GetItemsAsync_SupportsMultipleTables()
    {
        // Arrange
        var fakeMetadataCatalog = new FakeAnalyticsMetadataCatalog();
   var knowledgeSource = new MetadataAnalyticsSemanticKnowledgeSource(fakeMetadataCatalog);

     var table1 = new AnalyticsTableMetadata(
         ProjectId: "cbs-bi-poc",
    DatasetId: "analytics_demo",
    TableId: "cities",
         Description: null,
Columns: new List<AnalyticsColumnMetadata> { new("Name", "STRING", null) }.AsReadOnly());

        var table2 = new AnalyticsTableMetadata(
    ProjectId: "cbs-bi-poc",
 DatasetId: "analytics_demo",
            TableId: "regions",
  Description: null,
        Columns: new List<AnalyticsColumnMetadata> { new("RegionCode", "STRING", null) }.AsReadOnly());

        fakeMetadataCatalog.SetMetadataToReturn(new[] { table1, table2 }.AsReadOnly());
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
        var items = await knowledgeSource.GetItemsAsync(user, CancellationToken.None);

        // Assert
        Assert.Equal(4, items.Count);
  Assert.True(items.Any(i => i.Id == "table:cbs-bi-poc.analytics_demo.cities"));
   Assert.True(items.Any(i => i.Id == "table:cbs-bi-poc.analytics_demo.regions"));
    }

    [Fact]
    public async Task GetItemsAsync_PreservesTableAndColumnOrderingFromMetadata()
    {
        // Arrange
        var fakeMetadataCatalog = new FakeAnalyticsMetadataCatalog();
        var knowledgeSource = new MetadataAnalyticsSemanticKnowledgeSource(fakeMetadataCatalog);

        var columns = new List<AnalyticsColumnMetadata>
        {
            new("FirstColumn", "STRING", null),
   new("SecondColumn", "INT64", null)
        }.AsReadOnly();

        var table = new AnalyticsTableMetadata(
       ProjectId: "cbs-bi-poc",
            DatasetId: "analytics_demo",
            TableId: "test_table",
      Description: null,
      Columns: columns);

        fakeMetadataCatalog.SetMetadataToReturn(new[] { table }.AsReadOnly());
var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

    // Act
  var items = await knowledgeSource.GetItemsAsync(user, CancellationToken.None);

        // Assert
        var itemList = items.ToList();
     Assert.Equal("table:cbs-bi-poc.analytics_demo.test_table", itemList[0].Id);
        Assert.Equal("column:cbs-bi-poc.analytics_demo.test_table.FirstColumn", itemList[1].Id);
        Assert.Equal("column:cbs-bi-poc.analytics_demo.test_table.SecondColumn", itemList[2].Id);
    }

    [Fact]
    public void Constructor_WithNullCatalog_ThrowsArgumentNullException()
    {
        // Act & Assert
      var exception = Assert.Throws<ArgumentNullException>(
            () => new MetadataAnalyticsSemanticKnowledgeSource(null!));

    Assert.Equal("metadataCatalog", exception.ParamName);
    }
}
