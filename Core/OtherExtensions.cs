namespace XPAB;

internal static partial class OtherExtensions
{
    public static IEnumerable<T> Except<T>(this IEnumerable<T> source, Func<T, bool> predicate) => source.Where(x => !predicate(x));

    public static string GetShortForm(this TargetPlatform target) => target switch
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
