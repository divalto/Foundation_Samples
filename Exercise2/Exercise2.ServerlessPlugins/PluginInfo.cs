namespace Exercise2.ServerlessPlugins;

/// <summary>
/// Informations sur un plugin chargé
/// </summary>
public class PluginInfo
{
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required int LoadCount { get; set; }
    public required DateTime LoadedAt { get; init; }
    public DateTime? LastExecutedAt { get; set; }
}
