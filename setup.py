import os
import shutil

from pathlib import Path

def deploy_file(src: Path, dst_dir: Path):
    if not src.exists():
        print(f"Warning: Source not found: {src}")
        return

    dst_dir.mkdir(parents=True, exist_ok=True)
    dst_file = dst_dir / src.name

    if dst_file.exists() or dst_file.is_symlink():
        try:
            dst_file.unlink()
        except OSError:
            pass # Skip if file is locked or cannot be removed

    try:
        os.symlink(src, dst_file)
        print(f"Linked: {dst_dir.as_posix()}/{dst_file.name} -> {src}")
    except OSError:
        try:
            shutil.copy2(src, dst_file)
            print(f"Copied (Symlink Fallback): {dst_dir.as_posix()}/{dst_file.name}")
        except Exception as e:
            print(f"Failed to copy {dst_file.name}. Error: {e}")

    # Automatically check for and deploy companion XML documentation
    if src.suffix.lower() == '.dll':
        xml_src = src.with_suffix('.xml')
        if xml_src.exists():
            deploy_file(xml_src, dst_dir)

# Configuration Prompts
game_dir = Path(input("Enter Game Directory: ").strip('"\''))
editor_dir = Path(input("Enter Unity Editor Directory (e.g., C:/.../2022.3.0f1): ").strip('"\''))
mod_loader = input("Mod Loader (BepInEx/MelonLoader/None): ").strip().lower()
arch = input("Architecture (Mono/Il2Cpp): ").strip().lower()

editor_dlls = [
    "UnityEditor.dll",
    "UnityEngine.IMGUIModule.dll"
]

unity_base_dlls = [
    "UnityEngine.dll",
    "UnityEngine.AnimationModule.dll",
    "UnityEngine.AssetBundleModule.dll",
    "UnityEngine.AudioModule.dll",
    "UnityEngine.CoreModule.dll",
    "UnityEngine.ImageConversionModule.dll",
    "UnityEngine.TextRenderingModule.dll",
    "UnityEngine.VideoModule.dll",
    "Unity.TextMeshPro.dll"
]

il2cpp_interop_dlls = [
    "Il2CppInterop.Runtime.dll",
    "Il2Cppmscorlib.dll"
]

bepinex_core_dlls = [
    "BepInEx.Core.dll",
    "BepInEx.Unity.Common.dll"
]

# Set the base library directory
libs_dir = Path("Libs")

# Deploy Editor DLLs
editor_managed = editor_dir / "Editor" / "Data" / "Managed"
for dll in editor_dlls:
    deploy_file(editor_managed / dll, libs_dir / "UnityEditor")

# Deploy Unity Engine & Il2Cpp DLLs
if arch == "mono":
    managed_dir = next((d / "Managed" for d in game_dir.iterdir() if d.is_dir() and d.name.endswith("_Data")), None)
    if managed_dir and managed_dir.exists():
        for dll in unity_base_dlls:
            deploy_file(managed_dir / dll, libs_dir / "UnityMono")
    else:
        print("Could not locate [GameName]_Data/Managed directory.")

elif arch == "il2cpp":
    unity_il2cpp_dir = libs_dir / "UnityIl2Cpp"
    il2cpp_dir = libs_dir / "Il2Cpp"

    if mod_loader == "bepinex":
        game_interop_dir = game_dir / "BepInEx" / "interop"
        game_core_dir = game_dir / "BepInEx" / "core"

        # Route Unity.* DLLs
        for dll in unity_base_dlls:
            src = game_interop_dir / dll if (game_interop_dir / dll).exists() else game_core_dir / dll
            deploy_file(src, unity_il2cpp_dir)

        # Route Il2Cpp interop DLLs
        for dll in il2cpp_interop_dlls:
            src = game_interop_dir / dll if (game_interop_dir / dll).exists() else game_core_dir / dll
            deploy_file(src, il2cpp_dir)

    elif mod_loader == "melonloader":
        game_il2cpp_dir = game_dir / "MelonLoader" / "Il2CppAssemblies"

        for dll in unity_base_dlls:
            deploy_file(game_il2cpp_dir / dll, unity_il2cpp_dir)
        for dll in il2cpp_interop_dlls:
            deploy_file(game_il2cpp_dir / dll, il2cpp_dir)

# Deploy Mod Loader DLLs
if mod_loader == "bepinex":
    bep_core = game_dir / "BepInEx" / "core"
    for dll in bepinex_core_dlls:
        deploy_file(bep_core / dll, libs_dir / "BepInEx")

    if arch == "mono":
        deploy_file(bep_core / "BepInEx.Unity.Mono.dll", libs_dir / "BepMono")
    elif arch == "il2cpp":
        deploy_file(bep_core / "BepInEx.Unity.IL2CPP.dll", libs_dir / "BepIl2Cpp")

elif mod_loader == "melonloader":
    melon_net = game_dir / "MelonLoader" / "net6"
    deploy_file(melon_net / "MelonLoader.dll", libs_dir / "MelonLoader")

print("\nSetup complete. Check console for any missing source warnings.")
