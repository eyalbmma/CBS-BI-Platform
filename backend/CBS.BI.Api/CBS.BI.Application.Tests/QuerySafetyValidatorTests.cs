using CBS.BI.Application.Analytics.Exceptions;
using CBS.BI.Application.Analytics.Validation;
using Xunit;

namespace CBS.BI.Application.Tests;

public class QuerySafetyValidatorTests
{
    private readonly ReadOnlyAnalyticsQuerySafetyValidator _validator = new();

    [Fact]
    public void EnsureSafe_SelectUpperCase_AllowsQuery()
    {
        // Arrange
        var query = "SELECT City FROM table";

        // Act & Assert
        _validator.EnsureSafe(query);
    }

    [Fact]
    public void EnsureSafe_SelectLowerCase_AllowsQuery()
    {
  // Arrange
        var query = "select City from table";

        // Act & Assert
        _validator.EnsureSafe(query);
  }

    [Fact]
    public void EnsureSafe_WithClause_AllowsQuery()
    {
        // Arrange
    var query = @"WITH data AS (
SELECT City FROM table
)
SELECT * FROM data";

 // Act & Assert
  _validator.EnsureSafe(query);
    }

    [Fact]
    public void EnsureSafe_DeleteStatement_ThrowsException()
    {
        // Arrange
    var query = "DELETE FROM table";

        // Act & Assert
        Assert.Throws<UnsafeAnalyticsQueryException>(() => _validator.EnsureSafe(query));
    }

    [Fact]
    public void EnsureSafe_UpdateStatement_ThrowsException()
    {
        // Arrange
        var query = "UPDATE table SET City = 'X'";

   // Act & Assert
        Assert.Throws<UnsafeAnalyticsQueryException>(() => _validator.EnsureSafe(query));
    }

    [Fact]
    public void EnsureSafe_DropStatement_ThrowsException()
 {
        // Arrange
        var query = "DROP TABLE table";

        // Act & Assert
        Assert.Throws<UnsafeAnalyticsQueryException>(() => _validator.EnsureSafe(query));
    }

    [Fact]
    public void EnsureSafe_ColumnNameWithUpdate_AllowsQuery()
    {
// Arrange
      var query = "SELECT UpdatedAt FROM table";

        // Act & Assert
        _validator.EnsureSafe(query);
    }

    [Fact]
    public void EnsureSafe_ColumnNameWithCreate_AllowsQuery()
    {
    // Arrange
        var query = "SELECT CreatedAt FROM table";

    // Act & Assert
  _validator.EnsureSafe(query);
    }

    [Fact]
    public void EnsureSafe_KeywordInStringLiteral_AllowsQuery()
    {
        // Arrange
        var query = @"SELECT *
FROM table
WHERE Description = 'DELETE'";

 // Act & Assert
        _validator.EnsureSafe(query);
    }

    [Fact]
    public void EnsureSafe_KeywordInQuotedString_AllowsQuery()
    {
        // Arrange
    var query = "SELECT '-- DROP TABLE x' AS Example";

   // Act & Assert
        _validator.EnsureSafe(query);
    }

  [Fact]
    public void EnsureSafe_KeywordInLineComment_AllowsQuery()
 {
        // Arrange
        var query = @"SELECT *
FROM table
-- DELETE FROM other_table";

      // Act & Assert
        _validator.EnsureSafe(query);
    }

    [Fact]
    public void EnsureSafe_KeywordInBlockComment_AllowsQuery()
    {
        // Arrange
        var query = @"SELECT *
FROM table
/* UPDATE other_table SET X = 1 */";

        // Act & Assert
        _validator.EnsureSafe(query);
    }

    [Fact]
 public void EnsureSafe_CommentBeforeQuery_AllowsQuery()
    {
        // Arrange
        var query = @"-- comment before query
SELECT City
FROM table";

        // Act & Assert
    _validator.EnsureSafe(query);
    }

  [Fact]
    public void EnsureSafe_BacktickQuotedIdentifier_AllowsQuery()
    {
        // Arrange
        var query = "SELECT `DROP` FROM table";

        // Act & Assert
   _validator.EnsureSafe(query);
    }

    [Fact]
    public void EnsureSafe_MultipleStatementsWithDrop_ThrowsException()
    {
        // Arrange
     var query = @"SELECT City FROM table;
DROP TABLE table";

        // Act & Assert
        Assert.Throws<UnsafeAnalyticsQueryException>(() => _validator.EnsureSafe(query));
    }

    [Fact]
    public void EnsureSafe_InsertStatement_ThrowsException()
    {
        // Arrange
        var query = "INSERT INTO table VALUES (1)";

        // Act & Assert
        Assert.Throws<UnsafeAnalyticsQueryException>(() => _validator.EnsureSafe(query));
    }

    [Fact]
    public void EnsureSafe_MergeStatement_ThrowsException()
    {
        // Arrange
        var query = "MERGE INTO table USING source ON target.id = source.id";

   // Act & Assert
        Assert.Throws<UnsafeAnalyticsQueryException>(() => _validator.EnsureSafe(query));
    }

    [Fact]
    public void EnsureSafe_CreateStatement_ThrowsException()
    {
        // Arrange
        var query = "CREATE TABLE example (id INT)";

        // Act & Assert
        Assert.Throws<UnsafeAnalyticsQueryException>(() => _validator.EnsureSafe(query));
    }

    [Fact]
    public void EnsureSafe_AlterStatement_ThrowsException()
    {
        // Arrange
        var query = "ALTER TABLE example ADD COLUMN name VARCHAR(100)";

        // Act & Assert
     Assert.Throws<UnsafeAnalyticsQueryException>(() => _validator.EnsureSafe(query));
    }

    [Fact]
    public void EnsureSafe_TruncateStatement_ThrowsException()
    {
      // Arrange
        var query = "TRUNCATE TABLE example";

        // Act & Assert
      Assert.Throws<UnsafeAnalyticsQueryException>(() => _validator.EnsureSafe(query));
    }
}
