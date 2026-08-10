using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Exceptions;
using CBS.BI.Application.Analytics.Models;
using CBS.BI.Application.Analytics.UseCases.ExecuteAnalyticsQuery;
using CBS.BI.Application.Analytics.UseCases.ExecuteNaturalLanguageAnalyticsQuery;
using CBS.BI.Application.Analytics.UseCases.GenerateAnalyticsQuery;
using CBS.BI.Application.Analytics.Validation;
using CBS.BI.Application.Security.Abstractions;
using CBS.BI.Application.Security.Exceptions;
using CBS.BI.Application.Security.Models;
using Xunit;

namespace CBS.BI.Application.Tests;

public class ExecuteNaturalLanguageAnalyticsQueryUseCaseTests
{
    private sealed class FakeCurrentUserContext : ICurrentUserContext
    {
        private readonly CurrentUser _userToReturn;

    public FakeCurrentUserContext(CurrentUser user)
        {
     _userToReturn = user;
  }

    public CurrentUser GetCurrentUser()
        {
        return _userToReturn;
        }
    }

    private sealed class FakeSemanticContextProvider : IAnalyticsSemanticContextProvider
  {
 private string _contextToReturn = "Available tables: city_population";
        private bool _shouldThrow = false;
        private Exception _exceptionToThrow = null!;

  public Task<string> GetContextAsync(string question, CurrentUser user, CancellationToken cancellationToken)
        {
  if (_shouldThrow)
       {
       throw _exceptionToThrow;
   }

            return Task.FromResult(_contextToReturn);
     }

        public void SetContextToReturn(string context)
{
        _contextToReturn = context;
        _shouldThrow = false;
        }

        public void SetExceptionToThrow(Exception exception)
        {
         _shouldThrow = true;
     _exceptionToThrow = exception;
        }
    }

    private sealed class FakeAnalyticsQueryGenerator : IAnalyticsQueryGenerator
    {
  private string _sqlToReturn = "SELECT 1";
        private bool _shouldThrow = false;
        private Exception _exceptionToThrow = null!;

    public Task<string> GenerateSqlAsync(string question, string semanticContext, CancellationToken cancellationToken)
        {
      if (_shouldThrow)
            {
   throw _exceptionToThrow;
            }

            return Task.FromResult(_sqlToReturn);
  }

        public void SetSqlToReturn(string sql)
        {
    _sqlToReturn = sql;
     _shouldThrow = false;
        }

     public void SetExceptionToThrow(Exception exception)
        {
    _shouldThrow = true;
    _exceptionToThrow = exception;
        }
    }

    private sealed class FakeAnalyticsAuthorizationService : IAnalyticsAuthorizationService
    {
        private bool _shouldThrow = false;
        private Exception _exceptionToThrow = null!;

        public void EnsureCanExecuteQuery(CurrentUser user)
        {
            if (_shouldThrow)
    {
      throw _exceptionToThrow;
            }
        }

 public void SetExceptionToThrow(Exception exception)
        {
            _shouldThrow = true;
   _exceptionToThrow = exception;
        }
    }

    private sealed class FakeQueryCostEstimator : IQueryCostEstimator
    {
        private QueryCostEstimate _estimateToReturn = new() { EstimatedBytesProcessed = 100000, IsWithinLimit = true };
     private bool _shouldThrow = false;
        private Exception _exceptionToThrow = null!;

        public Task<QueryCostEstimate> EstimateAsync(string query, CancellationToken cancellationToken)
        {
     if (_shouldThrow)
          {
   throw _exceptionToThrow;
            }

            return Task.FromResult(_estimateToReturn);
        }

        public void SetEstimateToReturn(QueryCostEstimate estimate)
     {
        _estimateToReturn = estimate;
  _shouldThrow = false;
        }

     public void SetExceptionToThrow(Exception exception)
        {
            _shouldThrow = true;
     _exceptionToThrow = exception;
        }
    }

    private sealed class FakeAnalyticsQueryExecutor : IAnalyticsQueryExecutor
    {
    private AnalyticsQueryResult _resultToReturn = new()
        {
      Columns = new[] { "Column1" }.AsReadOnly(),
     Rows = new List<IReadOnlyDictionary<string, object?>>().AsReadOnly()
  };
        private bool _shouldThrow = false;
        private Exception _exceptionToThrow = null!;

        public string? ReceivedQuery { get; private set; }
        public bool WasCalled { get; private set; }

        public async Task<AnalyticsQueryResult> ExecuteAsync(string query, CancellationToken cancellationToken)
        {
WasCalled = true;
       ReceivedQuery = query;

            if (_shouldThrow)
       {
      throw _exceptionToThrow;
       }

    return await Task.FromResult(_resultToReturn);
        }

   public void SetResultToReturn(AnalyticsQueryResult result)
        {
            _resultToReturn = result;
         _shouldThrow = false;
        }

        public void SetExceptionToThrow(Exception exception)
        {
 _shouldThrow = true;
            _exceptionToThrow = exception;
    }
    }

    [Fact]
    public async Task ExecuteAsync_WithValidQuestion_ReturnsGeneratedSqlAndExecutionResult()
  {
        // Arrange
        var currentUser = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };
        var fakeCurrentUserContext = new FakeCurrentUserContext(currentUser);
      var fakeSemanticContextProvider = new FakeSemanticContextProvider();
        var fakeQueryGenerator = new FakeAnalyticsQueryGenerator();
        var fakeAuthorizationService = new FakeAnalyticsAuthorizationService();
        var fakeCostEstimator = new FakeQueryCostEstimator();
        var fakeQueryExecutor = new FakeAnalyticsQueryExecutor();
        var safetyValidator = new ReadOnlyAnalyticsQuerySafetyValidator();

        var generateUseCase = new GenerateAnalyticsQueryUseCase(
      fakeCurrentUserContext,
        fakeSemanticContextProvider,
  fakeQueryGenerator,
         safetyValidator);

     var executeUseCase = new ExecuteAnalyticsQueryUseCase(
       fakeCurrentUserContext,
        fakeAuthorizationService,
            safetyValidator,
     fakeCostEstimator,
       fakeQueryExecutor);

        var orchestrator = new ExecuteNaturalLanguageAnalyticsQueryUseCase(generateUseCase, executeUseCase);

        var question = "Which city has the largest population?";
        var expectedSql = "SELECT City FROM `cbs-bi-poc.analytics_demo.city_population` ORDER BY Population DESC LIMIT 1";
        var expectedResult = new AnalyticsQueryResult
        {
        Columns = new[] { "City" }.AsReadOnly(),
            Rows = new List<IReadOnlyDictionary<string, object?>>
{
          new Dictionary<string, object?> { { "City", "New York" } }.AsReadOnly()
    }.AsReadOnly()
        };

 fakeSemanticContextProvider.SetContextToReturn("Available tables: city_population");
        fakeQueryGenerator.SetSqlToReturn(expectedSql);
  fakeQueryExecutor.SetResultToReturn(expectedResult);

      // Act
        var result = await orchestrator.ExecuteAsync(question, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
     Assert.Equal(expectedSql, result.Sql);
        Assert.Same(expectedResult, result.Result);
    }

    [Fact]
    public async Task ExecuteAsync_PassesGeneratedSqlToExecutionUnchanged()
    {
        // Arrange
    var currentUser = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };
    var fakeCurrentUserContext = new FakeCurrentUserContext(currentUser);
  var fakeSemanticContextProvider = new FakeSemanticContextProvider();
        var fakeQueryGenerator = new FakeAnalyticsQueryGenerator();
        var fakeAuthorizationService = new FakeAnalyticsAuthorizationService();
     var fakeCostEstimator = new FakeQueryCostEstimator();
        var fakeQueryExecutor = new FakeAnalyticsQueryExecutor();
      var safetyValidator = new ReadOnlyAnalyticsQuerySafetyValidator();

        var generateUseCase = new GenerateAnalyticsQueryUseCase(
    fakeCurrentUserContext,
        fakeSemanticContextProvider,
      fakeQueryGenerator,
 safetyValidator);

        var executeUseCase = new ExecuteAnalyticsQueryUseCase(
            fakeCurrentUserContext,
          fakeAuthorizationService,
            safetyValidator,
        fakeCostEstimator,
            fakeQueryExecutor);

        var orchestrator = new ExecuteNaturalLanguageAnalyticsQueryUseCase(generateUseCase, executeUseCase);

        var generatedSql = "SELECT * FROM table WHERE 1=1";
        fakeSemanticContextProvider.SetContextToReturn("context");
        fakeQueryGenerator.SetSqlToReturn(generatedSql);

        // Act
        await orchestrator.ExecuteAsync("Show me data", CancellationToken.None);

        // Assert
        Assert.True(fakeQueryExecutor.WasCalled);
        Assert.Equal(generatedSql, fakeQueryExecutor.ReceivedQuery);
    }

    [Fact]
 public async Task ExecuteAsync_WhenGenerationFails_ExecutionDoesNotOccur()
    {
        // Arrange
    var currentUser = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };
        var fakeCurrentUserContext = new FakeCurrentUserContext(currentUser);
        var fakeSemanticContextProvider = new FakeSemanticContextProvider();
     var fakeQueryGenerator = new FakeAnalyticsQueryGenerator();
      var fakeAuthorizationService = new FakeAnalyticsAuthorizationService();
    var fakeCostEstimator = new FakeQueryCostEstimator();
        var fakeQueryExecutor = new FakeAnalyticsQueryExecutor();
        var safetyValidator = new ReadOnlyAnalyticsQuerySafetyValidator();

        var generateUseCase = new GenerateAnalyticsQueryUseCase(
         fakeCurrentUserContext,
          fakeSemanticContextProvider,
            fakeQueryGenerator,
            safetyValidator);

    var executeUseCase = new ExecuteAnalyticsQueryUseCase(
            fakeCurrentUserContext,
  fakeAuthorizationService,
            safetyValidator,
   fakeCostEstimator,
            fakeQueryExecutor);

        var orchestrator = new ExecuteNaturalLanguageAnalyticsQueryUseCase(generateUseCase, executeUseCase);

        var generateException = new AnalyticsQueryGenerationException("Generation failed");
        fakeSemanticContextProvider.SetExceptionToThrow(generateException);

    // Act & Assert
        var thrownException = await Assert.ThrowsAsync<AnalyticsQueryGenerationException>(
    () => orchestrator.ExecuteAsync("Test", CancellationToken.None));

    Assert.Same(generateException, thrownException);
     Assert.False(fakeQueryExecutor.WasCalled);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExecutionFails_ExceptionPropagates()
    {
  // Arrange
        var currentUser = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };
        var fakeCurrentUserContext = new FakeCurrentUserContext(currentUser);
 var fakeSemanticContextProvider = new FakeSemanticContextProvider();
        var fakeQueryGenerator = new FakeAnalyticsQueryGenerator();
        var fakeAuthorizationService = new FakeAnalyticsAuthorizationService();
        var fakeCostEstimator = new FakeQueryCostEstimator();
        var fakeQueryExecutor = new FakeAnalyticsQueryExecutor();
        var safetyValidator = new ReadOnlyAnalyticsQuerySafetyValidator();

        var generateUseCase = new GenerateAnalyticsQueryUseCase(
     fakeCurrentUserContext,
        fakeSemanticContextProvider,
     fakeQueryGenerator,
            safetyValidator);

        var executeUseCase = new ExecuteAnalyticsQueryUseCase(
        fakeCurrentUserContext,
            fakeAuthorizationService,
     safetyValidator,
            fakeCostEstimator,
 fakeQueryExecutor);

        var orchestrator = new ExecuteNaturalLanguageAnalyticsQueryUseCase(generateUseCase, executeUseCase);

 var executeException = new QueryCostLimitExceededException(1000000000);
        fakeSemanticContextProvider.SetContextToReturn("context");
        fakeQueryGenerator.SetSqlToReturn("SELECT 1");
    fakeQueryExecutor.SetExceptionToThrow(executeException);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<QueryCostLimitExceededException>(
            () => orchestrator.ExecuteAsync("Test", CancellationToken.None));

  Assert.Same(executeException, thrownException);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesAuthorizationException()
{
        // Arrange
        var currentUser = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };
        var fakeCurrentUserContext = new FakeCurrentUserContext(currentUser);
        var fakeSemanticContextProvider = new FakeSemanticContextProvider();
        var fakeQueryGenerator = new FakeAnalyticsQueryGenerator();
        var fakeAuthorizationService = new FakeAnalyticsAuthorizationService();
     var fakeCostEstimator = new FakeQueryCostEstimator();
  var fakeQueryExecutor = new FakeAnalyticsQueryExecutor();
        var safetyValidator = new ReadOnlyAnalyticsQuerySafetyValidator();

   var generateUseCase = new GenerateAnalyticsQueryUseCase(
    fakeCurrentUserContext,
      fakeSemanticContextProvider,
        fakeQueryGenerator,
      safetyValidator);

        var executeUseCase = new ExecuteAnalyticsQueryUseCase(
       fakeCurrentUserContext,
   fakeAuthorizationService,
       safetyValidator,
 fakeCostEstimator,
            fakeQueryExecutor);

        var orchestrator = new ExecuteNaturalLanguageAnalyticsQueryUseCase(generateUseCase, executeUseCase);

        var authException = new AnalyticsAuthorizationException();
    fakeSemanticContextProvider.SetContextToReturn("context");
        fakeQueryGenerator.SetSqlToReturn("SELECT 1");
     fakeAuthorizationService.SetExceptionToThrow(authException);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<AnalyticsAuthorizationException>(
       () => orchestrator.ExecuteAsync("Test", CancellationToken.None));

      Assert.Same(authException, thrownException);
    }
}
