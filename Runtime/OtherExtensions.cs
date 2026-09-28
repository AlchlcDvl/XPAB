namespace XPAB;

internal static partial class OtherExtensions
{
    [SuppressMessage("Major Code Smell", "S3928:Parameter names used into ArgumentException constructors should match an existing one ", Justification = "Ignore obvious issue, can't use nameof in this context.")]
    public static TargetPlatform GetTargetPlatform() => Application.platform switch
    {
        // Windows (Client, Dedicated Server, and Editor)
        RuntimePlatform.WindowsPlayer or RuntimePlatform.WindowsServer or RuntimePlatform.WindowsEditor => Environment.Is64BitProcess ? TargetPlatform.Windows64 : TargetPlatform.Windows32,

        // Mac
        RuntimePlatform.OSXPlayer or RuntimePlatform.OSXServer or RuntimePlatform.OSXEditor => TargetPlatform.Mac,

        // Linux
        RuntimePlatform.LinuxPlayer or RuntimePlatform.LinuxServer or RuntimePlatform.LinuxEditor => TargetPlatform.Linux64,

        // Mobile
        RuntimePlatform.IPhonePlayer => TargetPlatform.iOS,
        RuntimePlatform.Android => TargetPlatform.Android,

        // PlayStation
        RuntimePlatform.PS4 => TargetPlatform.PlayStation4,
        RuntimePlatform.PS5 => TargetPlatform.PlayStation5,

        // Xbox
        RuntimePlatform.XboxOne or RuntimePlatform.GameCoreXboxOne => TargetPlatform.XboxOne,
        RuntimePlatform.GameCoreXboxSeries => TargetPlatform.XboxSeries,

        // Nintendo
        RuntimePlatform.Switch => TargetPlatform.Switch,

        _ => throw new ArgumentOutOfRangeException("Application.platform", Application.platform, "The runtime cannot read asset bundles on unsupported platforms."),
    };
}
