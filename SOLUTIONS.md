# Solutions des Exercices

Ce fichier regroupe les solutions complètes pour chaque exercice du dépôt.

---

## Exercise 1 : Compilation d'expressions C# dynamiques

### Solution - `ExpressionCompiler.cs`

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;

namespace Exercise1.ExpressionCompiler;

public class ExpressionCompiler
{
    public static double Evaluate(string expression)
    {
        return Evaluate(expression, []);
    }

    public static double Evaluate(string expression, Dictionary<string, double> variables)
    {
        // Étape 1 : Générer les paramètres pour les variables
        var parameters = string.Join(", ", 
            variables.Select(v => $"double {v.Key}"));
        
        // Étape 2 : Créer le code source complet
        var code = $@"
            using System;
            
            public class ExpressionEvaluator
            {{
                public static double Evaluate({parameters})
                {{
                    return {expression};
                }}
            }}
        ";

        // Étape 3 : Parser le code en SyntaxTree
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        // Étape 4 : Créer les références d'assemblies
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Math).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
        };

        // Étape 5 : Créer la compilation
        var compilation = CSharpCompilation.Create("ExpressionAssembly")
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .WithReferences(references)
            .AddSyntaxTrees(syntaxTree);

        // Étape 6 : Émettre l'assembly en mémoire
        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        // Étape 7 : Vérifier les erreurs de compilation
        if (!result.Success)
        {
            var errors = result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.GetMessage())
                .ToList();
            
            throw new CompilationException("Compilation failed", errors);
        }

        // Étape 8 : Charger l'assembly
        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());

        // Étape 9 : Obtenir le type et la méthode
        var type = assembly.GetType("ExpressionEvaluator")!;
        var method = type.GetMethod("Evaluate")!;

        // Étape 10 : Préparer les arguments et invoquer
        var args = variables.Values.Cast<object>().ToArray();
        return (double)method.Invoke(null, args)!;
    }
}
```

---

## Exercise 2 : Plugins Serverless

### Solution - `ServerlessFramework.cs`

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.Loader;
using System.Collections.Concurrent;
using System.Reflection;

namespace Exercise3.ServerlessPlugins;

public class ServerlessFramework : IDisposable
{
    private class PluginContainer
    {
        public required IPlugin Plugin { get; init; }
        public AssemblyLoadContext? LoadContext { get; init; }
        public WeakReference? WeakRef { get; set; }
    }

    private readonly ConcurrentDictionary<string, PluginContainer> _plugins = new();
    private readonly ConcurrentDictionary<string, PluginInfo> _pluginInfos = new();

    public static IPlugin CompilePlugin(string pluginCode, string pluginName)
    {
        var code = $@"
            using System;
            using System.Threading.Tasks;
            
            {pluginCode}
        ";

        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IPlugin).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Linq").Location)
        };

        var compilation = CSharpCompilation.Create(
            $"PluginAssembly_{Guid.NewGuid()}",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var errors = result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.GetMessage())
                .ToList();
            
            throw new PluginCompilationException($"Plugin '{pluginName}' compilation failed", errors);
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());

        var pluginType = assembly.GetTypes()
            .FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface);

        if (pluginType == null)
            throw new PluginLoadException($"Plugin '{pluginName}' does not implement IPlugin");

        var plugin = (IPlugin)Activator.CreateInstance(pluginType)!;
        return plugin;
    }

    public void LoadPlugin(IPlugin plugin, bool isolate = true)
    {
        AssemblyLoadContext? loadContext = null;

        if (isolate)
        {
            loadContext = new AssemblyLoadContext(
                $"PluginContext_{plugin.Name}", 
                isCollectible: true);
        }

        var container = new PluginContainer { Plugin = plugin, LoadContext = loadContext };
        _plugins[plugin.Name] = container;

        var pluginInfo = new PluginInfo
        {
            Name = plugin.Name,
            Version = plugin.Version,
            LoadCount = 1,
            LoadedAt = DateTime.UtcNow
        };
        _pluginInfos[plugin.Name] = pluginInfo;
    }

    public async Task<PluginResult> ExecutePluginAsync(string pluginName, PluginContext context)
    {
        if (!_plugins.TryGetValue(pluginName, out var container))
            throw new PluginExecutionException(pluginName, $"Plugin '{pluginName}' not loaded");

        try
        {
            var result = await container.Plugin.ExecuteAsync(context);

            if (_pluginInfos.TryGetValue(pluginName, out var info))
            {
                info.LastExecutedAt = DateTime.UtcNow;
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new PluginExecutionException(
                pluginName, 
                $"Plugin '{pluginName}' execution failed", 
                ex);
        }
    }

    public bool UnloadPlugin(string pluginName)
    {
        if (!_plugins.TryRemove(pluginName, out var container))
            return false;

        if (container.LoadContext != null)
        {
            var weakRef = new WeakReference(container.LoadContext, trackResurrection: true);
            container.WeakRef = weakRef;
            container.LoadContext.Unload();

            for (int i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        _pluginInfos.TryRemove(pluginName, out _);
        return true;
    }

    public IPlugin ReloadPlugin(string pluginCode, string pluginName)
    {
        if (_plugins.ContainsKey(pluginName))
        {
            var oldInfo = _pluginInfos[pluginName];
            UnloadPlugin(pluginName);

            var newPlugin = CompilePlugin(pluginCode, pluginName);
            LoadPlugin(newPlugin);

            if (_pluginInfos.TryGetValue(pluginName, out var newInfo))
            {
                newInfo.LoadCount = oldInfo.LoadCount + 1;
            }

            return newPlugin;
        }
        else
        {
            var plugin = CompilePlugin(pluginCode, pluginName);
            LoadPlugin(plugin);
            return plugin;
        }
    }

    public PluginInfo? GetPluginInfo(string pluginName)
    {
        return _pluginInfos.TryGetValue(pluginName, out var info) ? info : null;
    }

    public IEnumerable<string> GetLoadedPlugins() => _plugins.Keys;

    public bool IsPluginLoaded(string pluginName) => _plugins.ContainsKey(pluginName);

    public void Dispose()
    {
        foreach (var pluginName in _plugins.Keys.ToList())
        {
            UnloadPlugin(pluginName);
        }
        _plugins.Clear();
        _pluginInfos.Clear();
    }
}
```

---

## Exercise 3 : Conversion de lambda en SQL

### Solution - `LambdaToSqlConverter.cs`

```csharp
using System.Linq.Expressions;

namespace Exercise3.LambdaToSQL;

/// <summary>
/// Visiteur d'expression pour traduire les lambda en SQL
/// </summary>
public class SqlExpressionVisitor : ExpressionVisitor
{
    private readonly Stack<string> _sqlStack = new();

    // Non géré dans cet exemple
    private readonly Dictionary<string, object?> _parameters = new();

    public string Sql => string.Join(" ", _sqlStack.Reverse());
    public Dictionary<string, object?> Parameters => _parameters;

    protected override Expression VisitBinary(BinaryExpression node)
    {
        Visit(node.Left);
        var left = _sqlStack.Pop();

        Visit(node.Right);
        var right = _sqlStack.Pop();

        string? op = node.NodeType switch
        {
            ExpressionType.Add => "+",
            ExpressionType.Subtract => "-",
            ExpressionType.Multiply => "*",
            ExpressionType.Divide => "/",
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "!=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            _ => null
        };

        if (op == null)
            throw new NotSupportedException($"Operator {node.NodeType} not supported");

        _sqlStack.Push($"({left} {op} {right})");
        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression is ParameterExpression)
        {
            _sqlStack.Push($"[{node.Member.Name}]");
        }
        else
        {
            var value = Expression.Lambda(node).Compile().DynamicInvoke();
            if(value is null)
            {
                _sqlStack.Push("NULL");
            }
            else
            {
                _parameters.Add($"param{_parameters.Count}", value);
                _sqlStack.Push($"@param{_parameters.Count - 1}");
            }
        }
        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (node.Value == null)
        {
            _sqlStack.Push("NULL");
        }
        else if(node.Value is string strValue)
        {
            _sqlStack.Push($"'{strValue}'");
        }
        else
        {
            _sqlStack.Push(node.Value.ToString()!);
        }
        return node;
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        Visit(node.Body);
        return node;
    }
}

public class LambdaToSqlConverter : ILambdaToSqlConverter
{
    public SqlQuery ConvertWhere<T>(Expression<Func<T, bool>> predicate) where T : class
    {
        var visitor = new SqlExpressionVisitor();
        visitor.Visit(predicate);

        var tableName = typeof(T).Name;
        var sql = $"SELECT * FROM [{tableName}] WHERE {visitor.Sql}";

        return new SqlQuery { Sql = sql, Parameters = visitor.Parameters };
    }

    public SqlQuery ConvertSelect<TSource, TResult>(
        Expression<Func<TSource, TResult>> projection) 
        where TSource : class
    {
        var tableName = typeof(TSource).Name;
        string columns = "*";

        if (projection.Body is NewExpression newExpr)
        {
            var props = newExpr.Arguments
                .OfType<MemberExpression>()
                .Select(m => $"[{m.Member.Name}]");
            columns = string.Join(", ", props);
        }
        else if (projection.Body is MemberExpression memberExpr)
        {
            columns = $"[{memberExpr.Member.Name}]";
        }

        var sql = $"SELECT {columns} FROM [{tableName}]";
        return new SqlQuery { Sql = sql, Parameters = new Dictionary<string, object?>() };
    }

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
        var parts = fullSql.Split(new[] { "WHERE" }, StringSplitOptions.None);
        return parts.Length > 1 ? parts[1].Trim() : "";
    }
}
```

---

## Notes importantes

- Chaque exercice contient des **TODO** commentés dans le code source pour vous guider vers la solution
- Les fichiers de tests unitaires dans les dossiers `*.Tests` vous permettent de valider votre implémentation
- Utilisez `dotnet test` pour exécuter les tests et vérifier que votre solution fonctionne correctement
- Les solutions fournies ici sont des implémentations complètes et fonctionnelles