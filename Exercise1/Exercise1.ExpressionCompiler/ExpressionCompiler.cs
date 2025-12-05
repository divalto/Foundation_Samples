using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;

namespace Exercise1.ExpressionCompiler;

/// <summary>
/// Compilateur d'expressions C# dynamiques
/// Partiellement implémenté - À COMPLÉTER
/// </summary>
public class ExpressionCompiler
{
    /// <summary>
    /// Compile et évalue une expression mathématique simple
    /// </summary>
    /// <param name="expression">L'expression C# à évaluer (ex: "2 + 2")</param>
    /// <returns>Le résultat de l'évaluation</returns>
    public static double Evaluate(string expression)
    {
        // Déléguer à la version avec variables (sans variables)
        return Evaluate(expression, []);
    }

    /// <summary>
    /// Compile et évalue une expression avec des variables
    /// PARTIELLEMENT IMPLÉMENTÉ - À COMPLÉTER
    /// </summary>
    /// <param name="expression">L'expression C# utilisant des variables (ex: "x * 2")</param>
    /// <param name="variables">Dictionnaire des variables et leurs valeurs</param>
    /// <returns>Le résultat de l'évaluation</returns>
    public static double Evaluate(string expression, Dictionary<string, double> variables)
    {
        // Générer les paramètres pour les variables
        var parameters = string.Join(", ", 
            variables.Select(v => $"double {v.Key}"));
        
        // Créer le code source complet
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

        // Parser le code en SyntaxTree
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        // Créer les références d'assemblies
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Math).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
        };

        // TODO: 
        // - Créer la compilation avec CSharpCompilation.Create
        // - Émettre l'assembly en mémoire avec compilation.Emit(ms)
        // - Vérifier les erreurs de compilation
        // - Charger l'assembly avec Assembly.Load(ms.ToArray())
        // - Obtenir le type et la méthode via reflection sur l'assembly générée
        // - Préparer les arguments et invoquer
        
        throw new NotImplementedException("Complétez les TODO ci-dessus");
    }
}
