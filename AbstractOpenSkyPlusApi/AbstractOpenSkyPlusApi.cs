using System;
using System.Reflection;
using System.Text.Json;

namespace OpenSkyPlusApi;

public abstract class AbstractOpenSkyPlusApi
{
    private static object _apiInstance;

    public static string ApiVersion = "1.0"; // Must match OpenSkyPlus Api version
    private Action _onConnect = () => { };
    private Action _onDisconnect = () => { };
    private Action _onNotReady = () => { };
    private Action _onReady = () => { };
    private Action _onShot = () => { };

    public string PluginName = "My Plugin";

    /// <summary>
    ///     Override with the method to call when the launch monitor is connected
    /// </summary>
    protected Action OnConnect
    {
        get => _onConnect;
        set
        {
            _onConnect = value;
            RegisterAction("_onConnect");
        }
    }

    /// <summary>
    ///     Override with the method to call when the launch monitor is disconnected
    /// </summary>
    protected Action OnDisconnect
    {
        get => _onDisconnect;
        set
        {
            _onDisconnect = value;
            RegisterAction("_onDisconnect");
        }
    }

    /// <summary>
    ///     Override with the method to call when the monitor is ready to receive a Shot
    /// </summary>
    protected Action OnReady
    {
        get => _onReady;
        set
        {
            _onReady = value;
            RegisterAction("_onReady");
        }
    }

    /// <summary>
    ///     Override with the method to call when the monitor isn't ready to read a Shot
    /// </summary>
    protected Action OnNotReady
    {
        get => _onNotReady;
        set
        {
            _onNotReady = value;
            RegisterAction("_onNotReady");
        }
    }

    /// <summary>
    ///     Override with the method to call when a Shot is received by the monitor
    /// </summary>
    protected Action OnShot
    {
        get => _onShot;
        set
        {
            _onShot = value;
            RegisterAction("_onShot");
        }
    }

    private void Init(object apiInstance)
    {
        try
        {
            _apiInstance = apiInstance;
        }
        catch
        {
            throw new Exception("Could not find OpenSkyPlus Api.\nThis plugin is " +
                                "not meant to be run in isolation. Please see documentation.");
        }
    }

    /// <summary>
    ///     Plugin entrypoint. Override with any initialization code you need
    ///     such as establishing a connection with the simulator software.
    /// </summary>
    protected abstract void Initialize();

    /// <summary>
    ///     Gets the current connection status of the monitor
    /// </summary>
    public virtual bool IsConnected()
    {
        return (bool?)_apiInstance?.GetType()
            .GetMethod("IsConnected", BindingFlags.Public | BindingFlags.Instance)?
            .Invoke(_apiInstance, null) ?? false;
    }

    /// <summary>
    ///     Gets the current ready status of the monitor
    /// </summary>
    public virtual bool IsReady()
    {
        return (bool?)_apiInstance?.GetType()
            .GetMethod("IsReady", BindingFlags.Public | BindingFlags.Instance)?
            .Invoke(_apiInstance, null) ?? false;
    }

    /// <summary>
    ///     Takes the launch monitor out of putting mode
    /// </summary>
    public virtual bool SetNormalMode()
    {
        return (bool?)_apiInstance.GetType()
            .GetMethod("SetNormalMode", BindingFlags.Public | BindingFlags.Instance)?
            .Invoke(_apiInstance, null) ?? false;
    }

    /// <summary>
    ///     Puts the launch monitor in putting mode
    /// </summary>
    public virtual bool SetPuttingMode()
    {
        return (bool?)_apiInstance.GetType()
            .GetMethod("SetPuttingMode", BindingFlags.Public | BindingFlags.Instance)?
            .Invoke(_apiInstance, null) ?? false;
    }

    /// <summary>
    ///     Returns the monitor's current battery level
    /// </summary>
    public virtual int GetBatteryLevel()
    {
        throw new NotImplementedException("Battery level not yet implemented");
        return (int)_apiInstance.GetType()
            .GetMethod("GetBatteryLevel", BindingFlags.Public | BindingFlags.Instance)
            .Invoke(_apiInstance, null);
    }

    /// <summary>
    ///     Checks if the monitor's battery is currently Charging or not
    /// </summary>
    public virtual bool IsCharging()
    {
        throw new NotImplementedException("Charging status not yet implemented");
        return (bool)_apiInstance.GetType()
            .GetMethod("IsCharging", BindingFlags.Public | BindingFlags.Instance)
            .Invoke(_apiInstance, null);
    }

    /// <summary>
    ///     Gets the current Shot mode
    /// </summary>
    public virtual ShotMode GetShotMode()
    {
        return (ShotMode?)_apiInstance.GetType()
            .GetMethod("GetShotMode", BindingFlags.Public | BindingFlags.Instance)?
            .Invoke(_apiInstance, null) ?? ShotMode.Normal;
    }

    /// <summary>
    ///     Returns the last Shot received by the monitor
    /// </summary>
    public virtual ShotData GetLastShot()
    {
        try
        {
            var shot = _apiInstance.GetType()
                .GetMethod("GetLastShot", BindingFlags.Public | BindingFlags.Instance)?
                .Invoke(_apiInstance, null);

            var jsonShot = JsonSerializer.Serialize(shot);
            return JsonSerializer.Deserialize<ShotData>(jsonShot);
        }
        catch (Exception ex)
        {
            LogToOpenSkyPlus($"Failed to get the last Shot from the Api:\n{ex}", LogLevels.Warning);
            return null;
        }
    }

    /// <summary>
    ///     Tells the launch monitor that the game is ready for the next Shot
    /// </summary>
    public virtual bool ReadyForNextShot()
    {
        return (bool?)_apiInstance.GetType()
            .GetMethod("ReadyForNextShot", BindingFlags.Public | BindingFlags.Instance)?
            .Invoke(_apiInstance, null) ?? false;
    }

    /// <summary>
    ///     Sends a message to the OpenSkyPlus logger
    /// </summary>
    public void LogToOpenSkyPlus(string message, LogLevels level)
    {
        try
        {
            _apiInstance?.GetType()
                .GetMethod("LogToOpenSkyPlus", BindingFlags.Public | BindingFlags.Instance,
                    null, [typeof(string), typeof(object)], null)?
                .Invoke(_apiInstance, [message, level]);
        }
        catch (Exception ex)
        {
            throw new Exception($"Logging to OpenSkyPlus failed:\n{ex}");
        }
    }

    public void RegisterAction(string action)
    {
        try
        {
            if (_apiInstance == null)
                return;
            var oksActionField = _apiInstance?.GetType().BaseType
                ?.GetField(action, BindingFlags.NonPublic | BindingFlags.Instance);
            var instanceActionField =
                GetType().BaseType?.GetField(action, BindingFlags.NonPublic | BindingFlags.Instance);
            var oksAction = (Action)oksActionField?.GetValue(_apiInstance);
            var instanceAction = (Action)instanceActionField?.GetValue(this);
            var combinedAction = (Action)Delegate.Combine(oksAction, instanceAction);
            oksActionField?.SetValue(_apiInstance, combinedAction);
        }
        catch (Exception ex)
        {
            LogToOpenSkyPlus($"Failed to register {action} Action:\n{ex}", LogLevels.Warning);
        }
    }
}

public class ShotData
{
    public virtual BallPositions BallPosition { get; set; }
    public virtual ClubData Club { get; set; }
    public virtual LaunchData Launch { get; set; }
    public virtual SpinData Spin { get; set; }
}

public class ClubData
{
    public virtual float HeadSpeed { get; set; }
    public virtual float HeadSpeedConfidence { get; set; }
}

public class LaunchData
{
    public virtual float HorizontalAngle { get; set; }
    public virtual float HorizontalAngleConfidence { get; set; }
    public virtual float LaunchAngle { get; set; }
    public virtual float LaunchAngleConfidence { get; set; }
    public virtual float TotalSpeed { get; set; }
    public virtual float TotalSpeedConfidence { get; set; }
}

public class SpinData
{
    public virtual float Backspin { get; set; }
    public virtual float SideSpin { get; set; }
    public virtual float TotalSpin { get; set; }
    public virtual float SpinAxis { get; set; }
    public virtual float MeasurementConfidence { get; set; }
}

public enum ShotMode
{
    Normal,
    Putting
}

public enum BallPositions
{
    Ok = 0,
    Near = 1,
    Far = 2,
    Unknown = 3
}

public enum LogLevels
{
    Info,
    Debug,
    Warning,
    Error
}