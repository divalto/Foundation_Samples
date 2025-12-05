namespace Exercise2.ServerlessPlugins;

/// <summary>
/// Interface que tous les plugins doivent implémenter
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Nom du plugin
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Version du plugin
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Exécute le plugin avec le contexte fourni
    /// </summary>
    /// <param name="context">Contexte d'exécution</param>
    /// <returns>Résultat de l'exécution</returns>
    Task<PluginResult> ExecuteAsync(PluginContext context);
}

/// <summary>
/// Contexte d'exécution pour les plugins
/// </summary>
public class PluginContext
{
    private readonly Dictionary<string, object> _data = [];

    /// <summary>
    /// Ajoute ou met à jour une valeur dans le contexte
    /// </summary>
    public void Set<T>(string key, T value) where T : notnull
    {
        _data[key] = value;
    }

    /// <summary>
    /// Récupère une valeur du contexte
    /// </summary>
    public T Get<T>(string key)
    {
        if (_data.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        throw new KeyNotFoundException($"Key '{key}' not found in context");
    }

    /// <summary>
    /// Tente de récupérer une valeur du contexte
    /// </summary>
    public bool TryGet<T>(string key, out T? value)
    {
        if (_data.TryGetValue(key, out var obj))
        {
            value = (T)obj;
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    /// Retourne toutes les clés du contexte
    /// </summary>
    public IEnumerable<string> Keys => _data.Keys;
}

/// <summary>
/// Résultat de l'exécution d'un plugin
/// </summary>
public class PluginResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = [];

    public static PluginResult Successful(string message = "", object? data = null)
    {
        return new PluginResult
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static PluginResult Failed(string message)
    {
        return new PluginResult
        {
            Success = false,
            Message = message
        };
    }
}
