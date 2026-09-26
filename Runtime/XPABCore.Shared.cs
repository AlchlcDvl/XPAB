#if BEPINEX || MELON_LOADER || CUSTOM_LOADER

namespace XPAB;

/// <summary>The core BepInEx plugin for loading XPAB files.</summary>
public sealed partial class XPABPlugin
{
    /// <summary>Id of the <see cref="XPABPlugin"/>.</summary>
    public const string Id = "alchlc.xpab";

    /// <summary>Name of the <see cref="XPABPlugin"/>.</summary>
    public const string Name = "XPAB";

    /// <summary>Version of the <see cref="XPABPlugin"/>.</summary>
    public const string Version = "0.0.0.1";

    /// <summary>Author of the <see cref="XPABPlugin"/>.</summary>
    public const string Author = "AlchlcSystm";
}
#endif
