using System;
using System.IO;
using BepInEx.Logging;

namespace OpenSkyPlus;

public static class OpenSkyPlusLoggerInitializer
{
    private static ManualLogSource _logger;

    public static void Initialize(ManualLogSource log)
    {
        _logger = log;

        var loggingPath = Path.Combine(OpenSkyPlusConfiguration.Config.AppSettings.LogPath, "Log.txt");

        var logLevel = Enum.TryParse(OpenSkyPlusConfiguration.Config.AppSettings.LogLevel, out LogLevel level) switch
        {
            true => level,
            _ => LogLevel.Info
        };

        _logger.LogInfo($"Logging at: {loggingPath}");
        Logger.Listeners.Add(new OpenSkyPlusLogListener(loggingPath, logLevel));
    }
}