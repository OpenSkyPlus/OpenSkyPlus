using System;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;

namespace OpenSkyPlus;

public delegate void NotificationOpenSkyPlusReady();

public delegate void NotificationNewShot(object launchMonitorShot);

public static class OpenSkyPlusApiInjector
{
    private static ManualLogSource _logger;

    private static readonly Harmony Harmony = new("OpenSkyPlus");

    private static bool _deviceReady;
    public static event NotificationDeviceReady MessageDeviceReady;
    public static event NotificationOpenSkyPlusReady MessageOpenSkyPlusReady;
    public static event NotificationNewShot MessageNewShot;
    public static event NotificationMonitorStatusChange MessageMonitorConnected;
    public static event NotificationMonitorStatusChange MessageMonitorDisconnected;

    public static void Initialize(ManualLogSource log)
    {
        _logger = log;
        DeviceReadyCheckPatch();
        ApiSubscriber.MessageApiLoaded += ShotDataInjector;
        ApiSubscriber.MessageApiLoaded += DeviceConnectedPatch;
        ApiSubscriber.MessageApiLoaded += DeviceDisconnectedPatch;
    }

    private static void DeviceReadyCheckPatch()
    {
        try
        {
            Harmony.Patch(
                AccessTools.TypeByName("Security.AOMKMFBGCGG")
                    .GetMethod("OAPNBCHPPBI", BindingFlags.NonPublic | BindingFlags.Instance),
                postfix: new HarmonyMethod(typeof(OpenSkyPlusApiInjector), nameof(DeviceReadyPostFixPatch)));
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to apply device ready patch:\n{ex}");
        }

        _logger.LogDebug("Device ready check patch successfully applied");
    }

    private static void DeviceReadyPostFixPatch()
    {
        try
        {
            if (_deviceReady)
                return;

            if (
                !((bool?)AccessTools.TypeByName("Security.ELOOGCNONDM")
                    .GetProperty("GPCDJOPENGH")?
                    .GetValue(AccessTools
                        .TypeByName("Security.AOMKMFBGCGG")
                        .GetProperty("KGJFDOMIPBN", BindingFlags.Public | BindingFlags.Static)?
                        .GetValue(null)) ?? false)
            ) return;

            _deviceReady = true;
            _logger.LogDebug("Device Ready");
            MessageDeviceReady?.Invoke(_logger);
            MessageMonitorConnected?.Invoke();
            if (!DeviceControls.Armed)
                DeviceControls.DisarmMonitor();
        }
        catch
        {
            if (_deviceReady)
            {
                _deviceReady = false;
                _logger.LogDebug("Device Disconnected");
                MessageMonitorDisconnected?.Invoke();
            }
        }
    }

    private static void DeviceDisconnectedPatch()
    {
        try
        {
            Harmony.Patch(
                AccessTools.TypeByName(
                        $"{OpenSkyPlusConfiguration.Config.AppSettings.LaunchMonitor}Wrapper.EONIOHDNGND")
                    .GetMethod("PAOPCKCGFGE", BindingFlags.NonPublic | BindingFlags.Instance),
                postfix: new HarmonyMethod(typeof(OpenSkyPlusApiInjector), nameof(DeviceDisconnectedPostFixPatch)));
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to apply device connected patch:\n{ex}");
        }

        _logger.LogDebug("Device disconnected patch successfully applied");
    }

    private static void DeviceDisconnectedPostFixPatch()
    {
        MessageMonitorDisconnected?.Invoke();
    }
    
    private static void DeviceConnectedPatch()
    {
        try
        {
            Harmony.Patch(
                AccessTools.TypeByName(
                        $"{OpenSkyPlusConfiguration.Config.AppSettings.LaunchMonitor}Wrapper.EONIOHDNGND")
                    .GetNestedType("DKHFNPIHCDM", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .GetMethod("IKLNPOOAIPI", BindingFlags.Public | BindingFlags.Static),
                postfix: new HarmonyMethod(typeof(OpenSkyPlusApiInjector), nameof(DeviceConnectedPostFixPatch)));
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to apply device connected patch:\n{ex}");
        }

        _logger.LogDebug("Device connected patch successfully applied");
    }

    private static void DeviceConnectedPostFixPatch(ref object __result)
    {
        var value = (int)Convert.ChangeType(__result, Enum.GetUnderlyingType(__result.GetType()));
        if (value == 0) MessageMonitorConnected?.Invoke();
    }

    private static void ShotDataInjector()
    {
        try
        {
            Harmony.Patch(
                AccessTools.TypeByName("Security.AOMKMFBGCGG")
                    .GetMethod("EHFHCHBHBEF", BindingFlags.NonPublic | BindingFlags.Instance),
                postfix: new HarmonyMethod(typeof(OpenSkyPlusApiInjector), nameof(ShotDataPostFixPatch)));
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to apply shot data patch:\n{ex}");
        }

        _logger.LogDebug("Shot data patch successfully applied");
        MessageOpenSkyPlusReady?.Invoke();
    }

    private static void ShotDataPostFixPatch(object IJHAAAHONOE)
    {
        MessageNewShot?.Invoke(IJHAAAHONOE);
    }
}