namespace Exercise3.LambdaToSQL;

/// <summary>
/// Représente une entité simple pour les tests
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Résultat de la traduction d'une expression lambda en SQL
/// </summary>
public class SqlQuery
{
    public required string Sql { get; init; }
    public required Dictionary<string, object?> Parameters { get; init; }

    public override string ToString()
    {
        var paramStr = string.Join(", ", Parameters.Select(p => $"{p.Key}={p.Value}"));
        return $"SQL: {Sql}\nParameters: {paramStr}";
    }
}

/// <summary>
/// Convertisseur d'expressions lambda en requêtes SQL
/// </summary>
public interface ILambdaToSqlConverter
{
    /// <summary>
    /// Convertit une expression lambda de filtrage en requête SQL WHERE
    /// </summary>
    SqlQuery ConvertWhere<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class;

    /// <summary>
    /// Convertit une expression lambda de projection en requête SQL SELECT
    /// </summary>
    SqlQuery ConvertSelect<TSource, TResult>(
        System.Linq.Expressions.Expression<Func<TSource, TResult>> projection) 
        where TSource : class;

    /// <summary>
    /// Combine un WHERE et un SELECT pour une requête complète
    /// </summary>
    SqlQuery BuildQuery<T>(
        System.Linq.Expressions.Expression<Func<T, bool>>? where = null,
        System.Linq.Expressions.Expression<Func<T, T>>? select = null) 
        where T : class;
}
