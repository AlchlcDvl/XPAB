namespace XPAB;

internal static partial class OtherExtensions
{
    public static BuildTarget GetBuildTarget(this TargetPlatform target) => target switch
    {
        TargetPlatform.Windows32 => BuildTarget.StandaloneWindows,
        TargetPlatform.Windows64 => BuildTarget.StandaloneWindows64,
        TargetPlatform.Mac => BuildTarget.StandaloneOSX,
        TargetPlatform.Linux64 => BuildTarget.StandaloneLinux64,
        TargetPlatform.PlayStation4 => BuildTarget.PS4,
        TargetPlatform.PlayStation5 => BuildTarget.PS5,
        TargetPlatform.XboxOne => BuildTarget.XboxOne,
        TargetPlatform.XboxSeries => BuildTarget.GameCoreXboxSeries,
        TargetPlatform.Android => BuildTarget.Android,
        TargetPlatform.iOS => BuildTarget.iOS,
        TargetPlatform.Switch => BuildTarget.Switch,
        _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Received an unsupported platform."),
    };
}
