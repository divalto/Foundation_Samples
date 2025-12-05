namespace Exercise1.ExpressionCompiler;

/// <summary>
/// Exception lancée lorsqu'une erreur de compilation se produit
/// </summary>
public class CompilationException(string message, IReadOnlyList<string> errors) : Exception(message)
{
    public IReadOnlyList<string> Errors { get; } = errors;
}
