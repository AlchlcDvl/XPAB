#if BEPINEX
// This code uses BepInEx6, will need changes to match older stable versions

using BepInEx;
using BepInEx.Configuration;
using XPAB.Internal;

#if IL2CPP
using BepInEx.Unity.IL2CPP;
#else
using BepInEx.Unity.Mono; // I assume that this is the namespace, please tell me if I'm wrong
#endif

namespace XPAB;

[BepInPlugin(XPABPlugin.Id, XPABPlugin.Name, XPABPlugin.Version)]
[BepInProcess("Target Game.exe")]
public sealed partial class XPABPlugin : BasePlugin
{
    public override void Load()
    {
        XPABLogger.SetAll(Log.LogDebug, Log.LogMessage, Log.LogWarning, Log.LogError);
        XPABCore.Initialise();
    }
}
#endif
