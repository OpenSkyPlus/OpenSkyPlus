using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BepInEx.Logging;
using HarmonyLib;
using OpenSkyPlusApi;
using UnityEngine;

namespace OpenSkyPlus;

internal static class DeviceControls
{
    private static ManualLogSource _logger;
    private static object _launchMonitorWrapper;
    private static object _launchMonitorController;
    private static OpenSkyPlusConfiguration.Configuration _config;
    public static bool? Armed { get; private set; }
    public static Handedness? HandednessValue { get; private set; }

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
        _logger.LogInfo("InitializeLaunchMonitorWrapper");

        var launchMonitorWrapperType =
            AccessTools.TypeByName($"{_config.AppSettings.LaunchMonitor}Wrapper.EONIOHDNGND");
        var launchMonitorWrapperProperty = launchMonitorWrapperType.GetProperty("KGJFDOMIPBN");
        _launchMonitorWrapper = launchMonitorWrapperProperty?.GetValue(null);

        //DumpType("_launchMonitorWrapper", _launchMonitorWrapper.GetType());
        //_logger.LogInfo($"_launchMonitorWrapper:\n{System.Text.Json.JsonSerializer.Serialize(_launchMonitorWrapper, new JsonSerializerOptions() { IncludeFields = true, WriteIndented = true })}");
    }

    private static void InitializeLaunchMonitorController()
    {
        _logger.LogInfo("InitializeLaunchMonitorController");

        var securityWrapperInitializerType = AccessTools.TypeByName("SecurityWrapperInitializer");
        var securityWrapperInitializerProp = securityWrapperInitializerType.GetProperty("KGJFDOMIPBN");
        var securityWrapperInitializerInstance = securityWrapperInitializerProp?.GetValue(null);
        var launchMonitorControllerProperty = securityWrapperInitializerType.GetProperty("IJDFIOFEBMA");
        _launchMonitorController = launchMonitorControllerProperty?.GetValue(securityWrapperInitializerInstance);

        //DumpType("securityWrapperInitializerInstance", securityWrapperInitializerInstance.GetType());

        //DumpType("_launchMonitorController", _launchMonitorController.GetType());

        var isLeftHandednessProperty = _launchMonitorController.GetType().GetProperty("EAOIHLFKBDI", BindingFlags.NonPublic | BindingFlags.Instance);
        var isLeftHandedness = isLeftHandednessProperty.GetValue(_launchMonitorController);
        HandednessValue = Convert.ToBoolean(isLeftHandedness) ? Handedness.Left : Handedness.Right;
        _logger.LogInfo($"IsHandednessLeft: {isLeftHandedness}");
    }

    private static void DumpType(string member, Type type)
    {
        StringBuilder sb = new StringBuilder($"Dumping type of {member}: {type.FullDescription()}:" + Environment.NewLine);
        sb.AppendLine("Interfaces:");
        foreach (Type t in type.GetInterfaces())
        {
            sb.AppendLine($"\t{t}");
            sb.AppendLine("\tProperties:");
            foreach (var property in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                sb.AppendLine($"\t\t{property.PropertyType,30} {property.Name,30} - Info: {(property.CanRead ? "Read" : "")} {(property.CanWrite ? "Write" : "")} {(property.Attributes)}");
            }
            sb.AppendLine("\tFields:");
            foreach (var field in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                sb.AppendLine($"\t\t{field.FieldType,30} {field.Name,30} - Info: {(field.IsPrivate ? "Private" : "")} {(field.IsPublic ? "Public" : "")}");
            }
        }
        sb.AppendLine("Properties:");
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
        {
            sb.AppendLine($"\t{property.PropertyType,30} {property.Name,30} - Info: {(property.CanRead ? "Read" : "")} {(property.CanWrite ? "Write" : "")}");
        }
        sb.AppendLine("Methods:");
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
        {
            var parameters = method.GetParameters();
            var parameterDescriptions = string.Join
                (", ", method.GetParameters()
                             .Select(x => x.ParameterType + " " + x.Name)
                             .ToArray());

            sb.AppendLine($"\t{method.ReturnType,30} {method.Name,30}({parameterDescriptions,50}) - Info: {(method.IsPrivate ? "Private" : "Public"),10} {(method.IsStatic ? "Static" : "Instance"),10}");
        }
        sb.AppendLine("Fields:");
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
        {
            sb.AppendLine($"\t{field.FieldType,30} {field.Name,30} - Info: {(field.IsPrivate ? "Private" : "")} {(field.IsPublic ? "Public" : "")}");
        }
        _logger.LogInfo(sb);
    }

    public static event NotificationMonitorStatusChange MessageMonitorReady;

    public static bool ArmMonitor()
    {
        _logger.LogInfo($"ArmMonitor");

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
        _logger.LogInfo($"DisarmMonitor");

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
        _logger.LogInfo($"Setting shot mode to {shotMode}");

        // Putting:
        // public void CBJNBOIKFIE
        //
        // Normal:
        // public void FPFBIPJNOKH
        var wasArmed = Armed == true;
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
            if (_config.AppSettings.RefreshConnectionAfterModeSwitch)
            {
                RefreshConnection();
            }
            if (wasArmed) ArmMonitor();
        }
    }

    public static async void RefreshConnection(bool disconnectOnly = false)
    {
        _logger.LogInfo($"RefreshConnection: pause/unpause");

        // Pause SW
        _launchMonitorController.GetType()
            .InvokeMember("JNCPBMDPIKB", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                null, _launchMonitorController, [true]);

        // Do not call twice if only want to pause (disconnect)
        if (!disconnectOnly)
        {
            await Task.Delay(200);

            // Resume SW
            _launchMonitorController.GetType()
                .InvokeMember("JNCPBMDPIKB", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                    null, _launchMonitorController, [false]);
        }
    }

    internal static Handedness? GetHandedness()
    {
        _logger.LogInfo($"GetHandedness: {HandednessValue}");

        return HandednessValue;
    }

    internal static Handedness? SetHandedness(Handedness handedness)
    {
        if (HandednessValue == handedness)
        {
            _logger.LogInfo($"SetHandedness to {handedness} skipped because already in proper mode");
            return HandednessValue;
        }
        
        _logger.LogInfo($"SetHandedness: {handedness}");

        var methodInfo = _launchMonitorWrapper.GetType().GetMethod("JNPKHHIIAGD");
        var handednessParam = methodInfo.GetParameters()[0];
        var enumVals = handednessParam.ParameterType.GetEnumValues();

        var wasArmed = Armed == true;
        try
        {
            DisarmMonitor();
            switch (handedness)
            {
                case Handedness.Left:
                    _launchMonitorWrapper.GetType().InvokeMember(
                        "JNPKHHIIAGD", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                        null, _launchMonitorWrapper, [enumVals.GetValue(1)]);
                    break;
                case Handedness.Right:
                default:
                    _launchMonitorWrapper.GetType().InvokeMember(
                        "JNPKHHIIAGD", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                        null, _launchMonitorWrapper, [enumVals.GetValue(0)]);
                    break;
            }
            HandednessValue = handedness;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Couldn't set handedness to {handedness}:\n{ex}");
        }
        finally
        {
            if (wasArmed) ArmMonitor();
        }
        return HandednessValue;
    }

    internal static bool SoftResetNetwork()
    {
        _logger.LogInfo($"SoftReset network");

        // public void CKLGICEOKAN
        try
        {
            _launchMonitorWrapper.GetType().InvokeMember(
                "CKLGICEOKAN", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                null, _launchMonitorWrapper, null);
            return true;
        }
        catch
        {
            return false;
        }
    }
}