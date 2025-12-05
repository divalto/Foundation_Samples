namespace Exercise2.ServerlessPlugins;

/// <summary>
/// Exception lancée lorsqu'une erreur de compilation se produit
/// </summary>
public class PluginCompilationException(string message, IReadOnlyList<string> errors) : Exception(message)
{
    public IReadOnlyList<string> Errors { get; } = errors;
}

/// <summary>
/// Exception lancée lorsqu'une erreur de chargement se produit
/// </summary>
public class PluginLoadException : Exception
{
    public PluginLoadException(string message) : base(message) { }
    public PluginLoadException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// Exception lancée lorsqu'une erreur d'exécution se produit
/// </summary>
public class PluginExecutionException : Exception
{
    public string PluginName { get; }

    public PluginExecutionException(string pluginName, string message) 
        : base(message)
    {
        PluginName = pluginName;
    }

    public PluginExecutionException(string pluginName, string message, Exception innerException) 
        : base(message, innerException)
    {
        PluginName = pluginName;
    }
}
