# Exercice 2 : Framework Serverless avec Plugins

## 🎯 Objectif

Construire un mini-framework serverless permettant de charger et d'exécuter des plugins compilés dynamiquement avec isolation et gestion de dépendances.

## 📋 Ce que vous devez implémenter

Créez un framework serverless qui :

1. **Compile des plugins** à partir de code source C#
2. **Isole chaque plugin** dans son propre AssemblyLoadContext
3. **Charge et exécute** les plugins de manière sécurisée
4. **Gère les dépendances** entre plugins
5. **Supporte le hot-reload** (rechargement à chaud)
6. **Fournit un contexte d'exécution** partagé
7. **Permet le déchargement** des plugins

## 🧪 Tests à valider

Les tests suivants doivent passer :
- `CompileAndLoadPlugin_ShouldWork`
- `ExecutePlugin_WithContext_ShouldWork`
- `MultiplePlugins_ShouldBeIsolated`
- `PluginWithDependency_ShouldLoadCorrectly`
- `UnloadPlugin_ShouldReleaseResources`
- `HotReloadPlugin_ShouldUpdateCode`
- `PluginWithSharedInterface_ShouldWork`
- `InvalidPlugin_ShouldThrowException`

## 💡 Indices

- Utilisez `AssemblyLoadContext` pour l'isolation
- Définissez une interface `IPlugin` que tous les plugins doivent implémenter
- Créez un système de versioning pour le hot-reload
- Gérez WeakReferences pour permettre le garbage collection

## 🚀 Lancer les tests

```bash
dotnet test
```

## 📚 Concepts Avancés

- **AssemblyLoadContext** : Isolation des assemblies
- **WeakReference** : Gestion mémoire et déchargement
- **Reflection** : Découverte et invocation dynamique
- **Plugin Architecture** : Design patterns pour plugins
