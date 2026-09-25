using XPAB.Internal;

#if IL2CPP
using Il2CppInterop.Runtime.Injection;
using XPAB.Assets;
#endif

namespace XPAB.Core;

/// <summary>The core class of the XPAB file handler.</summary>
public static class XPABCore
{
    /// <summary>Initialises the core components of the handler.</summary>
    /// <remarks>Make sure that this is invoked before you load ANY asset.</remarks>
    public static void Initialise()
    {
#if IL2CPP
        ClassInjector.RegisterTypeInIl2Cpp<BinaryAsset>();
#endif

        XPABLogger.Info("Initialised!");
    }
}
