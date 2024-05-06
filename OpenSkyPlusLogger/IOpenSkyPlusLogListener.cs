using System;
using System.IO;
using System.Threading;
using BepInEx;
using BepInEx.Logging;

namespace OpenSkyPlus;

public class OpenSkyPlusLogListener : ILogListener
{
    public OpenSkyPlusLogListener(string localPath, LogLevel logLevel = LogLevel.Info)
    {
        LogLevel = logLevel;
        Utility.TryOpenFileStream(Path.Combine(Paths.BepInExRootPath, localPath), FileMode.Create, out var fileStream,
            share: FileShare.Read, access: FileAccess.Write);
        LogWriter = TextWriter.Synchronized(new StreamWriter(fileStream, Utility.UTF8NoBom));
        FlushTimer = new Timer(o => { LogWriter?.Flush(); }, null, 2000, 2000);
    }

    public LogLevel LogLevel { get; set; }
    public TextWriter LogWriter { get; protected set; }
    public Timer FlushTimer { get; protected set; }

    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        if (eventArgs.Source.SourceName != PluginInfo.PLUGIN_GUID || LogLevel < eventArgs.Level)
            return;

        OpenSkyPlusUi.LogToConsole(FormatLogLine(eventArgs)); // Logs to the UI console
        LogWriter.WriteLine(FormatLogLine(eventArgs)); // Logs to disk
    }

    private string FormatLogLine(LogEventArgs logEventArgs)
    {
        return $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} | {logEventArgs.Level,-10}:{logEventArgs.Source.SourceName, 20}] {logEventArgs.Data}";
    }

    public void Dispose()
    {
        FlushTimer?.Dispose();
        LogWriter?.Flush();
        LogWriter?.Dispose();
    }

    ~OpenSkyPlusLogListener()
    {
        Dispose();
    }
}