# Exercice 3 : Parser d'Expressions Lambda vers SQL

## 🎯 Objectif

Créer un convertisseur d'expressions lambda en requêtes SQL, permettant de traduire les prédicats LINQ en SQL natif.

## 📋 Ce que vous devez implémenter

Complétez les classes pour créer un parser lambda qui :

1. **Parse les expressions lambda** en arbre syntaxique
2. **Visite l'arbre** pour extraire les conditions
3. **Traduit les opérateurs** en SQL (==, !=, <, >, &&, ||)
4. **Paramétrise les valeurs** pour éviter les injections SQL
5. **Génère des requêtes** SELECT WHERE complètes

## 🧪 Tests à valider

Les tests suivants doivent passer :
- `ConvertSimpleEquality_ShouldGenerateCorrectSql`
- `ConvertGreaterThan_ShouldGenerateCorrectSql`
- `ConvertLessThan_ShouldGenerateCorrectSql`
- `ConvertNotEqual_ShouldGenerateCorrectSql`
- `ConvertAndCondition_ShouldGenerateBothConditions`
- `ConvertOrCondition_ShouldGenerateBothConditions`
- `ConvertComplexCondition_ShouldGenerateCorrectSql`
- `BuildQuery_WithWhere_ShouldIncludeWhereClause`
- `BuildQuery_WithoutConditions_ShouldReturnSelectAll`
- `ConvertMultipleParameters_ShouldHaveDifferentParameterNames`
- `ConvertWithStringProperty_ShouldHandleCorrectly`
- `ConvertWithBooleanProperty_ShouldHandleCorrectly`
- `ConvertWithDateProperty_ShouldHandleCorrectly`
- `SqlQuery_ToString_ShouldDisplayBothSqlAndParameters`

## 💡 Concepts Clés

### Expression Trees

Les expression trees (arbres d'expressions) permettent de représenter le code sous forme de données :

```csharp
// Lambda
Expression<Func<User, bool>> predicate = u => u.Age > 18;

// Est représentée comme :
// BinaryExpression (>)
//  ├─ MemberExpression (Age)
//  │   └─ ParameterExpression (u)
//  └─ ConstantExpression (18)
```

### ExpressionVisitor

La classe `ExpressionVisitor` permet de parcourir récursivement l'arbre d'expressions :

```csharp
public class SqlExpressionVisitor : ExpressionVisitor
{
    protected override Expression VisitBinary(BinaryExpression node)
    {
        // Traiter les opérateurs binaires (==, !=, <, >, etc.)
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        // Traiter les accès aux propriétés
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        // Traiter les constantes
    }
}
```

## 📝 À Compléter

### 1. `SqlExpressionVisitor.VisitBinary()`

Complétez la traduction des opérateurs :
```csharp
string? op = node.NodeType switch
{
    ExpressionType.Equal => "=",
    ExpressionType.NotEqual => "!=",
    ExpressionType.GreaterThan => ">",
    ExpressionType.GreaterThanOrEqual => ">=",
    ExpressionType.LessThan => "<",
    ExpressionType.LessThanOrEqual => "<=",
    // TODO: Ajouter AndAlso et OrElse
    _ => null
};
```

### 2. `SqlExpressionVisitor.VisitConstant()`

Paramétriser les valeurs constantes :
```csharp
protected override Expression VisitConstant(ConstantExpression node)
{
    if (node.Value == null)
    {
        _sqlStack.Push("NULL");
    }
    else
    {
        var paramName = $"@param{_parameterCounter++}";
        _parameters[paramName] = node.Value;
        _sqlStack.Push(paramName);
    }
    return node;
}
```

### 3. `LambdaToSqlConverter.ConvertWhere<T>()`

Construisez la requête SQL complète :
```csharp
public SqlQuery ConvertWhere<T>(Expression<Func<T, bool>> predicate) where T : class
{
    var visitor = new SqlExpressionVisitor();
    visitor.Visit(predicate);

    var tableName = typeof(T).Name;
    var sql = $"SELECT * FROM [{tableName}] WHERE {visitor.Sql}";
    
    return new SqlQuery { Sql = sql, Parameters = visitor.Parameters };
}
```

## 🔄 Flux de Traitement

```
Lambda Expression
    │
    ▼
SqlExpressionVisitor
    ├─ VisitBinary()  → Traduit les opérateurs
    ├─ VisitMember()  → Extrait les noms de colonnes
    ├─ VisitConstant() → Paramétrise les valeurs
    └─ Stack SQL
    │
    ▼
SqlQuery
    ├─ Sql: "SELECT * FROM [User] WHERE ([Age] > @param0) AND ([IsActive] = @param1)"
    └─ Parameters: { "@param0": 18, "@param1": true }
```

## 🚀 Lancer les tests

```bash
dotnet test --filter "FullyQualifiedName~Exercise5"
```

## Exemples de Résultats Attendus

### Exemple 1 : Simple équation
```csharp
u => u.Id == 5
// SQL: SELECT * FROM [User] WHERE ([Id] = @param0)
// Parameters: @param0 = 5
```

### Exemple 2 : Condition composite
```csharp
u => (u.Age > 18) && (u.IsActive == true)
// SQL: SELECT * FROM [User] WHERE (([Age] > @param0) AND ([IsActive] = @param1))
// Parameters: @param0 = 18, @param1 = true
```

### Exemple 3 : Condition OR
```csharp
u => u.Name == "Admin" || u.Id == 1
// SQL: SELECT * FROM [User] WHERE (([Name] = @param0) OR ([Id] = @param1))
// Parameters: @param0 = "Admin", @param1 = 1
```

## 🎓 Apprentissages

- Expression Trees et LINQ
- Pattern Visitor pour AST
- Traduction code → SQL
- Sécurité (paramétrage)
- Compilation dynamique d'expressions
