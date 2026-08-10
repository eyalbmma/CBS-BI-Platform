using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.Exceptions;
using CBS.BI.Application.Analytics.UseCases.GenerateAnalyticsQuery;
using CBS.BI.Application.Analytics.Validation;
using CBS.BI.Application.Security.Abstractions;
using CBS.BI.Application.Security.Models;
using Xunit;

namespace CBS.BI.Application.Tests;

public class GenerateAnalyticsQueryUseCaseTests
{
    /// <summary>
    /// Test fake for ICurrentUserContext that returns a configured CurrentUser.
    /// </summary>
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

    /// <summary>
    /// Test fake for IAnalyticsSemanticContextProvider that allows tests to configure
    /// the returned context and verify method calls.
    /// </summary>
 private sealed class FakeSemanticContextProvider : IAnalyticsSemanticContextProvider
    {
     private string _contextToReturn = "Available tables: city_population";
        private bool _shouldThrow = false;
        private Exception _exceptionToThrow = null!;

      public bool WasCalled { get; private set; }
        public string? ReceivedQuestion { get; private set; }
        public CurrentUser? ReceivedUser { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

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

        public Task<string> GetContextAsync(
       string question,
     CurrentUser user,
            CancellationToken cancellationToken)
        {
WasCalled = true;
       ReceivedQuestion = question;
     ReceivedUser = user;
            ReceivedCancellationToken = cancellationToken;

if (_shouldThrow)
            {
      throw _exceptionToThrow;
}

   return Task.FromResult(_contextToReturn);
        }
    }

    /// <summary>
    /// Test fake for IAnalyticsQueryGenerator that allows tests to configure
    /// the returned SQL and verify method calls.
    /// </summary>
    private sealed class FakeAnalyticsQueryGenerator : IAnalyticsQueryGenerator
    {
        private string _sqlToReturn = "SELECT 1";
    private bool _shouldThrow = false;
        private Exception _exceptionToThrow = null!;

        public bool WasCalled { get; private set; }
        public string? ReceivedQuestion { get; private set; }
        public string? ReceivedSemanticContext { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

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

        public Task<string> GenerateSqlAsync(
            string question,
        string semanticContext,
       CancellationToken cancellationToken)
      {
       WasCalled = true;
ReceivedQuestion = question;
            ReceivedSemanticContext = semanticContext;
    ReceivedCancellationToken = cancellationToken;

          if (_shouldThrow)
          {
   throw _exceptionToThrow;
  }

            return Task.FromResult(_sqlToReturn);
        }
  }

    [Fact]
    public async Task ExecuteAsync_WithValidQuestion_ReturnsGeneratedSafeSQL()
    {
        // Arrange
        var currentUser = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };
        var fakeCurrentUserContext = new FakeCurrentUserContext(currentUser);
        var fakeSemanticContextProvider = new FakeSemanticContextProvider();
      var fakeQueryGenerator = new FakeAnalyticsQueryGenerator();
        var validator = new ReadOnlyAnalyticsQuerySafetyValidator();
        var useCase = new GenerateAnalyticsQueryUseCase(
fakeCurrentUserContext,
            fakeSemanticContextProvider,
        fakeQueryGenerator,
            validator);

        var question = "Which city has the largest population?";
        var expectedSql = @"SELECT City, Population
FROM `cbs-bi-poc.analytics_demo.city_population`
ORDER BY Population DESC
LIMIT 1";

        fakeSemanticContextProvider.SetContextToReturn("Available tables: city_population with columns City, Population");
   fakeQueryGenerator.SetSqlToReturn(expectedSql);

     // Act
      var result = await useCase.ExecuteAsync(question, CancellationToken.None);

        // Assert
        Assert.Equal(expectedSql, result);
        Assert.True(fakeSemanticContextProvider.WasCalled);
        Assert.True(fakeQueryGenerator.WasCalled);
    }

    [Fact]
    public async Task ExecuteAsync_ProviderReceivesQuestionUserAndCancellationToken()
    {
        // Arrange
        var currentUser = new CurrentUser { UserId = "user-123", Roles = Array.Empty<string>().AsReadOnly() };
        var fakeCurrentUserContext = new FakeCurrentUserContext(currentUser);
  var fakeSemanticContextProvider = new FakeSemanticContextProvider();
        var fakeQueryGenerator = new FakeAnalyticsQueryGenerator();
   var validator = new ReadOnlyAnalyticsQuerySafetyValidator();
   var useCase = new GenerateAnalyticsQueryUseCase(
      fakeCurrentUserContext,
   fakeSemanticContextProvider,
      fakeQueryGenerator,
        validator);

     var question = "How many cities exist?";
        var cancellationToken = new CancellationToken(false);
    var safeSql = "SELECT COUNT(*) FROM cities";

        fakeSemanticContextProvider.SetContextToReturn("Table: cities");
        fakeQueryGenerator.SetSqlToReturn(safeSql);

        // Act
        await useCase.ExecuteAsync(question, cancellationToken);

  // Assert
        Assert.Equal(question, fakeSemanticContextProvider.ReceivedQuestion);
        Assert.Same(currentUser, fakeSemanticContextProvider.ReceivedUser);
        Assert.Equal(cancellationToken, fakeSemanticContextProvider.ReceivedCancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_GeneratorReceivesQuestionSemanticContextAndCancellationToken()
    {
    // Arrange
        var currentUser = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };
 var fakeCurrentUserContext = new FakeCurrentUserContext(currentUser);
   var fakeSemanticContextProvider = new FakeSemanticContextProvider();
    var fakeQueryGenerator = new FakeAnalyticsQueryGenerator();
        var validator = new ReadOnlyAnalyticsQuerySafetyValidator();
        var useCase = new GenerateAnalyticsQueryUseCase(
  fakeCurrentUserContext,
     fakeSemanticContextProvider,
     fakeQueryGenerator,
            validator);

        var question = "List all cities";
      var semanticContext = "Table: cities with columns name, population";
        var safeSql = "SELECT name, population FROM cities";
     var cancellationToken = new CancellationToken(false);

      fakeSemanticContextProvider.SetContextToReturn(semanticContext);
        fakeQueryGenerator.SetSqlToReturn(safeSql);

   // Act
await useCase.ExecuteAsync(question, cancellationToken);

        // Assert
Assert.Equal(question, fakeQueryGenerator.ReceivedQuestion);
        Assert.Equal(semanticContext, fakeQueryGenerator.ReceivedSemanticContext);
        Assert.Equal(cancellationToken, fakeQueryGenerator.ReceivedCancellationToken);
  }

    [Fact]
    public async Task ExecuteAsync_WithEmptyQuestion_ThrowsBeforeCallingProvider()
 {
        // Arrange
        var currentUser = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };
        var fakeCurrentUserContext = new FakeCurrentUserContext(currentUser);
     var fakeSemanticContextProvider = new FakeSemanticContextProvider();
        var fakeQueryGenerator = new FakeAnalyticsQueryGenerator();
        var validator = new ReadOnlyAnalyticsQuerySafetyValidator();
        var useCase = new GenerateAnalyticsQueryUseCase(
            fakeCurrentUserContext,
 fakeSemanticContextProvider,
   fakeQueryGenerator,
            validator);

        var question = "";

  // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
() => useCase.ExecuteAsync(question, CancellationToken.None));

        Assert.False(fakeSemanticContextProvider.WasCalled);
        Assert.False(fakeQueryGenerator.WasCalled);
    Assert.Contains("Question", exception.Message);
 }

    [Fact]
    public async Task ExecuteAsync_WhenProviderReturnsEmptyContext_ThrowsAnalyticsQueryGenerationException()
    {
        // Arrange
     var currentUser = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };
     var fakeCurrentUserContext = new FakeCurrentUserContext(currentUser);
var fakeSemanticContextProvider = new FakeSemanticContextProvider();
    var fakeQueryGenerator = new FakeAnalyticsQueryGenerator();
     var validator = new ReadOnlyAnalyticsQuerySafetyValidator();
        var useCase = new GenerateAnalyticsQueryUseCase(
     fakeCurrentUserContext,
  fakeSemanticContextProvider,
   fakeQueryGenerator,
            validator);

        var question = "What is the answer?";
 fakeSemanticContextProvider.SetContextToReturn("");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AnalyticsQueryGenerationException>(
    () => useCase.ExecuteAsync(question, CancellationToken.None));

     Assert.False(fakeQueryGenerator.WasCalled);
    Assert.NotNull(exception);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGeneratorReturnsUnsafeSql_ThrowsUnsafeAnalyticsQueryException()
    {
        // Arrange
        var currentUser = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };
    var fakeCurrentUserContext = new FakeCurrentUserContext(currentUser);
        var fakeSemanticContextProvider = new FakeSemanticContextProvider();
        var fakeQueryGenerator = new FakeAnalyticsQueryGenerator();
        var validator = new ReadOnlyAnalyticsQuerySafetyValidator();
        var useCase = new GenerateAnalyticsQueryUseCase(
         fakeCurrentUserContext,
   fakeSemanticContextProvider,
    fakeQueryGenerator,
  validator);

        var question = "Delete records";
        var unsafeSql = "DELETE FROM cities";

        fakeSemanticContextProvider.SetContextToReturn("Table: cities");
        fakeQueryGenerator.SetSqlToReturn(unsafeSql);

        // Act & Assert
     var exception = await Assert.ThrowsAsync<UnsafeAnalyticsQueryException>(
            () => useCase.ExecuteAsync(question, CancellationToken.None));

      Assert.NotNull(exception);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGeneratorThrowsAnalyticsQueryGenerationException_PropagatesException()
    {
// Arrange
  var currentUser = new CurrentUser { UserId = "test-user", Roles = Array.Empty<string>().AsReadOnly() };
        var fakeCurrentUserContext = new FakeCurrentUserContext(currentUser);
        var fakeSemanticContextProvider = new FakeSemanticContextProvider();
        var fakeQueryGenerator = new FakeAnalyticsQueryGenerator();
        var validator = new ReadOnlyAnalyticsQuerySafetyValidator();
    var useCase = new GenerateAnalyticsQueryUseCase(
         fakeCurrentUserContext,
    fakeSemanticContextProvider,
            fakeQueryGenerator,
     validator);

        var question = "Ask a question";
        var expectedException = new AnalyticsQueryGenerationException("Generator failed");

        fakeSemanticContextProvider.SetContextToReturn("Valid context");
     fakeQueryGenerator.SetExceptionToThrow(expectedException);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<AnalyticsQueryGenerationException>(
 () => useCase.ExecuteAsync(question, CancellationToken.None));

 Assert.Same(expectedException, thrownException);
    }

  [Fact]
    public async Task ExecuteAsync_CurrentUserPassedToProviderIsNotModified()
    {
        // Arrange
        var originalUser = new CurrentUser { UserId = "user-456", Roles = new[] { "role1", "role2" }.AsReadOnly() };
      var fakeCurrentUserContext = new FakeCurrentUserContext(originalUser);
        var fakeSemanticContextProvider = new FakeSemanticContextProvider();
        var fakeQueryGenerator = new FakeAnalyticsQueryGenerator();
        var validator = new ReadOnlyAnalyticsQuerySafetyValidator();
    var useCase = new GenerateAnalyticsQueryUseCase(
   fakeCurrentUserContext,
            fakeSemanticContextProvider,
         fakeQueryGenerator,
validator);

        var question = "Test question";
      var safeSql = "SELECT * FROM table";

        fakeSemanticContextProvider.SetContextToReturn("context");
        fakeQueryGenerator.SetSqlToReturn(safeSql);

   // Act
        await useCase.ExecuteAsync(question, CancellationToken.None);

        // Assert
  Assert.Same(originalUser, fakeSemanticContextProvider.ReceivedUser);
        Assert.Equal("user-456", fakeSemanticContextProvider.ReceivedUser!.UserId);
        Assert.Equal(2, fakeSemanticContextProvider.ReceivedUser!.Roles.Count);
    }
}
