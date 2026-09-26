#if MELON_LOADER
// This uses Melon Loader 0.7 and over, changes will need to be made for lower versions

using MelonLoader;
using MelonLoader.Utils;
using XPAB.Internal;

[assembly: MelonPriority(-20000)]
[assembly: MelonInfo(typeof(XPAB.XPABPlugin), XPAB.XPABPlugin.Name, XPAB.XPABPlugin.Version, XPAB.XPABPlugin.Author)]
[assembly: MelonGame]
[assembly: MelonColor(255, 255, 255, 255)]
[assembly: HarmonyDontPatchAll]

namespace XPAB;

public sealed partial class XPABPlugin : MelonMod
{
    public override void OnInitializeMelon()
    {
        XPABLogger.SetAll(LoggerInstance.Msg, LoggerInstance.Warning, LoggerInstance.Error);
        XPABCore.Initialise();
    }
}
#endif
