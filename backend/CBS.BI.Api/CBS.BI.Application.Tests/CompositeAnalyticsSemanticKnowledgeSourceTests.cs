using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Analytics.SemanticKnowledge;
using CBS.BI.Application.Security.Models;
using Xunit;

namespace CBS.BI.Application.Tests;

public class CompositeAnalyticsSemanticKnowledgeSourceTests
{
    private sealed class FakeAnalyticsMetadataCatalog : IAnalyticsMetadataCatalog
    {
        private IReadOnlyCollection<AnalyticsTableMetadata> _tablesToReturn = Array.Empty<AnalyticsTableMetadata>();

     public CurrentUser? ReceivedUser { get; private set; }
    public CancellationToken ReceivedCancellationToken { get; private set; }

        public void SetTablesToReturn(IReadOnlyCollection<AnalyticsTableMetadata> tables)
        {
         _tablesToReturn = tables;
 }

  public Task<IReadOnlyCollection<AnalyticsTableMetadata>> GetTablesAsync(
        CurrentUser user,
        CancellationToken cancellationToken)
    {
        ReceivedUser = user;
   ReceivedCancellationToken = cancellationToken;
     
 IReadOnlyCollection<AnalyticsTableMetadata> result = _tablesToReturn;
        return Task.FromResult(result);
    }
 }

    [Fact]
    public async Task GetItemsAsync_CombinesMetadataAndBusinessDictionaryItems()
    {
        // Arrange
        var fakeMetadataCatalog = new FakeAnalyticsMetadataCatalog();
        var metadataSource = new MetadataAnalyticsSemanticKnowledgeSource(fakeMetadataCatalog);
        var businessDictionarySource = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
        var composite = new CompositeAnalyticsSemanticKnowledgeSource(metadataSource, businessDictionarySource);

  var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

 // Act
        var items = await composite.GetItemsAsync(user, CancellationToken.None);

   // Assert
 Assert.True(items.Count >= 2);
     Assert.True(items.Any(i => i.SourceType == "BusinessDictionary"));
    }

    [Fact]
    public async Task GetItemsAsync_MetadataItemsAppearBeforeBusinessDictionaryItems()
    {
        // Arrange
        var columns = new List<AnalyticsColumnMetadata>
        {
    new("Population", "INT64", "population count")
        }.AsReadOnly();

        var table = new AnalyticsTableMetadata(
            ProjectId: "test-project",
            DatasetId: "test-dataset",
          TableId: "test-table",
            Description: "test table",
          Columns: columns);

     var fakeMetadataCatalog = new FakeAnalyticsMetadataCatalog();
        fakeMetadataCatalog.SetTablesToReturn(new[] { table }.AsReadOnly());

        var metadataSource = new MetadataAnalyticsSemanticKnowledgeSource(fakeMetadataCatalog);
    var businessDictionarySource = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
      var composite = new CompositeAnalyticsSemanticKnowledgeSource(metadataSource, businessDictionarySource);

        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

    // Act
        var items = await composite.GetItemsAsync(user, CancellationToken.None);

        // Assert
        var itemList = items.ToList();
      var firstBusinessDictIndex = itemList.FindIndex(i => i.SourceType == "BusinessDictionary");
        var lastMetadataIndex = itemList.FindLastIndex(i => i.SourceType == "TableMetadata" || i.SourceType == "ColumnMetadata");

  if (firstBusinessDictIndex >= 0 && lastMetadataIndex >= 0)
    {
            Assert.True(lastMetadataIndex < firstBusinessDictIndex);
   }
    }

    [Fact]
 public async Task GetItemsAsync_PreservesMetadataOrdering()
    {
        // Arrange
        var columns1 = new List<AnalyticsColumnMetadata>
     {
        new("Col1", "STRING", null),
     new("Col2", "INT64", null)
        }.AsReadOnly();

        var table1 = new AnalyticsTableMetadata(
            ProjectId: "project",
      DatasetId: "dataset",
   TableId: "table1",
  Description: null,
            Columns: columns1);

        var fakeMetadataCatalog = new FakeAnalyticsMetadataCatalog();
        fakeMetadataCatalog.SetTablesToReturn(new[] { table1 }.AsReadOnly());

        var metadataSource = new MetadataAnalyticsSemanticKnowledgeSource(fakeMetadataCatalog);
  var businessDictionarySource = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
  var composite = new CompositeAnalyticsSemanticKnowledgeSource(metadataSource, businessDictionarySource);

        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
        var items = await composite.GetItemsAsync(user, CancellationToken.None);

        // Assert
        var itemList = items.ToList();
   var tableItemIndex = itemList.FindIndex(i => i.Id == "table:project.dataset.table1");
 var col1ItemIndex = itemList.FindIndex(i => i.Id == "column:project.dataset.table1.Col1");
var col2ItemIndex = itemList.FindIndex(i => i.Id == "column:project.dataset.table1.Col2");

   Assert.True(tableItemIndex >= 0);
   Assert.True(col1ItemIndex > tableItemIndex);
        Assert.True(col2ItemIndex > col1ItemIndex);
    }

    [Fact]
    public async Task GetItemsAsync_SupportsMetadataSourceReturningEmptyCollection()
{
        // Arrange
        var fakeMetadataCatalog = new FakeAnalyticsMetadataCatalog();
        fakeMetadataCatalog.SetTablesToReturn(Array.Empty<AnalyticsTableMetadata>().AsReadOnly());

    var metadataSource = new MetadataAnalyticsSemanticKnowledgeSource(fakeMetadataCatalog);
        var businessDictionarySource = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
        var composite = new CompositeAnalyticsSemanticKnowledgeSource(metadataSource, businessDictionarySource);

        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Get expected business dictionary items
      var expectedBusinessDictionaryItems = await businessDictionarySource.GetItemsAsync(user, CancellationToken.None);

        // Act
        var items = await composite.GetItemsAsync(user, CancellationToken.None);

        // Assert
Assert.Equal(expectedBusinessDictionaryItems.Count, items.Count);
        Assert.All(items, item => Assert.Equal("BusinessDictionary", item.SourceType));
    Assert.True(items.SequenceEqual(expectedBusinessDictionaryItems));
    }

    [Fact]
    public async Task GetItemsAsync_CombinedCollectionContainsAllExpectedItems()
    {
 // Arrange
        var columns = new List<AnalyticsColumnMetadata>
  {
            new("TestColumn", "STRING", null)
        }.AsReadOnly();

     var table = new AnalyticsTableMetadata(
            ProjectId: "proj",
        DatasetId: "ds",
         TableId: "tbl",
   Description: null,
            Columns: columns);

        var fakeMetadataCatalog = new FakeAnalyticsMetadataCatalog();
        fakeMetadataCatalog.SetTablesToReturn(new[] { table }.AsReadOnly());

        var metadataSource = new MetadataAnalyticsSemanticKnowledgeSource(fakeMetadataCatalog);
        var businessDictionarySource = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
      var composite = new CompositeAnalyticsSemanticKnowledgeSource(metadataSource, businessDictionarySource);

        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
    var items = await composite.GetItemsAsync(user, CancellationToken.None);

        // Assert
   var itemList = items.ToList();
        Assert.True(itemList.Count >= 3);
        Assert.Contains(itemList, i => i.Id == "table:proj.ds.tbl");
        Assert.Contains(itemList, i => i.Id == "column:proj.ds.tbl.TestColumn");
   Assert.Contains(itemList, i => i.SourceType == "BusinessDictionary");
    }

    [Fact]
    public async Task GetItemsAsync_ReturnsReadOnlyCollection()
    {
        // Arrange
        var fakeMetadataCatalog = new FakeAnalyticsMetadataCatalog();
        var metadataSource = new MetadataAnalyticsSemanticKnowledgeSource(fakeMetadataCatalog);
        var businessDictionarySource = new BusinessDictionaryAnalyticsSemanticKnowledgeSource();
   var composite = new CompositeAnalyticsSemanticKnowledgeSource(metadataSource, businessDictionarySource);

        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
        var items = await composite.GetItemsAsync(user, CancellationToken.None);

        // Assert
        Assert.IsAssignableFrom<IReadOnlyCollection<AnalyticsSemanticKnowledgeItem>>(items);
  }

    [Fact]
    public void Constructor_WithNullMetadataSource_ThrowsArgumentNullException()
    {
        // Act & Assert
   var exception = Assert.Throws<ArgumentNullException>(
   () => new CompositeAnalyticsSemanticKnowledgeSource(null!, new BusinessDictionaryAnalyticsSemanticKnowledgeSource()));

        Assert.Equal("metadataSource", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullBusinessDictionarySource_ThrowsArgumentNullException()
    {
     // Act & Assert
     var exception = Assert.Throws<ArgumentNullException>(
  () => new CompositeAnalyticsSemanticKnowledgeSource(
       new MetadataAnalyticsSemanticKnowledgeSource(new FakeAnalyticsMetadataCatalog()), 
     null!));

        Assert.Equal("businessDictionarySource", exception.ParamName);
    }
}
