using Exercise2.ServerlessPlugins;

namespace Exercise2.ServerlessPlugins.Tests;

/// <summary>
/// Tests pour le framework serverless
/// Ces tests définissent le comportement attendu - votre implémentation doit les faire passer
/// </summary>
public class ServerlessFrameworkTests : IDisposable
{
    private readonly ServerlessFramework _framework = new();

    [Fact]
    public void CompileAndLoadPlugin_ShouldWork()
    {
        // Arrange
        var pluginCode = 
        """
            using Exercise2.ServerlessPlugins;
            
            public class SimplePlugin : IPlugin
            {
                public string Name => "SimplePlugin";
                public string Version => "1.0.0";
                
                public Task<PluginResult> ExecuteAsync(PluginContext context)
                {
                    return Task.FromResult(PluginResult.Successful("Hello from plugin!"));
                }
            }
        """;

        // Act
        var plugin = ServerlessFramework.CompilePlugin(pluginCode, "SimplePlugin");
        _framework.LoadPlugin(plugin);

        // Assert
        Assert.True(_framework.IsPluginLoaded("SimplePlugin"));
        Assert.Equal("SimplePlugin", plugin.Name);
        Assert.Equal("1.0.0", plugin.Version);
    }

    [Fact]
    public async Task ExecutePlugin_WithContext_ShouldWork()
    {
        // Arrange
        var pluginCode = 
        """
            using Exercise2.ServerlessPlugins;
            
            public class ContextPlugin : IPlugin
            {
                public string Name => "ContextPlugin";
                public string Version => "1.0.0";
                
                public Task<PluginResult> ExecuteAsync(PluginContext context)
                {
                    var input = context.Get<string>("input");
                    var result = $"Processed: {input}";
                    return Task.FromResult(PluginResult.Successful(result, result));
                }
            }
        """;

        var plugin = ServerlessFramework.CompilePlugin(pluginCode, "ContextPlugin");
        _framework.LoadPlugin(plugin);

        var context = new PluginContext();
        context.Set("input", "test data");

        // Act
        var result = await _framework.ExecutePluginAsync("ContextPlugin", context);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Processed: test data", result.Data);
    }

    [Fact]
    public void MultiplePlugins_ShouldBeIsolated()
    {
        // Arrange
        var plugin1Code = 
        """
            using Exercise2.ServerlessPlugins;
            
            public class Plugin1 : IPlugin
            {
                public string Name => "Plugin1";
                public string Version => "1.0.0";
                
                public Task<PluginResult> ExecuteAsync(PluginContext context)
                {
                    return Task.FromResult(PluginResult.Successful("Plugin 1"));
                }
            }
        """;

        var plugin2Code = 
        """
            using Exercise2.ServerlessPlugins;
            
            public class Plugin2 : IPlugin
            {
                public string Name => "Plugin2";
                public string Version => "2.0.0";
                
                public Task<PluginResult> ExecuteAsync(PluginContext context)
                {
                    return Task.FromResult(PluginResult.Successful("Plugin 2"));
                }
            }
        """;

        // Act
        var plugin1 = ServerlessFramework.CompilePlugin(plugin1Code, "Plugin1");
        var plugin2 = ServerlessFramework.CompilePlugin(plugin2Code, "Plugin2");

        _framework.LoadPlugin(plugin1, isolate: true);
        _framework.LoadPlugin(plugin2, isolate: true);

        // Assert
        Assert.True(_framework.IsPluginLoaded("Plugin1"));
        Assert.True(_framework.IsPluginLoaded("Plugin2"));
        
        var loadedPlugins = _framework.GetLoadedPlugins().ToList();
        Assert.Contains("Plugin1", loadedPlugins);
        Assert.Contains("Plugin2", loadedPlugins);
    }

    [Fact]
    public void UnloadPlugin_ShouldReleaseResources()
    {
        // Arrange
        var pluginCode = 
        """
            using Exercise2.ServerlessPlugins;
            
            public class UnloadablePlugin : IPlugin
            {
                public string Name => "UnloadablePlugin";
                public string Version => "1.0.0";
                
                public Task<PluginResult> ExecuteAsync(PluginContext context)
                {
                    return Task.FromResult(PluginResult.Successful("OK"));
                }
            }
        """;

        var plugin = ServerlessFramework.CompilePlugin(pluginCode, "UnloadablePlugin");
        _framework.LoadPlugin(plugin);

        // Act
        var unloaded = _framework.UnloadPlugin("UnloadablePlugin");

        // Assert
        Assert.True(unloaded);
        Assert.False(_framework.IsPluginLoaded("UnloadablePlugin"));
    }

    [Fact]
    public void HotReloadPlugin_ShouldUpdateCode()
    {
        // Arrange
        var originalCode = 
        """
            using Exercise2.ServerlessPlugins;
            
            public class HotReloadPlugin : IPlugin
            {
                public string Name => "HotReloadPlugin";
                public string Version => "1.0.0";
                
                public Task<PluginResult> ExecuteAsync(PluginContext context)
                {
                    return Task.FromResult(PluginResult.Successful("Version 1"));
                }
            }
        """;

        var updatedCode = 
        """
            using Exercise2.ServerlessPlugins;
            
            public class HotReloadPlugin : IPlugin
            {
                public string Name => "HotReloadPlugin";
                public string Version => "2.0.0";
                
                public Task<PluginResult> ExecuteAsync(PluginContext context)
                {
                    return Task.FromResult(PluginResult.Successful("Version 2"));
                }
            }
        """;

        var originalPlugin = ServerlessFramework.CompilePlugin(originalCode, "HotReloadPlugin");
        _framework.LoadPlugin(originalPlugin);

        // Act
        var reloadedPlugin = _framework.ReloadPlugin(updatedCode, "HotReloadPlugin");

        // Assert
        Assert.Equal("2.0.0", reloadedPlugin.Version);
        
        var info = _framework.GetPluginInfo("HotReloadPlugin");
        Assert.NotNull(info);
        Assert.Equal(2, info.LoadCount); // Chargé 2 fois
    }

    [Fact]
    public async Task PluginWithComplexLogic_ShouldWork()
    {
        // Arrange
        var pluginCode = 
        """
            using Exercise2.ServerlessPlugins;
            using System.Linq;
            
            public class ComplexPlugin : IPlugin
            {
                public string Name => "ComplexPlugin";
                public string Version => "1.0.0";
                
                public async Task<PluginResult> ExecuteAsync(PluginContext context)
                {
                    var numbers = context.Get<int[]>("numbers");
                    
                    // Logique métier complexe
                    await Task.Delay(10); // Simulation d'une opération asynchrone
                    
                    var sum = numbers.Sum();
                    var average = numbers.Average();
                    var max = numbers.Max();
                    
                    var result = new 
                    { 
                        Sum = sum, 
                        Average = average, 
                        Max = max 
                    };
                    
                    return PluginResult.Successful("Calculated", result);
                }
            }
        """;

        var plugin = ServerlessFramework.CompilePlugin(pluginCode, "ComplexPlugin");
        _framework.LoadPlugin(plugin);

        var context = new PluginContext();
        context.Set("numbers", new[] { 1, 2, 3, 4, 5 });

        // Act
        var result = await _framework.ExecutePluginAsync("ComplexPlugin", context);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public void GetPluginInfo_ShouldReturnCorrectInformation()
    {
        // Arrange
        var pluginCode = 
        """
            using Exercise2.ServerlessPlugins;
            
            public class InfoPlugin : IPlugin
            {
                public string Name => "InfoPlugin";
                public string Version => "1.5.0";
                
                public Task<PluginResult> ExecuteAsync(PluginContext context)
                {
                    return Task.FromResult(PluginResult.Successful());
                }
            }
        """;

        var plugin = ServerlessFramework.CompilePlugin(pluginCode, "InfoPlugin");
        _framework.LoadPlugin(plugin);

        // Act
        var info = _framework.GetPluginInfo("InfoPlugin");

        // Assert
        Assert.NotNull(info);
        Assert.Equal("InfoPlugin", info.Name);
        Assert.Equal("1.5.0", info.Version);
        Assert.Equal(1, info.LoadCount);
        Assert.True((DateTime.UtcNow - info.LoadedAt).TotalSeconds < 5);
    }

    [Fact]
    public void InvalidPlugin_ShouldThrowCompilationException()
    {
        // Arrange
        var invalidCode = 
        """
            using Exercise2.ServerlessPlugins;
            
            public class InvalidPlugin : IPlugin
            {
                public string Name => "InvalidPlugin";
                public string Version => "1.0.0";
                
                public Task<PluginResult> ExecuteAsync(PluginContext context)
                {
                    // Code invalide - variable non définie
                    return Task.FromResult(undefinedVariable);
                }
            }
        """;

        // Act & Assert
        var exception = Assert.Throws<PluginCompilationException>(() =>
            ServerlessFramework.CompilePlugin(invalidCode, "InvalidPlugin")
        );
        
        Assert.NotEmpty(exception.Errors);
    }

    [Fact]
    public void PluginWithoutIPluginImplementation_ShouldThrowLoadException()
    {
        // Arrange
        var pluginCode = 
        """
            // Cette classe n'implémente pas IPlugin
            public class NotAPlugin
            {
                public string Name => "NotAPlugin";
            }
        """;

        // Act & Assert
        Assert.Throws<PluginLoadException>(() =>
            ServerlessFramework.CompilePlugin(pluginCode, "NotAPlugin")
        );
    }

    [Fact]
    public async Task ExecuteNonExistentPlugin_ShouldThrowException()
    {
        // Arrange
        var context = new PluginContext();

        // Act & Assert
        await Assert.ThrowsAsync<PluginExecutionException>(async () =>
            await _framework.ExecutePluginAsync("NonExistentPlugin", context)
        );
    }

    public void Dispose()
    {
        _framework.Dispose();
    }
}
