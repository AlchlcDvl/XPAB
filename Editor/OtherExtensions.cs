namespace XPAB;

internal static class OtherExtensions
{
    extension(TargetPlatform target)
    {
        public BuildTarget GetBuildTarget() => target switch
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

        public string GetShortForm() => target switch
        {
            TargetPlatform.Windows32 => "win32",
            TargetPlatform.Windows64 => "win64",
            TargetPlatform.Mac => "mac",
            TargetPlatform.Linux64 => "lin",
            TargetPlatform.PlayStation4 => "ps4",
            TargetPlatform.PlayStation5 => "ps5",
            TargetPlatform.XboxOne => "xbox1",
            TargetPlatform.XboxSeries => "xbox",
            TargetPlatform.Android => "and",
            TargetPlatform.iOS => "ios",
            TargetPlatform.Switch => "swit",
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Received an unsupported platform."),
        };
    }
}
