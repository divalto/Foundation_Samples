using System.Linq.Expressions;

namespace Exercise3.LambdaToSQL;

/// <summary>
/// Visiteur d'expression pour traduire les lambda en SQL
/// </summary>
internal class SqlExpressionVisitor : ExpressionVisitor
{
    private readonly Stack<string> _sqlStack = new();
    private readonly Dictionary<string, object?> _parameters = new();
    private int _parameterCounter = 0;

    public string Sql => string.Join(" ", _sqlStack.Reverse());
    public Dictionary<string, object?> Parameters => _parameters;

    /// <summary>
    /// Visite une expression binaire (==, !=, <, >, <=, >=, &&, ||)
    /// PARTIELLEMENT IMPLÉMENTÉ - À COMPLÉTER
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression node)
    {
        Visit(node.Left);
        var left = _sqlStack.Pop();

        Visit(node.Right);
        var right = _sqlStack.Pop();

        string? op = node.NodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "!=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            // TODO: Ajouter les autres opérateurs
            _ => null
        };

        if (op == null)
            throw new NotSupportedException($"Operator {node.NodeType} not supported");

        _sqlStack.Push($"({left} {op} {right})");
        return node;
    }

    /// <summary>
    /// Visite une expression membre (accès à une propriété)
    /// </summary>
    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression is ParameterExpression)
        {
            _sqlStack.Push($"[{node.Member.Name}]");
        }
        else
        {
            // TODO: Gérer les propriétés sous forme de paramètres sql
            throw new NotSupportedException("Parameters not supported");
        }
        return node;
    }

    /// <summary>
    /// Visite une constante
    /// PARTIELLEMENT IMPLÉMENTÉ - À COMPLÉTER
    /// </summary>
    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (node.Value == null)
        {
            _sqlStack.Push("NULL");
        }
        else
        {
            //TODO gérer les types de données
        }
        return node;
    }

    /// <summary>
    /// Visite une expression lambda
    /// IMPLÉMENTÉ
    /// </summary>
    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        Visit(node.Body);
        return node;
    }
}

/// <summary>
/// Convertisseur lambda vers SQL
/// PARTIELLEMENT IMPLÉMENTÉ - À COMPLÉTER
/// </summary>
public class LambdaToSqlConverter : ILambdaToSqlConverter
{
    /// <summary>
    /// Convertit une expression WHERE en SQL
    /// PARTIELLEMENT IMPLÉMENTÉ - À COMPLÉTER
    /// </summary>
    public SqlQuery ConvertWhere<T>(Expression<Func<T, bool>> predicate) where T : class
    {
        var visitor = new SqlExpressionVisitor();
        visitor.Visit(predicate);

        // TODO: Construire la requête SQL complète
        
        throw new NotImplementedException("Complétez le TODO ci-dessus");
    }

    /// <summary>
    /// Convertit une expression SELECT en SQL
    /// PARTIELLEMENT IMPLÉMENTÉ - À COMPLÉTER
    /// </summary>
    public SqlQuery ConvertSelect<TSource, TResult>(
        Expression<Func<TSource, TResult>> projection) 
        where TSource : class
    {
        // TODO: Analyser l'expression de projection

        throw new NotImplementedException("Complétez le TODO ci-dessus");
    }

    /// <summary>
    /// Construit une requête complète
    /// IMPLÉMENTÉ
    /// </summary>
    public SqlQuery BuildQuery<T>(
        Expression<Func<T, bool>>? where = null,
        Expression<Func<T, T>>? select = null) 
        where T : class
    {
        var tableName = typeof(T).Name;
        var sql = $"SELECT * FROM [{tableName}]";
        var parameters = new Dictionary<string, object?>();

        if (where != null)
        {
            var whereQuery = ConvertWhere(where);
            // Extraire juste la condition (sans SELECT * FROM ...)
            var whereCondition = ExtractWhereCondition(whereQuery.Sql);
            sql += $" WHERE {whereCondition}";
            foreach (var param in whereQuery.Parameters)
            {
                parameters[param.Key] = param.Value;
            }
        }

        return new SqlQuery { Sql = sql, Parameters = parameters };
    }

    private string ExtractWhereCondition(string fullSql)
    {
        // Extraire la partie WHERE de la requête complète
        var parts = fullSql.Split("WHERE", StringSplitOptions.None);
        return parts.Length > 1 ? parts[1].Trim() : "";
    }
}
