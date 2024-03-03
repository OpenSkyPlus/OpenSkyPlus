using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using OpenSkyPlusApi;

namespace OpenSkyPlus;

internal static class DeviceControls
{
    private static ManualLogSource _logger;
    private static object _launchMonitorWrapper;
    private static object _launchMonitorController;
    private static OpenSkyPlusConfiguration.Configuration _config;
    public static bool Armed { get; private set; }

    public static void Initialize(ManualLogSource log)
    {
        _logger = log;
        _config = OpenSkyPlusConfiguration.Config;
        InitializeLaunchMonitorWrapper();
        InitializeLaunchMonitorController();
    }

    public static void CheckAssemblyAndLicense()
    {
        try
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var unityAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name.Equals("Assembly-CSharp"));

            if (unityAssembly == null)
            {
                // Assembly not found. Try path from configuration                    
                var assemblyPath = Path.Combine(_config.AppSettings.LaunchMonitorAppPath,
                    $"{_config.AppSettings.LaunchMonitor}_Data\\Managed", "Assembly-CSharp.dll");
                _logger.LogDebug($"Loading Assembly-CSharp from {assemblyPath}");
                Assembly.LoadFrom(assemblyPath);
                unityAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name.Equals("Assembly-CSharp"));
            }

            var launchMonitorAppVersion = unityAssembly?
                .GetType("ECIHIAMNLCN")
                .GetProperty("MPGEGBMCFFN", BindingFlags.Static | BindingFlags.Public)?
                .GetValue(null).ToString();

            if (launchMonitorAppVersion != null)
                _logger.LogDebug("Assembly-CSharp.dll is loaded");
            else
                Terminate("Could not find Assembly-CSharp.dll. Check the app installation and try again.");

            if (launchMonitorAppVersion != OpenSkyPlusApi.SupportedVersion)
            {
                _logger.LogWarning(
                    $"{_config.AppSettings.LaunchMonitor} application version does not match supported version. Application may not work correctly.");
                _logger.LogWarning($"Supported version: {OpenSkyPlusApi.SupportedVersion}");
                _logger.LogWarning($"Found: {launchMonitorAppVersion}");
            }
        }
        catch
        {
            Terminate($"{_config.AppSettings.LaunchMonitor} assemblies not found. Check the app path and try again.");
        }

        // THIS LINE CHECKS TO MAKE SURE THE LICENSE IS VALID. DO NOT DELETE THIS LINE OR THE CHECK WILL NOT WORK!
        if (!CheckLicense()) Terminate("Could not verify app license");

        try
        {
            InitializeLaunchMonitorWrapper();
            InitializeLaunchMonitorController();
        }
        catch (Exception ex)
        {
            _logger.LogFatal($"Could not initialize {_config.AppSettings.LaunchMonitor} wrapper functions:\n{ex}");
            Terminate($"Failed to locate {_config.AppSettings.LaunchMonitor} monitor wrapper functions.\n" +
                      $"Is this Api compatible with the version {_config.AppSettings.LaunchMonitor}?");
        }

        return;

        static bool CheckLicense()
        {
            try
            {
                var licenseStatusProperty = AccessTools
                    .TypeByName($"{_config.AppSettings.LaunchMonitor}Wrapper.EONIOHDNGND+MOBEJGCNPEM")
                    .GetProperty("GNFAINMEEHO");

                var licenseStatusInstance = AccessTools
                    .TypeByName($"{_config.AppSettings.LaunchMonitor}Wrapper.EONIOHDNGND+IBHCDNLENEF")?
                    .GetProperty("GEDJNGPNLKO")?.GetValue(AccessTools.TypeByName("Security.ELOOGCNONDM")?
                        .GetProperty("IBHCDNLENEF")?.GetValue(AccessTools.TypeByName("Security.AOMKMFBGCGG")?
                            .GetProperty("KGJFDOMIPBN", BindingFlags.Public | BindingFlags.Static)?
                            .GetValue(null)));

                if ((int?)licenseStatusProperty?.GetValue(licenseStatusInstance) == 0)
                {
                    _logger.LogInfo("Play and Improve license is valid");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Exception during license check:\n{ex}");
                return false;
            }

            return false;
        }
    }

    private static void Terminate(string ex)
    {
        _logger.LogFatal($"Failed to load OpenSkyPlus Api:\n{ex}");
        throw new OpenSkyPlusApiException("Couldn't load OpenSkyPlus Api. Check the log for details.");
    }

    private static void InitializeLaunchMonitorWrapper()
    {
        var launchMonitorWrapperType =
            AccessTools.TypeByName($"{_config.AppSettings.LaunchMonitor}Wrapper.EONIOHDNGND");
        var launchMonitorWrapperProperty = launchMonitorWrapperType.GetProperty("KGJFDOMIPBN");
        _launchMonitorWrapper = launchMonitorWrapperProperty?.GetValue(null);
    }

    private static void InitializeLaunchMonitorController()
    {
        var securityWrapperInitializerType = AccessTools.TypeByName("SecurityWrapperInitializer");
        var securityWrapperInitializerProp = securityWrapperInitializerType.GetProperty("KGJFDOMIPBN");
        var securityWrapperInitializerInstance = securityWrapperInitializerProp?.GetValue(null);
        var launchMonitorControllerProperty = securityWrapperInitializerType.GetProperty("IJDFIOFEBMA");
        _launchMonitorController = launchMonitorControllerProperty?.GetValue(securityWrapperInitializerInstance);
    }

    public static event NotificationMonitorStatusChange MessageMonitorReady;

    public static bool ArmMonitor()
    {
        // public void MDOFGLDBEIK
        try
        {
            _launchMonitorWrapper.GetType().InvokeMember(
                "MDOFGLDBEIK", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                null, _launchMonitorWrapper, null);
            Armed = true;
            MessageMonitorReady?.Invoke();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static event NotificationMonitorStatusChange MessageMonitorNotReady;

    public static bool DisarmMonitor()
    {
        // public void JDIMANKGGMF
        try
        {
            _launchMonitorWrapper.GetType().InvokeMember(
                "JDIMANKGGMF", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                null, _launchMonitorWrapper, null);
            Armed = false;
            MessageMonitorNotReady?.Invoke();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static ShotMode SetShotMode(ShotMode shotMode)
    {
        // Putting:
        // public void CBJNBOIKFIE
        //
        // Normal:
        // public void FPFBIPJNOKH
        var wasArmed = Armed;
        try
        {
            DisarmMonitor();
            switch (shotMode)
            {
                case ShotMode.Putting:
                    _launchMonitorWrapper.GetType().InvokeMember(
                        "CBJNBOIKFIE", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                        null, _launchMonitorWrapper, null);
                    shotMode = ShotMode.Putting;
                    return ShotMode.Putting;
                case ShotMode.Normal:
                default:
                    _launchMonitorWrapper.GetType().InvokeMember(
                        "FPFBIPJNOKH", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                        null, _launchMonitorWrapper, null);
                    return ShotMode.Normal;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Couldn't set shot mode to {shotMode}:\n{ex}");
            return shotMode;
        }
        finally
        {
            RefreshConnection();
            if (wasArmed) ArmMonitor();
        }
    }

    public static void RefreshConnection(bool disconnectOnly = false)
    {
        // disconnect -> reconnect
        _launchMonitorController.GetType()
            .InvokeMember("JNCPBMDPIKB", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                null, _launchMonitorController, [true]);


        // reconnect fails, so explicitly reconnect
        _launchMonitorController.GetType()
            .InvokeMember("JNCPBMDPIKB", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                null, _launchMonitorController, [disconnectOnly]);
    }
}