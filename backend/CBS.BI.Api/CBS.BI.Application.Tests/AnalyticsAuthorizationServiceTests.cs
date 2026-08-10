using CBS.BI.Application.Security.Authorization;
using CBS.BI.Application.Security.Exceptions;
using CBS.BI.Application.Security.Models;
using Xunit;

namespace CBS.BI.Application.Tests;

public class AnalyticsAuthorizationServiceTests
{
    private readonly AnalyticsAuthorizationService _service = new();

    [Fact]
    public void EnsureCanExecuteQuery_WithRequiredRole_AllowsExecution()
    {
        // Arrange
        var user = new CurrentUser
        {
   UserId = "test-user",
            Roles = new[] { "AnalyticsQueryExecutor" }.AsReadOnly()
};

    // Act & Assert - should not throw
        _service.EnsureCanExecuteQuery(user);
    }

    [Fact]
    public void EnsureCanExecuteQuery_RoleCaseInsensitive_AllowsExecution()
  {
        // Arrange
 var user = new CurrentUser
        {
      UserId = "test-user",
     Roles = new[] { "analyticsqueryexecutor" }.AsReadOnly()
        };

        // Act & Assert - should not throw
        _service.EnsureCanExecuteQuery(user);
    }

    [Fact]
  public void EnsureCanExecuteQuery_WithUnrelatedRole_ThrowsException()
    {
        // Arrange
        var user = new CurrentUser
        {
            UserId = "test-user",
      Roles = new[] { "OtherRole" }.AsReadOnly()
  };

        // Act & Assert
    Assert.Throws<AnalyticsAuthorizationException>(() => _service.EnsureCanExecuteQuery(user));
    }

    [Fact]
    public void EnsureCanExecuteQuery_WithNoRoles_ThrowsException()
    {
        // Arrange
   var user = new CurrentUser
        {
            UserId = "test-user",
 Roles = Array.Empty<string>().AsReadOnly()
        };

        // Act & Assert
        Assert.Throws<AnalyticsAuthorizationException>(() => _service.EnsureCanExecuteQuery(user));
    }

    [Fact]
    public void EnsureCanExecuteQuery_WithMultipleRolesIncludingRequired_AllowsExecution()
    {
        // Arrange
        var user = new CurrentUser
        {
  UserId = "test-user",
            Roles = new[] { "OtherRole", "AnalyticsQueryExecutor", "AnotherRole" }.AsReadOnly()
        };

        // Act & Assert - should not throw
        _service.EnsureCanExecuteQuery(user);
    }
}
