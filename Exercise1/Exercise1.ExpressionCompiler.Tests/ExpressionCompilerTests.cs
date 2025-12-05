namespace Exercise1.ExpressionCompiler.Tests;

/// <summary>
/// Tests pour le compilateur d'expressions
/// Ces tests définissent le comportement attendu - votre implémentation doit les faire passer
/// </summary>
public class ExpressionCompilerTests
{
    private readonly ExpressionCompiler _compiler = new();

    [Fact]
    public void SimpleAddition_ShouldReturnCorrectResult()
    {
        // Arrange
        var expression = "2 + 2";

        // Act
        var result = ExpressionCompiler.Evaluate(expression);

        // Assert
        Assert.Equal(4, result);
    }

    [Fact]
    public void ExpressionWithMultipleOperations_ShouldWork()
    {
        // Arrange
        var expression = "(10 + 5) * 2 - 3";

        // Act
        var result = ExpressionCompiler.Evaluate(expression);

        // Assert
        Assert.Equal(27, result);
    }

    [Fact]
    public void ExpressionWithVariable_ShouldUseProvidedValue()
    {
        // Arrange
        var expression = "x * 2";
        var variables = new Dictionary<string, double> { { "x", 5 } };

        // Act
        var result = ExpressionCompiler.Evaluate(expression, variables);

        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public void ExpressionWithMultipleVariables_ShouldWork()
    {
        // Arrange
        var expression = "x + y * z";
        var variables = new Dictionary<string, double>
        {
            { "x", 10 },
            { "y", 5 },
            { "z", 2 }
        };

        // Act
        var result = ExpressionCompiler.Evaluate(expression, variables);

        // Assert
        Assert.Equal(20, result); // 10 + (5 * 2) = 20
    }

    [Fact]
    public void MathFunctions_ShouldBeSupported()
    {
        // Arrange
        var expression = "Math.Sqrt(16) + Math.Pow(2, 3)";

        // Act
        var result = ExpressionCompiler.Evaluate(expression);

        // Assert
        Assert.Equal(12, result); // 4 + 8 = 12
    }

    [Fact]
    public void InvalidExpression_ShouldThrowCompilationException()
    {
        // Arrange
        var expression = "2 $ 2"; // Expression invalide

        // Act & Assert
        var exception = Assert.Throws<CompilationException>(() => ExpressionCompiler.Evaluate(expression));
        Assert.NotEmpty(exception.Errors);
    }

    [Fact]
    public void DecimalNumbers_ShouldBeSupported()
    {
        // Arrange
        var expression = "3.14 * 2";

        // Act
        var result = ExpressionCompiler.Evaluate(expression);

        // Assert
        Assert.Equal(6.28, result, precision: 2);
    }

    [Fact]
    public void ComplexExpression_WithVariablesAndFunctions_ShouldWork()
    {
        // Arrange
        var expression = "Math.Sqrt(x) + Math.Pow(y, 2)";
        var variables = new Dictionary<string, double>
        {
            { "x", 25 },
            { "y", 3 }
        };

        // Act
        var result = ExpressionCompiler.Evaluate(expression, variables);

        // Assert
        Assert.Equal(14, result); // 5 + 9 = 14
    }
}
