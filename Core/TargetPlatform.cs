namespace XPAB;

/// <summary>An enum that denotes the target platform for an asset bundle.</summary>
public enum TargetPlatform : byte
{
    /// <summary>Microsoft's Windows OS (32-bit).</summary>
    Windows32,

    /// <summary>Microsoft's Windows OS (64-bit).</summary>
    Windows64,

    /// <summary>Apple's Mac OS (64-bit/Universal).</summary>
    Mac,

    /// <summary>Linus' Linux Kernel (64-bit).</summary>
    Linux64,

    /// <summary>Sony's PlayStation 4.</summary>
    PlayStation4,

    /// <summary>Sony's PlayStation 5.</summary>
    PlayStation5,

    /// <summary>Microsoft's Xbox One.</summary>
    XboxOne,

    /// <summary>Microsoft's Xbox Series X/S.</summary>
    XboxSeries,

    /// <summary>Google's Android OS.</summary>
    Android,

    /// <summary>Apple's iOS.</summary>
    iOS,

    /// <summary>Nintendo's Switch.</summary>
    Switch,
}
