using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Security.Models;
using CBS.BI.Application.Analytics.UseCases.DeleteSavedAnalyticsQuery;
using CBS.BI.Application.Analytics.UseCases.GetSavedAnalyticsQueries;
using CBS.BI.Application.Analytics.UseCases.SaveAnalyticsQuery;
using Xunit;

namespace CBS.BI.Application.Tests;

public sealed class SavedAnalyticsQueryTests
{
  private readonly FakeSavedAnalyticsQueryStore _store;
    private readonly CurrentUser _testUser;
    private readonly CurrentUser _otherUser;

    public SavedAnalyticsQueryTests()
    {
        _store = new FakeSavedAnalyticsQueryStore();
        _testUser = new CurrentUser { UserId = "user-1", Roles = Array.Empty<string>().AsReadOnly() };
        _otherUser = new CurrentUser { UserId = "user-2", Roles = Array.Empty<string>().AsReadOnly() };
    }

    [Fact]
    public async Task SaveQuery_StoresQuerySuccessfully()
    {
        var saveUseCase = new SaveAnalyticsQueryUseCase(_store);

        var result = await saveUseCase.ExecuteAsync(
            "Test Query",
            "איזו עיר בעלת שיעור האבטלה הנמוך ביותר?",
            _testUser,
         CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Id);
        Assert.Equal("Test Query", result.Name);
        Assert.Equal("איזו עיר בעלת שיעור האבטלה הנמוך ביותר?", result.Question);
        Assert.Equal(_testUser.UserId, result.CreatedByUserId);
    }

  [Fact]
    public async Task SaveQuery_RejectEmptyName()
 {
        var saveUseCase = new SaveAnalyticsQueryUseCase(_store);

 await Assert.ThrowsAsync<ArgumentException>(
    () => saveUseCase.ExecuteAsync(
       "",
    "איזו עיר בעלת שיעור האבטלה הנמוך ביותר?",
      _testUser,
                CancellationToken.None));
    }

    [Fact]
    public async Task SaveQuery_RejectEmptyQuestion()
    {
        var saveUseCase = new SaveAnalyticsQueryUseCase(_store);

        await Assert.ThrowsAsync<ArgumentException>(
         () => saveUseCase.ExecuteAsync(
        "Test Query",
     "",
         _testUser,
         CancellationToken.None));
    }

    [Fact]
    public async Task GetQueries_ReturnsOnlyCurrentUserQueries()
    {
        var saveUseCase = new SaveAnalyticsQueryUseCase(_store);
        var getUseCase = new GetSavedAnalyticsQueriesUseCase(_store);

        var query1 = await saveUseCase.ExecuteAsync("Query 1", "Question 1", _testUser, CancellationToken.None);
        var query2 = await saveUseCase.ExecuteAsync("Query 2", "Question 2", _otherUser, CancellationToken.None);

        var userQueries = await getUseCase.ExecuteAsync(_testUser, CancellationToken.None);

        Assert.Single(userQueries);
        Assert.Equal(query1.Id, userQueries.First().Id);
    }

    [Fact]
    public async Task DeleteQuery_RemovesQueryForOwner()
    {
 var saveUseCase = new SaveAnalyticsQueryUseCase(_store);
     var deleteUseCase = new DeleteSavedAnalyticsQueryUseCase(_store);

  var savedQuery = await saveUseCase.ExecuteAsync("Test Query", "Test Question", _testUser, CancellationToken.None);

        var deleted = await deleteUseCase.ExecuteAsync(savedQuery.Id, _testUser, CancellationToken.None);

        Assert.True(deleted);
    }

    [Fact]
    public async Task DeleteQuery_PreventsDeletionByNonOwner()
    {
        var saveUseCase = new SaveAnalyticsQueryUseCase(_store);
        var deleteUseCase = new DeleteSavedAnalyticsQueryUseCase(_store);

 var savedQuery = await saveUseCase.ExecuteAsync("Test Query", "Test Question", _testUser, CancellationToken.None);

  var deleted = await deleteUseCase.ExecuteAsync(savedQuery.Id, _otherUser, CancellationToken.None);

        Assert.False(deleted);
    }

    [Fact]
    public async Task GetQueries_ReturnsNewestFirst()
    {
        var saveUseCase = new SaveAnalyticsQueryUseCase(_store);
        var getUseCase = new GetSavedAnalyticsQueriesUseCase(_store);

        var query1 = await saveUseCase.ExecuteAsync("Query 1", "Question 1", _testUser, CancellationToken.None);
        await Task.Delay(10);
        var query2 = await saveUseCase.ExecuteAsync("Query 2", "Question 2", _testUser, CancellationToken.None);

     var queries = await getUseCase.ExecuteAsync(_testUser, CancellationToken.None);

     Assert.Equal(2, queries.Count);
        Assert.Equal(query2.Id, queries.First().Id);
        Assert.Equal(query1.Id, queries.Last().Id);
  }

    /// <summary>
    /// Test-only fake implementation of ISavedAnalyticsQueryStore.
    /// Used only for Application layer testing, independent of Infrastructure.
    /// </summary>
    private sealed class FakeSavedAnalyticsQueryStore : ISavedAnalyticsQueryStore
 {
    private readonly Dictionary<string, SavedAnalyticsQuery> _queriesById = new();

     public Task<IReadOnlyCollection<SavedAnalyticsQuery>> GetForUserAsync(
         CurrentUser user,
    CancellationToken cancellationToken)
     {
   cancellationToken.ThrowIfCancellationRequested();

       var userQueries = _queriesById.Values
      .Where(q => q.CreatedByUserId == user.UserId)
    .OrderByDescending(q => q.CreatedAtUtc)
    .ToList()
 .AsReadOnly();

            return Task.FromResult<IReadOnlyCollection<SavedAnalyticsQuery>>(userQueries);
        }

        public Task<SavedAnalyticsQuery?> GetByIdAsync(
       string id,
          CurrentUser user,
    CancellationToken cancellationToken)
      {
 cancellationToken.ThrowIfCancellationRequested();

   if (string.IsNullOrWhiteSpace(id))
            {
    return Task.FromResult<SavedAnalyticsQuery?>(null);
            }

     if (_queriesById.TryGetValue(id, out var query))
      {
            if (query.CreatedByUserId == user.UserId)
         {
          return Task.FromResult<SavedAnalyticsQuery?>(query);
   }
    }

       return Task.FromResult<SavedAnalyticsQuery?>(null);
      }

        public Task SaveAsync(
            SavedAnalyticsQuery query,
            CancellationToken cancellationToken)
     {
        cancellationToken.ThrowIfCancellationRequested();

            if (query == null)
            {
 throw new ArgumentNullException(nameof(query));
            }

            _queriesById[query.Id] = query;

            return Task.CompletedTask;
        }

        public Task<bool> DeleteAsync(
            string id,
CurrentUser user,
    CancellationToken cancellationToken)
        {
   cancellationToken.ThrowIfCancellationRequested();

   if (string.IsNullOrWhiteSpace(id))
     {
    return Task.FromResult(false);
        }

            if (_queriesById.TryGetValue(id, out var query))
            {
              if (query.CreatedByUserId == user.UserId)
      {
      _queriesById.Remove(id);
              return Task.FromResult(true);
       }
   }

      return Task.FromResult(false);
        }
  }
}
