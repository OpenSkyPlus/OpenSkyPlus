using System.IO;
using BepInEx;
using BepInEx.Logging;

namespace OpenSkyPlus;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class OpenSkyPlus : BaseUnityPlugin
{
    public static ManualLogSource Log;
    public static readonly string OpenSkyPlusPath = Path.Combine(Paths.PluginPath, PluginInfo.PLUGIN_GUID);

    private void Awake()
    {
        Log = Logger;
        Log.LogInfo("Initializing OpenSkyPlus Framework");

        Log.LogDebug("Initialize Configuration");
        OpenSkyPlusConfiguration.Initialize(Log);

        Log.LogDebug("Initialize Custom Logger");
        OpenSkyPlusLoggerInitializer.Initialize(Log);

        Log.LogInfo("Logging to OpenSkyPlus Log");

        Log.LogDebug("Initialize Api Subscriber");
        ApiSubscriber.Initialize();

        Log.LogDebug("Checking to see if the device is ready before loading the Api");
        OpenSkyPlusApiInjector.Initialize(Log); // Api will initialize automatically when the device is ready

        Log.LogDebug("Loading the UI");
        OpenSkyPlusUi.Initialize();

        Log.LogInfo("OpenSkyPlus Framework Successfully Loaded");

        Log.LogDebug("Initialize plugin loader");
        OpenSkyPlusPluginLoader.Initialize(OpenSkyPlusConfiguration.Config.AppSettings.PluginPath);

        Log.LogInfo("Waiting for launch monitor to come online...");
    }
}