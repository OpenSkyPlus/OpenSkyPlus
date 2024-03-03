using System;
using System.IO;
using System.Reflection;
using BepInEx.Logging;

namespace OpenSkyPlus;

public delegate void NotificationPluginLoaded();

public static class OpenSkyPlusPluginLoader
{
    private static string _pluginPath;
    private static ManualLogSource _logger;

    public static event NotificationPluginLoaded MessagePluginLoaded;

    public static void Initialize(string pluginPath)
    {
        _pluginPath = pluginPath;
        _logger = OpenSkyPlus.Log;
        ApiSubscriber.MessageApiLoaded += ResolvePlugins;
    }


    /// <summary>
    ///     Loads compatible OpenSkyPro plugins.
    /// </summary>
    /// The loader traverses the plugin directory then for each directory, 
    /// searches for a dll by the same name and loads it.
    public static void ResolvePlugins()
    {
        _logger.LogInfo("Loading plugins...");
        if (!Directory.Exists(_pluginPath))
        {
            _logger.LogError("Plugin path does not exist. Check the configuration file.");
            return;
        }

        foreach (var subdirectory in Directory.GetDirectories(_pluginPath))
        {
            var pluginName = new DirectoryInfo(subdirectory);
            var pluginPattern = pluginName.Name + ".dll";
            var pluginFile = Directory.GetFiles(subdirectory, pluginPattern, SearchOption.TopDirectoryOnly);
            if (pluginFile.Length > 0)
                LoadPlugin(pluginFile[0]);
            else
                _logger.LogWarning($"Appropriately named dll not found in plugin subdirectory.\n"
                                  + $"Corresponding DLL should be named {pluginName}.dll");
        }
    }

    public static void LoadPlugin(string pluginFile)
    {
        try
        {
            var plugin = Assembly.LoadFrom(pluginFile);
            var pluginTypes = plugin.GetTypes();
            if (pluginTypes.Length == 0)
                throw new FormatException("Loaded plugin contains no type information.");
            var foundInterface = false;
            foreach (var type in pluginTypes)
                if (type.BaseType?.Name == "AbstractOpenSkyPlusApi" && type.Name != "AbstractOpenSkyPlusApi")
                {
                    var interfaceType = type.BaseType;
                    var pluginInstance = Activator.CreateInstance(type);
                    var versionField = (pluginInstance.GetType().BaseType?
                        .GetField("ApiVersion", BindingFlags.Public | BindingFlags.Static)) ?? 
                                       throw new MissingFieldException("Plugin is missing ApiVersion");
                    var version = (string)versionField.GetValue(null) ?? "";
                    foundInterface = true;
                    if (version == OpenSkyPlusApi.GetApiVersion())
                    {
                        var baseInitMethod = interfaceType.GetMethod("Init",
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        baseInitMethod?.Invoke(pluginInstance, [OpenSkyPlusApi.GetInstance()]);
                        var derivedInitMethod = type.GetMethod("Initialize",
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        if (derivedInitMethod != null)
                        {
                            try
                            {
                                derivedInitMethod.Invoke(pluginInstance, null);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Plugin failed to initialize: {ex}");
                            }

                            MessagePluginLoaded?.Invoke();
                            _logger.LogInfo($"Plugin {pluginFile} loaded!");
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Couldn't initialize plugin. Versions match, but there was no initialization method");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Couldn't initialize plugin due to version mismatch.\n +"
                                          + $"Expecting version: {OpenSkyPlusApi.GetApiVersion()}\n"
                                          + $"Plugin version: {version}");
                    }
                }

            if (!foundInterface)
                _logger.LogWarning(
                    $"Failed to initialize the plugin {pluginFile} because the interface was not found.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to load dll: {pluginFile}\n{ex}");
        }
    }
}