# Exercice 1 : Compilateur Simple d'Expressions

## 🎯 Objectif

Créer un compilateur capable d'évaluer des expressions mathématiques écrites en C# de manière dynamique.

## 📋 Ce que vous devez implémenter

Complétez la classe `ExpressionCompiler` pour qu'elle puisse :

1. Compiler une expression C# simple (ex: `"2 + 2"`)
2. Évaluer l'expression et retourner le résultat
3. Compiler des expressions avec variables (ex: `"x * 2"` où x est fourni)
4. Gérer les erreurs de compilation
5. Supporter les fonctions mathématiques (Math.Sqrt, Math.Pow, etc.)

## 🧪 Tests à valider

Les tests suivants doivent passer :
- `SimpleAddition_ShouldReturnCorrectResult`
- `ExpressionWithMultipleOperations_ShouldWork`
- `ExpressionWithVariable_ShouldUseProvidedValue`
- `ExpressionWithMultipleVariables_ShouldWork`
- `MathFunctions_ShouldBeSupported`
- `InvalidExpression_ShouldThrowCompilationException`

## 💡 Indices

- Utilisez `Microsoft.CodeAnalysis.CSharp` (Roslyn)
- Créez un assembly en mémoire
- Wrappez l'expression dans une méthode
- Pour les variables, utilisez des paramètres de méthode

## 🚀 Lancer les tests

```bash
dotnet test
```
