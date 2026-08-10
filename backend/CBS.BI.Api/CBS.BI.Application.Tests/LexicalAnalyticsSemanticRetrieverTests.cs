using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Analytics.SemanticRetrieval;
using CBS.BI.Application.Security.Models;
using Xunit;

namespace CBS.BI.Application.Tests;

public class LexicalAnalyticsSemanticRetrieverTests
{
    private sealed class FakeAnalyticsSemanticKnowledgeSource : IAnalyticsSemanticKnowledgeSource
 {
        private IReadOnlyCollection<AnalyticsSemanticKnowledgeItem> _itemsToReturn = Array.Empty<AnalyticsSemanticKnowledgeItem>();
        private bool _shouldThrow = false;
        private Exception _exceptionToThrow = null!;

        public bool WasCalled { get; private set; }
        public CurrentUser? ReceivedUser { get; private set; }
public CancellationToken ReceivedCancellationToken { get; private set; }

     public void SetItemsToReturn(IReadOnlyCollection<AnalyticsSemanticKnowledgeItem> items)
        {
            _itemsToReturn = items;
    _shouldThrow = false;
   }

        public void SetExceptionToThrow(Exception exception)
        {
       _shouldThrow = true;
  _exceptionToThrow = exception;
     }

        public Task<IReadOnlyCollection<AnalyticsSemanticKnowledgeItem>> GetItemsAsync(
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

            return Task.FromResult(_itemsToReturn);
        }
    }

    [Fact]
    public async Task RetrieveAsync_WithItemContainingMatchingToken_ReturnsItem()
    {
     // Arrange
        var fakeSource = new FakeAnalyticsSemanticKnowledgeSource();
        var retriever = new LexicalAnalyticsSemanticRetriever(fakeSource);

        var item = new AnalyticsSemanticKnowledgeItem(
        Id: "table:city_population",
        Content: "Table: city_population - Contains city population data",
            SourceType: "TableMetadata",
 SourceReference: "city_population");

    fakeSource.SetItemsToReturn(new[] { item }.AsReadOnly());
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

     // Act
        var result = await retriever.RetrieveAsync("population", user, CancellationToken.None);

      // Assert
  Assert.Single(result);
    Assert.Contains(result, r => r.Id == "table:city_population");
    }

    [Fact]
    public async Task RetrieveAsync_RanksItemWithMoreMatchingTokensAhead()
    {
        // Arrange
        var fakeSource = new FakeAnalyticsSemanticKnowledgeSource();
    var retriever = new LexicalAnalyticsSemanticRetriever(fakeSource);

        var weakMatch = new AnalyticsSemanticKnowledgeItem(
            Id: "item1",
            Content: "Table with city data",
    SourceType: "TableMetadata",
 SourceReference: null);

        var strongMatch = new AnalyticsSemanticKnowledgeItem(
          Id: "item2",
       Content: "City population and city area statistics",
     SourceType: "TableMetadata",
         SourceReference: null);

        fakeSource.SetItemsToReturn(new[] { weakMatch, strongMatch }.AsReadOnly());
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
        var result = await retriever.RetrieveAsync("city population area", user, CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal("item2", resultList[0].Id);
    Assert.Equal("item1", resultList[1].Id);
 }

    [Fact]
    public async Task RetrieveAsync_DoesNotReturnZeroScoreItems()
    {
        // Arrange
     var fakeSource = new FakeAnalyticsSemanticKnowledgeSource();
        var retriever = new LexicalAnalyticsSemanticRetriever(fakeSource);

        var matchingItem = new AnalyticsSemanticKnowledgeItem(
      Id: "match",
            Content: "Contains the word population",
          SourceType: "TableMetadata",
SourceReference: null);

        var nonMatchingItem = new AnalyticsSemanticKnowledgeItem(
            Id: "nomatch",
            Content: "Contains unrelated content",
      SourceType: "TableMetadata",
  SourceReference: null);

        fakeSource.SetItemsToReturn(new[] { matchingItem, nonMatchingItem }.AsReadOnly());
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
        var result = await retriever.RetrieveAsync("population", user, CancellationToken.None);

    // Assert
        Assert.Single(result);
        Assert.Equal("match", result.First().Id);
    }

    [Fact]
    public async Task RetrieveAsync_PreservesCorpusOrderWhenScoresAreEqual()
    {
 // Arrange
    var fakeSource = new FakeAnalyticsSemanticKnowledgeSource();
        var retriever = new LexicalAnalyticsSemanticRetriever(fakeSource);

        var item1 = new AnalyticsSemanticKnowledgeItem(
     Id: "item1",
            Content: "City table",
   SourceType: "TableMetadata",
            SourceReference: null);

   var item2 = new AnalyticsSemanticKnowledgeItem(
        Id: "item2",
            Content: "City table",
 SourceType: "TableMetadata",
            SourceReference: null);

  var item3 = new AnalyticsSemanticKnowledgeItem(
      Id: "item3",
            Content: "City table",
      SourceType: "TableMetadata",
  SourceReference: null);

   fakeSource.SetItemsToReturn(new[] { item1, item2, item3 }.AsReadOnly());
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
        var result = await retriever.RetrieveAsync("city", user, CancellationToken.None);

      // Assert
        var resultList = result.ToList();
        Assert.Equal(3, resultList.Count);
        Assert.Equal("item1", resultList[0].Id);
      Assert.Equal("item2", resultList[1].Id);
        Assert.Equal("item3", resultList[2].Id);
    }

    [Fact]
    public async Task RetrieveAsync_LimitsResultsToMaxResults()
    {
        // Arrange
        var fakeSource = new FakeAnalyticsSemanticKnowledgeSource();
        var retriever = new LexicalAnalyticsSemanticRetriever(fakeSource);

      var items = Enumerable.Range(1, 15)
         .Select(i => new AnalyticsSemanticKnowledgeItem(
    Id: $"item{i}",
     Content: "population city data",
        SourceType: "TableMetadata",
    SourceReference: null))
  .ToList()
     .AsReadOnly();

        fakeSource.SetItemsToReturn(items);
     var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

    // Act
        var result = await retriever.RetrieveAsync("population city data", user, CancellationToken.None);

        // Assert
    Assert.Equal(10, result.Count);
    }

    [Fact]
    public async Task RetrieveAsync_PassesCurrentUserAndCancellationTokenToSource()
    {
        // Arrange
        var fakeSource = new FakeAnalyticsSemanticKnowledgeSource();
        var retriever = new LexicalAnalyticsSemanticRetriever(fakeSource);

        fakeSource.SetItemsToReturn(Array.Empty<AnalyticsSemanticKnowledgeItem>().AsReadOnly());
        var user = new CurrentUser { UserId = "test-user", Roles = new[] { "role1" }.AsReadOnly() };
        var token = new CancellationToken(false);

        // Act
        await retriever.RetrieveAsync("question", user, token);

        // Assert
        Assert.True(fakeSource.WasCalled);
        Assert.Same(user, fakeSource.ReceivedUser);
   Assert.Equal(token, fakeSource.ReceivedCancellationToken);
 }

    [Fact]
    public async Task RetrieveAsync_ReturnsEmptyCollectionWhenNoItemsMatch()
    {
        // Arrange
     var fakeSource = new FakeAnalyticsSemanticKnowledgeSource();
        var retriever = new LexicalAnalyticsSemanticRetriever(fakeSource);

        var item = new AnalyticsSemanticKnowledgeItem(
            Id: "item1",
         Content: "Completely unrelated content",
   SourceType: "TableMetadata",
            SourceReference: null);

  fakeSource.SetItemsToReturn(new[] { item }.AsReadOnly());
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

  // Act
        var result = await retriever.RetrieveAsync("xyzabc123", user, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

[Fact]
    public async Task RetrieveAsync_MatchingIsCaseInsensitive()
    {
        // Arrange
        var fakeSource = new FakeAnalyticsSemanticKnowledgeSource();
        var retriever = new LexicalAnalyticsSemanticRetriever(fakeSource);

        var item = new AnalyticsSemanticKnowledgeItem(
            Id: "item1",
      Content: "Table POPULATION City",
            SourceType: "TableMetadata",
    SourceReference: null);

        fakeSource.SetItemsToReturn(new[] { item }.AsReadOnly());
   var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act - query in lowercase, content in mixed case
        var result = await retriever.RetrieveAsync("population city", user, CancellationToken.None);

   // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task RetrieveAsync_IncludesSourceReferenceInSearchableText()
    {
   // Arrange
        var fakeSource = new FakeAnalyticsSemanticKnowledgeSource();
        var retriever = new LexicalAnalyticsSemanticRetriever(fakeSource);

        var item = new AnalyticsSemanticKnowledgeItem(
            Id: "item1",
       Content: "Some content",
  SourceType: "TableMetadata",
        SourceReference: "cbs-bi-poc.analytics_demo.city_population");

        fakeSource.SetItemsToReturn(new[] { item }.AsReadOnly());
  var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

     // Act - query matches the SourceReference
        var result = await retriever.RetrieveAsync("analytics_demo", user, CancellationToken.None);

        // Assert
   Assert.Single(result);
    }

    [Fact]
    public void Constructor_WithNullSource_ThrowsArgumentNullException()
    {
        // Act & Assert
   var exception = Assert.Throws<ArgumentNullException>(
       () => new LexicalAnalyticsSemanticRetriever(null!));

        Assert.Equal("knowledgeSource", exception.ParamName);
  }

    [Fact]
    public async Task RetrieveAsync_WithEmptyQuestion_ReturnsEmptyCollection()
    {
        // Arrange
        var fakeSource = new FakeAnalyticsSemanticKnowledgeSource();
        var retriever = new LexicalAnalyticsSemanticRetriever(fakeSource);

        var item = new AnalyticsSemanticKnowledgeItem(
            Id: "item1",
   Content: "Some content",
    SourceType: "TableMetadata",
            SourceReference: null);

        fakeSource.SetItemsToReturn(new[] { item }.AsReadOnly());
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
        var result = await retriever.RetrieveAsync("", user, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task RetrieveAsync_RespectsCancellationToken()
    {
        // Arrange
  var fakeSource = new FakeAnalyticsSemanticKnowledgeSource();
        var retriever = new LexicalAnalyticsSemanticRetriever(fakeSource);

     using var cts = new CancellationTokenSource();
        cts.Cancel();

        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
         () => retriever.RetrieveAsync("question", user, cts.Token));
    }

    [Fact]
    public async Task RetrieveAsync_WithContainmentMatch_ReturnsItem()
    {
        // Arrange
        var fakeSource = new FakeAnalyticsSemanticKnowledgeSource();
        var retriever = new LexicalAnalyticsSemanticRetriever(fakeSource);

        var item = new AnalyticsSemanticKnowledgeItem(
       Id: "business_term:population",
   Content: "Business term: אוכלוסייה Synonyms: population, תושבים, residents",
            SourceType: "BusinessDictionary",
    SourceReference: "cbs-bi-poc.analytics_demo.city_population.Population");

        fakeSource.SetItemsToReturn(new[] { item }.AsReadOnly());
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

 // Act - Question contains "האוכלוסייה" which contains "אוכלוסייה" (corpus token)
        var result = await retriever.RetrieveAsync("איזו עיר בעלת האוכלוסייה הגדולה", user, CancellationToken.None);

        // Assert
        Assert.Single(result);
     Assert.Equal("business_term:population", result.First().Id);
    }

    [Fact]
    public async Task RetrieveAsync_ExactMatchRanksAboveContainmentMatch()
    {
        // Arrange
 var fakeSource = new FakeAnalyticsSemanticKnowledgeSource();
        var retriever = new LexicalAnalyticsSemanticRetriever(fakeSource);

        var containmentOnlyMatch = new AnalyticsSemanticKnowledgeItem(
 Id: "item1",
            Content: "Contains word 'population'",
            SourceType: "TableMetadata",
    SourceReference: null);

        var exactMatch = new AnalyticsSemanticKnowledgeItem(
            Id: "item2",
    Content: "Exact population match in content",
     SourceType: "TableMetadata",
     SourceReference: null);

     fakeSource.SetItemsToReturn(new[] { containmentOnlyMatch, exactMatch }.AsReadOnly());
    var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

        // Act
        var result = await retriever.RetrieveAsync("population", user, CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal("item2", resultList[0].Id);
  Assert.Equal("item1", resultList[1].Id);
  }

 [Fact]
    public async Task RetrieveAsync_ShortTokensDoNotCauseContainmentFalsePositives()
    {
        // Arrange
        var fakeSource = new FakeAnalyticsSemanticKnowledgeSource();
      var retriever = new LexicalAnalyticsSemanticRetriever(fakeSource);

     var item = new AnalyticsSemanticKnowledgeItem(
     Id: "item1",
            Content: "Contains word information",
      SourceType: "TableMetadata",
      SourceReference: null);

        fakeSource.SetItemsToReturn(new[] { item }.AsReadOnly());
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

     // Act - Very short token "in" should not match "information" via containment
   var result = await retriever.RetrieveAsync("in", user, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

 [Fact]
    public async Task RetrieveAsync_ShortExactTokensStillMatch()
    {
        // Arrange
  var fakeSource = new FakeAnalyticsSemanticKnowledgeSource();
        var retriever = new LexicalAnalyticsSemanticRetriever(fakeSource);

  var item = new AnalyticsSemanticKnowledgeItem(
       Id: "item1",
       Content: "עיר city municipality",
       SourceType: "BusinessDictionary",
          SourceReference: null);

        fakeSource.SetItemsToReturn(new[] { item }.AsReadOnly());
        var user = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };

    // Act - Short exact match "עיר" should still work via exact matching, not containment
        var result = await retriever.RetrieveAsync("עיר", user, CancellationToken.None);

        // Assert
    Assert.Single(result);
  Assert.Equal("item1", result.First().Id);
    }
}
