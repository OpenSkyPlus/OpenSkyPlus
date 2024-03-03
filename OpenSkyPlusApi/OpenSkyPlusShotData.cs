using OpenSkyPlusApi;

namespace OpenSkyPlus;

public static class BallPositionData
{
    public static string ContainerSymbol => "FDDDFMHPPEP";
    public static string BallPositionSymbol => "GGAEPNHCLEM";
}

public class LaunchMonitorShotData : ShotData
{
    public override BallPositions BallPosition { get; set; } = BallPositions.Unknown;
    public override ClubData Club { get; set; } = new();
    public override LaunchData Launch { get; set; } = new();
    public override SpinData Spin { get; set; } = new();

    public sealed class LaunchMonitorClubData : ClubData
    {
        public static string HeadSpeedSymbol = "CEGFOPNGEKB";
        public static string HeadSpeedConfidenceSymbol = "EGHIKFPPDKJ";

        public static string ContainerSymbol => "FDDDFMHPPEP";

        public override float HeadSpeed { get; set; }
        public override float HeadSpeedConfidence { get; set; }
    }

    public sealed class LaunchMonitorLaunchData(
        float horizontalAngle,
        float horizontalAngleConfidence,
        float launchAngle,
        float launchAngleConfidence,
        float totalSpeed,
        float totalSpeedConfidence) : LaunchData
    {
        public LaunchMonitorLaunchData() : this(0f, 0f, 0f, 0f, 0f, 0f)
        {
        }

        public static string HorizontalAngleSymbol => "GPJCPFANDGJ";
        public static string HorizontalAngleConfidenceSymbol => "DCCOCLOMLGA";
        public static string LaunchAngleSymbol => "EDNNKEPLHKC";
        public static string LaunchAngleConfidenceSymbol => "GMGMMCENIPA";
        public static string TotalSpeedSymbol => "BAFOBJEAGMN";
        public static string TotalSpeedConfidenceSymbol => "GMBPBDLKJEG";

        public override float HorizontalAngle { get; set; } = horizontalAngle;
        public override float HorizontalAngleConfidence { get; set; } = horizontalAngleConfidence;
        public override float LaunchAngle { get; set; } = launchAngle;
        public override float LaunchAngleConfidence { get; set; } = launchAngleConfidence;
        public override float TotalSpeed { get; set; } = totalSpeed;
        public override float TotalSpeedConfidence { get; set; } = totalSpeedConfidence;
    }

    public sealed class LaunchMonitorSpinData(
        float backspin,
        float sideSpin,
        float totalSpin,
        float spinAxis,
        float measurementConfidence) : SpinData
    {
        public static string MeasurementConfidenceSymbol = "AICPAFBKOPI";

        public LaunchMonitorSpinData() : this(0f, 0f, 0f, 0f, 0f)
        {
        }

        public static string ContainerSymbol => "GGEGJMKCNOP";
        public static string BackspinSymbol => "DHDLBIEFGMM";
        public static string SideSpinSymbol => "OFFCCJAAMLG";
        public static string TotalSpinSymbol => "BMINEIHGFLI";
        public static string SpinAxisSymbol => "MFLAAMECNIN";

        public override float Backspin { get; set; } = backspin;
        public override float SideSpin { get; set; } = sideSpin;
        public override float TotalSpin { get; set; } = totalSpin;
        public override float SpinAxis { get; set; } = spinAxis;
        public override float MeasurementConfidence { get; set; } = measurementConfidence;
    }
}