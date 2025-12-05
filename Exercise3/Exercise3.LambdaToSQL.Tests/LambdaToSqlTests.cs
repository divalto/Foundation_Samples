namespace Exercise3.LambdaToSQL.Tests;

using System.Linq.Expressions;
using Exercise3.LambdaToSQL;

/// <summary>
/// Tests pour la traduction Lambda vers SQL
/// </summary>
public class LambdaToSqlTests
{
    private readonly ILambdaToSqlConverter _converter = new LambdaToSqlConverter();

    [Fact]
    public void ConvertSimpleEquality_ShouldGenerateCorrectSql()
    {
        // Arrange
        Expression<Func<User, bool>> predicate = u => u.Id == 5;

        // Act
        var query = _converter.ConvertWhere(predicate);

        // Assert
        Assert.Contains("FROM [User]", query.Sql);
        Assert.Contains("[Id]", query.Sql);
        Assert.Contains("=", query.Sql);
        Assert.Contains("5", query.Sql);
    }

    [Fact]
    public void ConvertGreaterThan_ShouldGenerateCorrectSql()
    {
        // Arrange
        Expression<Func<User, bool>> predicate = u => u.Age > 18;

        // Act
        var query = _converter.ConvertWhere(predicate);

        // Assert
        Assert.Contains("[Age]", query.Sql);
        Assert.Contains(">", query.Sql);
        Assert.Contains("18", query.Sql);
    }

    [Fact]
    public void ConvertLessThan_ShouldGenerateCorrectSql()
    {
        // Arrange
        Expression<Func<User, bool>> predicate = u => u.Age < 30;

        // Act
        var query = _converter.ConvertWhere(predicate);

        // Assert
        Assert.Contains("[Age]", query.Sql);
        Assert.Contains("<", query.Sql);
    }

    [Fact]
    public void ConvertNotEqual_ShouldGenerateCorrectSql()
    {
        // Arrange
        Expression<Func<User, bool>> predicate = u => u.Name != "John";

        // Act
        var query = _converter.ConvertWhere(predicate);

        // Assert
        Assert.Contains("[Name]", query.Sql);
        Assert.Contains("!=", query.Sql);
        Assert.Contains("John", query.Sql);
    }

    [Fact]
    public void ConvertAndCondition_ShouldGenerateBothConditions()
    {
        // Arrange
        Expression<Func<User, bool>> predicate = u => u.Age > 18 && u.IsActive;

        // Act
        var query = _converter.ConvertWhere(predicate);

        // Assert
        Assert.Contains("[Age]", query.Sql);
        Assert.Contains("[IsActive]", query.Sql);
        Assert.Contains("AND", query.Sql);
    }

    [Fact]
    public void ConvertOrCondition_ShouldGenerateBothConditions()
    {
        // Arrange
        Expression<Func<User, bool>> predicate = u => u.Id == 1 || u.Id == 2;

        // Act
        var query = _converter.ConvertWhere(predicate);

        // Assert
        Assert.Contains("[Id]", query.Sql);
        Assert.Contains("OR", query.Sql);
    }

    [Fact]
    public void ConvertComplexCondition_ShouldGenerateCorrectSql()
    {
        // Arrange
        Expression<Func<User, bool>> predicate = u => (u.Age > 18 && u.IsActive) || u.Name == "Admin";

        // Act
        var query = _converter.ConvertWhere(predicate);

        // Assert
        Assert.Contains("[Age]", query.Sql);
        Assert.Contains("[IsActive]", query.Sql);
        Assert.Contains("[Name]", query.Sql);
        Assert.Contains("AND", query.Sql);
        Assert.Contains("OR", query.Sql);
    }

    [Fact]
    public void BuildQuery_WithWhere_ShouldIncludeWhereClause()
    {
        // Arrange
        Expression<Func<User, bool>> where = u => u.Age >= 21;

        // Act
        var query = _converter.BuildQuery(where);

        // Assert
        Assert.Contains("SELECT *", query.Sql);
        Assert.Contains("FROM [User]", query.Sql);
        Assert.Contains("WHERE", query.Sql);
        Assert.Contains("[Age]", query.Sql);
        Assert.Contains(">=", query.Sql);
    }

    [Fact]
    public void BuildQuery_WithoutConditions_ShouldReturnSelectAll()
    {
        // Arrange & Act
        var query = _converter.BuildQuery<User>();

        // Assert
        Assert.Equal("SELECT * FROM [User]", query.Sql);
        Assert.Empty(query.Parameters);
    }

    [Fact]
    public void ConvertWithStringProperty_ShouldHandleCorrectly()
    {
        // Arrange
        Expression<Func<User, bool>> predicate = u => u.Email == "test@example.com";

        // Act
        var query = _converter.ConvertWhere(predicate);

        // Assert
        Assert.Contains("[Email]", query.Sql);
        Assert.Contains("test@example.com", query.Sql);
    }

    [Fact]
    public void ConvertWithBooleanProperty_ShouldHandleCorrectly()
    {
        // Arrange
        Expression<Func<User, bool>> predicate = u => u.IsActive == true;

        // Act
        var query = _converter.ConvertWhere(predicate);

        // Assert
        Assert.Contains("[IsActive]", query.Sql);
        Assert.Contains("=", query.Sql);
    }

    [Fact]
    public void ConvertWithDateProperty_ShouldHandleCorrectly()
    {
        // Arrange
        var cutoffDate = new DateTime(2024, 1, 1);
        Expression<Func<User, bool>> predicate = u => u.CreatedDate >= cutoffDate;

        // Act
        var query = _converter.ConvertWhere(predicate);

        // Assert
        Assert.Contains("[CreatedDate]", query.Sql);
        Assert.Contains(">=", query.Sql);
        Assert.Single(query.Parameters);
        Assert.Equal(cutoffDate, query.Parameters.First().Value);
    }

    [Fact]
    public void SqlQuery_ToString_ShouldDisplayBothSqlAndParameters()
    {
        // Arrange
        Expression<Func<User, bool>> predicate = u => u.Id == 42;

        // Act
        var query = _converter.ConvertWhere(predicate);
        var result = query.ToString();

        // Assert
        Assert.Contains("SQL:", result);
        Assert.Contains("Parameters:", result);
        Assert.Contains("[User]", result);
    }
}
