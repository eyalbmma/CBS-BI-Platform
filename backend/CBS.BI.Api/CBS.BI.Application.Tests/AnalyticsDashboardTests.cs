using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Analytics.UseCases.DeleteAnalyticsDashboard;
using CBS.BI.Application.Analytics.UseCases.GetAnalyticsDashboards;
using CBS.BI.Application.Analytics.UseCases.GetAnalyticsDashboardById;
using CBS.BI.Application.Analytics.UseCases.SaveAnalyticsDashboard;
using CBS.BI.Application.Analytics.UseCases.SaveAnalyticsQuery;
using CBS.BI.Application.Security.Models;
using Xunit;

namespace CBS.BI.Application.Tests;

public sealed class AnalyticsDashboardTests
{
    private readonly FakeSavedAnalyticsQueryStore _queryStore;
    private readonly FakeAnalyticsDashboardStore _dashboardStore;
  private readonly CurrentUser _testUser;
    private readonly CurrentUser _otherUser;

   public AnalyticsDashboardTests()
    {
   _queryStore = new FakeSavedAnalyticsQueryStore();
  _dashboardStore = new FakeAnalyticsDashboardStore();
        _testUser = new CurrentUser { UserId = "user-1", Roles = Array.Empty<string>().AsReadOnly() };
        _otherUser = new CurrentUser { UserId = "user-2", Roles = Array.Empty<string>().AsReadOnly() };
    }

    [Fact]
    public async Task SaveDashboard_StoresDashboardSuccessfully()
    {
 var saveQueryUseCase = new SaveAnalyticsQueryUseCase(_queryStore);
     var saveDashboardUseCase = new SaveAnalyticsDashboardUseCase(_dashboardStore, _queryStore);

  var savedQuery = await saveQueryUseCase.ExecuteAsync("Test Query", "Test Question", _testUser, CancellationToken.None);

     var widgets = new[]
      {
      new AnalyticsDashboardWidget(
    Id: Guid.NewGuid().ToString(),
Title: "Test Widget",
 SavedQueryId: savedQuery.Id,
VisualizationType: "Table",
 DisplayOrder: 1)
      };

   var result = await saveDashboardUseCase.ExecuteAsync(
   "Test Dashboard",
   false,
         widgets,
       _testUser,
       CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Id);
     Assert.Equal("Test Dashboard", result.Name);
        Assert.False(result.IsDynamic);
       Assert.Single(result.Widgets);
      Assert.Equal(_testUser.UserId, result.CreatedByUserId);
    }

    [Fact]
    public async Task SaveDashboard_RejectEmptyName()
    {
        var saveDashboardUseCase = new SaveAnalyticsDashboardUseCase(_dashboardStore, _queryStore);

     await Assert.ThrowsAsync<ArgumentException>(
   () => saveDashboardUseCase.ExecuteAsync(
       "",
  false,
         Array.Empty<AnalyticsDashboardWidget>(),
     _testUser,
    CancellationToken.None));
    }

    [Fact]
    public async Task SaveDashboard_RejectInvalidSavedQueryId()
    {
        var saveDashboardUseCase = new SaveAnalyticsDashboardUseCase(_dashboardStore, _queryStore);

  var widgets = new[]
  {
      new AnalyticsDashboardWidget(
     Id: Guid.NewGuid().ToString(),
Title: "Test Widget",
SavedQueryId: "nonexistent-query-id",
    VisualizationType: "Table",
    DisplayOrder: 1)
     };

 await Assert.ThrowsAsync<InvalidOperationException>(
   () => saveDashboardUseCase.ExecuteAsync(
          "Test Dashboard",
     false,
    widgets,
      _testUser,
    CancellationToken.None));
    }

    [Fact]
 public async Task SaveDashboard_PreventsCrossUserQueryReference()
    {
        var saveQueryUseCase = new SaveAnalyticsQueryUseCase(_queryStore);
 var saveDashboardUseCase = new SaveAnalyticsDashboardUseCase(_dashboardStore, _queryStore);

  var otherUserQuery = await saveQueryUseCase.ExecuteAsync("Other Query", "Other Question", _otherUser, CancellationToken.None);

      var widgets = new[]
        {
          new AnalyticsDashboardWidget(
    Id: Guid.NewGuid().ToString(),
 Title: "Test Widget",
   SavedQueryId: otherUserQuery.Id,
     VisualizationType: "Table",
   DisplayOrder: 1)
        };

 await Assert.ThrowsAsync<InvalidOperationException>(
      () => saveDashboardUseCase.ExecuteAsync(
  "Test Dashboard",
   false,
         widgets,
  _testUser,
    CancellationToken.None));
   }

    [Fact]
    public async Task GetDashboards_ReturnsOnlyCurrentUserDashboards()
    {
     var saveQueryUseCase = new SaveAnalyticsQueryUseCase(_queryStore);
    var saveDashboardUseCase = new SaveAnalyticsDashboardUseCase(_dashboardStore, _queryStore);
        var getUseCase = new GetAnalyticsDashboardsUseCase(_dashboardStore);

   var query1 = await saveQueryUseCase.ExecuteAsync("Query 1", "Question 1", _testUser, CancellationToken.None);
   var query2 = await saveQueryUseCase.ExecuteAsync("Query 2", "Question 2", _otherUser, CancellationToken.None);

     var widgets1 = new[] { new AnalyticsDashboardWidget(Guid.NewGuid().ToString(), "W1", query1.Id, "Table", 1) };
        var widgets2 = new[] { new AnalyticsDashboardWidget(Guid.NewGuid().ToString(), "W2", query2.Id, "Table", 1) };

    var dashboard1 = await saveDashboardUseCase.ExecuteAsync("Dashboard 1", false, widgets1, _testUser, CancellationToken.None);
   var dashboard2 = await saveDashboardUseCase.ExecuteAsync("Dashboard 2", false, widgets2, _otherUser, CancellationToken.None);

   var userDashboards = await getUseCase.ExecuteAsync(_testUser, CancellationToken.None);

  Assert.Single(userDashboards);
      Assert.Equal(dashboard1.Id, userDashboards.First().Id);
    }

 [Fact]
    public async Task DeleteDashboard_RemovesDashboardForOwner()
    {
 var saveQueryUseCase = new SaveAnalyticsQueryUseCase(_queryStore);
        var saveDashboardUseCase = new SaveAnalyticsDashboardUseCase(_dashboardStore, _queryStore);
 var deleteUseCase = new DeleteAnalyticsDashboardUseCase(_dashboardStore);

   var query = await saveQueryUseCase.ExecuteAsync("Query", "Question", _testUser, CancellationToken.None);
        var widgets = new[] { new AnalyticsDashboardWidget(Guid.NewGuid().ToString(), "W", query.Id, "Table", 1) };

        var dashboard = await saveDashboardUseCase.ExecuteAsync("Dashboard", false, widgets, _testUser, CancellationToken.None);

  var deleted = await deleteUseCase.ExecuteAsync(dashboard.Id, _testUser, CancellationToken.None);

      Assert.True(deleted);
    }

   [Fact]
    public async Task DeleteDashboard_PreventsDeletionByNonOwner()
    {
       var saveQueryUseCase = new SaveAnalyticsQueryUseCase(_queryStore);
    var saveDashboardUseCase = new SaveAnalyticsDashboardUseCase(_dashboardStore, _queryStore);
 var deleteUseCase = new DeleteAnalyticsDashboardUseCase(_dashboardStore);

      var query = await saveQueryUseCase.ExecuteAsync("Query", "Question", _testUser, CancellationToken.None);
  var widgets = new[] { new AnalyticsDashboardWidget(Guid.NewGuid().ToString(), "W", query.Id, "Table", 1) };

 var dashboard = await saveDashboardUseCase.ExecuteAsync("Dashboard", false, widgets, _testUser, CancellationToken.None);

  var deleted = await deleteUseCase.ExecuteAsync(dashboard.Id, _otherUser, CancellationToken.None);

     Assert.False(deleted);
    }

 [Fact]
    public async Task GetDashboardById_ReturnsOnlyForOwner()
    {
        var saveQueryUseCase = new SaveAnalyticsQueryUseCase(_queryStore);
     var saveDashboardUseCase = new SaveAnalyticsDashboardUseCase(_dashboardStore, _queryStore);
      var getByIdUseCase = new GetAnalyticsDashboardByIdUseCase(_dashboardStore);

    var query = await saveQueryUseCase.ExecuteAsync("Query", "Question", _testUser, CancellationToken.None);
        var widgets = new[] { new AnalyticsDashboardWidget(Guid.NewGuid().ToString(), "W", query.Id, "Table", 1) };

       var dashboard = await saveDashboardUseCase.ExecuteAsync("Dashboard", false, widgets, _testUser, CancellationToken.None);

    var ownResult = await getByIdUseCase.ExecuteAsync(dashboard.Id, _testUser, CancellationToken.None);
        Assert.NotNull(ownResult);

      var otherResult = await getByIdUseCase.ExecuteAsync(dashboard.Id, _otherUser, CancellationToken.None);
        Assert.Null(otherResult);
    }

    [Fact]
 public async Task Dashboard_WidgetOrderPreserved()
    {
     var saveQueryUseCase = new SaveAnalyticsQueryUseCase(_queryStore);
        var saveDashboardUseCase = new SaveAnalyticsDashboardUseCase(_dashboardStore, _queryStore);

     var query = await saveQueryUseCase.ExecuteAsync("Query", "Question", _testUser, CancellationToken.None);

       var widgets = new[]
       {
    new AnalyticsDashboardWidget(Guid.NewGuid().ToString(), "Widget 3", query.Id, "Table", 3),
  new AnalyticsDashboardWidget(Guid.NewGuid().ToString(), "Widget 1", query.Id, "Table", 1),
      new AnalyticsDashboardWidget(Guid.NewGuid().ToString(), "Widget 2", query.Id, "Table", 2)
     };

    var dashboard = await saveDashboardUseCase.ExecuteAsync("Dashboard", false, widgets, _testUser, CancellationToken.None);

    Assert.Equal(3, dashboard.Widgets.Count);
    Assert.Equal(3, dashboard.Widgets.First().DisplayOrder);
       Assert.Equal(1, dashboard.Widgets.Skip(1).First().DisplayOrder);
     Assert.Equal(2, dashboard.Widgets.Last().DisplayOrder);
    }

  [Fact]
 public async Task Dashboard_IsDynamicFlagPreserved()
    {
        var saveQueryUseCase = new SaveAnalyticsQueryUseCase(_queryStore);
      var saveDashboardUseCase = new SaveAnalyticsDashboardUseCase(_dashboardStore, _queryStore);

      var query = await saveQueryUseCase.ExecuteAsync("Query", "Question", _testUser, CancellationToken.None);
 var widgets = new[] { new AnalyticsDashboardWidget(Guid.NewGuid().ToString(), "W", query.Id, "Table", 1) };

       var staticDashboard = await saveDashboardUseCase.ExecuteAsync("Static", false, widgets, _testUser, CancellationToken.None);
    var dynamicDashboard = await saveDashboardUseCase.ExecuteAsync("Dynamic", true, widgets, _testUser, CancellationToken.None);

Assert.False(staticDashboard.IsDynamic);
  Assert.True(dynamicDashboard.IsDynamic);
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

/// <summary>
    /// Test-only fake implementation of IAnalyticsDashboardStore.
    /// Used only for Application layer testing, independent of Infrastructure.
    /// </summary>
    private sealed class FakeAnalyticsDashboardStore : IAnalyticsDashboardStore
    {
     private readonly Dictionary<string, AnalyticsDashboard> _dashboardsById = new();

   public Task<IReadOnlyCollection<AnalyticsDashboard>> GetForUserAsync(
            CurrentUser user,
            CancellationToken cancellationToken)
        {
          cancellationToken.ThrowIfCancellationRequested();

        var userDashboards = _dashboardsById.Values
  .Where(d => d.CreatedByUserId == user.UserId)
      .OrderByDescending(d => d.CreatedAtUtc)
                .ToList()
         .AsReadOnly();

       return Task.FromResult<IReadOnlyCollection<AnalyticsDashboard>>(userDashboards);
        }

    public Task<AnalyticsDashboard?> GetByIdAsync(
         string id,
            CurrentUser user,
        CancellationToken cancellationToken)
        {
        cancellationToken.ThrowIfCancellationRequested();

       if (string.IsNullOrWhiteSpace(id))
          {
   return Task.FromResult<AnalyticsDashboard?>(null);
            }

       if (_dashboardsById.TryGetValue(id, out var dashboard))
            {
         if (dashboard.CreatedByUserId == user.UserId)
{
         return Task.FromResult<AnalyticsDashboard?>(dashboard);
         }
    }

           return Task.FromResult<AnalyticsDashboard?>(null);
   }

        public Task SaveAsync(
        AnalyticsDashboard dashboard,
            CancellationToken cancellationToken)
        {
      cancellationToken.ThrowIfCancellationRequested();

            if (dashboard == null)
      {
    throw new ArgumentNullException(nameof(dashboard));
            }

           _dashboardsById[dashboard.Id] = dashboard;

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

        if (_dashboardsById.TryGetValue(id, out var dashboard))
            {
          if (dashboard.CreatedByUserId == user.UserId)
 {
   _dashboardsById.Remove(id);
           return Task.FromResult(true);
                }
        }

         return Task.FromResult(false);
        }
    }
}
