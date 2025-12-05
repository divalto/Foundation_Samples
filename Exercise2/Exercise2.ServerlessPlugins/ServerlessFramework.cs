using System.Runtime.Loader;
using System.Collections.Concurrent;

namespace Exercise2.ServerlessPlugins;

/// <summary>
/// Framework serverless permettant la compilation et l'exécution de plugins dynamiques
/// Partiellement implémenté - À COMPLÉTER
/// </summary>
public class ServerlessFramework : IDisposable
{
    // Conteneur interne pour stocker un plugin avec son contexte
    private class PluginContainer
    {
        public required IPlugin Plugin { get; init; }
        public AssemblyLoadContext? LoadContext { get; init; }
        public WeakReference? WeakRef { get; set; }
    }

    // Dictionnaires pour gérer les plugins et leurs informations
    // TODO: Ces dictionnaires sont utilisés par LoadPlugin, GetPluginInfo, etc.
    private readonly ConcurrentDictionary<string, PluginContainer> _plugins = new();
    private readonly ConcurrentDictionary<string, PluginInfo> _pluginInfos = new();

    /// <summary>
    /// Compile un plugin à partir de son code source
    /// PARTIELLEMENT IMPLÉMENTÉ - À COMPLÉTER
    /// </summary>
    /// <param name="pluginCode">Code source du plugin (doit implémenter IPlugin)</param>
    /// <param name="pluginName">Nom unique du plugin</param>
    /// <returns>Instance du plugin compilé</returns>
    public static IPlugin CompilePlugin(string pluginCode, string pluginName)
    {
        // Étape 1 : Ajouter les using nécessaires au code
        var code = 
        $"""
            using System;
            using System.Threading.Tasks;
            using Exercise2.ServerlessPlugins;
            
            {pluginCode}
        """;

        // TODO: Étape 2 - Parser le code avec CSharpSyntaxTree.ParseText

        // TODO: Étape 3 - Définir les références d'assemblies nécessaires

        // TODO: Étape 4 - Créer la compilation avec CSharpCompilation.Create

        // TODO: Étape 5 - Émettre en mémoire et vérifier les erreurs
        // Si échec, lancer PluginCompilationException

        // TODO: Étape 6 - Charger l'assembly avec Assembly.Load

        // TODO: Étape 7 - Trouver la classe qui implémente IPlugin

        // TODO: Étape 8 - Créer et retourner une instance
        // Si aucune classe trouvée, lancer PluginLoadException
        
        throw new NotImplementedException("Complétez les TODO ci-dessus");
    }

    /// <summary>
    /// Charge un plugin déjà compilé
    /// IMPLÉMENTATION FOURNIE - Déjà complète
    /// </summary>
    /// <param name="plugin">Instance du plugin</param>
    /// <param name="isolate">Si true, charge dans un contexte isolé</param>
    public void LoadPlugin(IPlugin plugin, bool isolate = true)
    {
        AssemblyLoadContext? loadContext = null;

        if (isolate)
        {
            loadContext = new AssemblyLoadContext(
                $"PluginContext_{plugin.Name}", 
                isCollectible: true);
        }

        // TODO: Créer un PluginContainer et stocker dans _plugins
        // TODO: Créer un PluginInfo et stocker dans _pluginInfos
        
        throw new NotImplementedException("Complétez le stockage du plugin");
    }

    /// <summary>
    /// Exécute un plugin par son nom
    /// PARTIELLEMENT IMPLÉMENTÉ - À COMPLÉTER
    /// </summary>
    /// <param name="pluginName">Nom du plugin à exécuter</param>
    /// <param name="context">Contexte d'exécution</param>
    /// <returns>Résultat de l'exécution</returns>
    public async Task<PluginResult> ExecutePluginAsync(string pluginName, PluginContext context)
    {
        if (!_plugins.TryGetValue(pluginName, out var container))
        {
            throw new PluginExecutionException(
                pluginName, 
                $"Plugin '{pluginName}' not found");
        }

        try
        {
            // TODO: Exécuter le plugin

            // TODO: Mettre à jour LastExecutedAt dans _pluginInfos

            // TODO: Retourner le résultat
        }
        catch (Exception ex)
        {
            throw new PluginExecutionException(
                pluginName, 
                $"Plugin '{pluginName}' execution failed", 
                ex);
        }
        
        throw new NotImplementedException("Complétez les TODO ci-dessus");
    }

    /// <summary>
    /// Décharge un plugin de la mémoire
    /// PARTIELLEMENT IMPLÉMENTÉ - À COMPLÉTER
    /// </summary>
    /// <param name="pluginName">Nom du plugin à décharger</param>
    /// <returns>True si le plugin a été déchargé avec succès</returns>
    public bool UnloadPlugin(string pluginName)
    {
        // TODO: Retirer le plugin du dictionnaire

        // TODO: Si le contexte existe, le décharger

        // TODO: Retirer aussi les infos du plugin
        
        throw new NotImplementedException("Complétez les TODO ci-dessus");
    }

    /// <summary>
    /// Recharge un plugin avec un nouveau code (hot-reload)
    /// PARTIELLEMENT IMPLÉMENTÉ - À COMPLÉTER
    /// </summary>
    /// <param name="pluginCode">Nouveau code source du plugin</param>
    /// <param name="pluginName">Nom du plugin à recharger</param>
    /// <returns>Instance du nouveau plugin</returns>
    public IPlugin ReloadPlugin(string pluginCode, string pluginName)
    {
        // TODO: Si le plugin existe déjà
        if (_plugins.ContainsKey(pluginName))
        {
            // TODO: Sauvegarder l'ancien LoadCount
            
            // TODO: Décharger l'ancien plugin

            // TODO: Compiler et charger le nouveau plugin

            // TODO: Incrémenter le LoadCount

            // TODO: Retourner le nouveau plugin
        }
        else
        {
            // Première fois : compiler et charger normalement
            var plugin = CompilePlugin(pluginCode, pluginName);
            LoadPlugin(plugin);
            return plugin;
        }
        
        throw new NotImplementedException("Complétez les TODO ci-dessus");
    }

    /// <summary>
    /// Obtient les informations sur un plugin
    /// </summary>
    /// <param name="pluginName">Nom du plugin</param>
    /// <returns>Informations sur le plugin, ou null s'il n'existe pas</returns>
    public PluginInfo? GetPluginInfo(string pluginName)
    {
        return _pluginInfos.TryGetValue(pluginName, out var info) ? info : null;
    }

    /// <summary>
    /// Liste tous les plugins chargés
    /// </summary>
    /// <returns>Collection des noms de plugins chargés</returns>
    public IEnumerable<string> GetLoadedPlugins()
    {
        return _plugins.Keys;
    }

    /// <summary>
    /// Vérifie si un plugin est chargé
    /// </summary>
    /// <param name="pluginName">Nom du plugin</param>
    /// <returns>True si le plugin est chargé</returns>
    public bool IsPluginLoaded(string pluginName)
    {
        return _plugins.ContainsKey(pluginName);
    }

    public void Dispose()
    {
        // TODO: Implémenter le nettoyage des ressources
    }
}
