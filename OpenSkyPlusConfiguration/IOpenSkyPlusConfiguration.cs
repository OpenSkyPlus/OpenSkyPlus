namespace OpenSkyPlus;

public interface IOpenSkyPlusConfiguration
{
    public struct AppSettings(
        string logPath,
        string logLevel,
        string launchMonitor,
        string launchMonitorAppPath,
        string pluginPath,
        string shotConfidence,
        bool refreshConnectionAfterModeSwitch)
    {
        public string LogPath { get; set; } = logPath;
        public string LogLevel { get; set; } = logLevel;
        public string LaunchMonitor { get; set; } = launchMonitor;
        public string LaunchMonitorAppPath { get; set; } = launchMonitorAppPath;
        public string PluginPath { get; set; } = pluginPath;
        public string ShotConfidence { get; set; } = shotConfidence;
        public bool RefreshConnectionAfterModeSwitch { get; set; } = refreshConnectionAfterModeSwitch;
    }
}