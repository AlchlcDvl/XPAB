# XPAB

Short for "Cross-Platform Asset Bundle", `xpab` is an attempt at creating a cross-platform asset bundle format for Unity modding.

Unity's native asset bundle system is inherently flawed for modding: if a game exists on multiple platforms (Windows, Mac, Linux, Android, etc), mod sizes bloat massively in an attempt to maintain compatibility because assets must be duplicated and compiled for each specific target.

This file type isn't perfect, it cannot be *truly* platform agnostic because engine-specific assets like shaders require specific instruction sets. This project aims to drastically reduce mod size bloat by isolating platform dependent data and generalizing everything else.

Say we have an asset bundle that is 10MB, of which 1MB is shaders. The target game runs on Windows, Mac, and Linux.\
Normally, to support all platforms, the modder must embed three separate 10MB bundles. The mod's size balloons to **30MB** (excluding other data within the dll, like code).\
If an `.xpab` file is used instead, the 9MB of generic assets (textures, audio, meshes) are stored once, alongside the three 1MB platform-specific shader bundles. The resulting mod size is **12MB** (9 + 1 + 1 + 1). That's a 60% reduction in file size!\
This is just a proof of concept, as numbers will be provided once the project is actually completed for the most part.

Best of all, these files are engineered to be both backward and forward compatible as much as possible.

---

## The Binary Format

Currently, this is the planned binary format of the `xpab` file. Feel free to suggest improvements until V1 is finalized:

```text
[XPAB]                (ASCII)
[File Version]        (LEB128)

-- Embedded Unity bundles for assets that cannot be made platform agnostic (eg, shaders)
[Target Count]        (Byte)
  [Target ID]         (Byte)
  [Byte Count]        (LEB128)
  [Bytes]             (Byte[])
  -- Repeats per platform target

[Master TOC Offset]   (Int64)

-- Asset Groups (Chunked by asset type)
[Asset TOC Offset]    (Int64)
  [Asset Metadata]    (Variable depending on type)
  [Byte Count]        (LEB128)
  [Bytes]             (Byte[]) -- Conditionally written if there is associated file data (eg, videos); Byte Count is set to -1 otherwise
  -- Repeats per asset

-- Asset Table of Contents
[Asset Count]         (LEB128)
  [Entry Length]      (LEB128)
  [Path]              (String)
  [File Name]         (String)
  [Asset Offset]      (Int64)
  [Raw Length]        (LEB128)
  [IsCompressed]      (Boolean)
  [Compressed Length] (LEB128) -- Conditionally written if IsCompressed is true
  -- Repeats per asset
-- Repeats per asset type group

-- Master Table of Contents
[Type Count]          (Byte)
  [Entry Length]      (LEB128)
  [Type ID]           (Byte)
  [Type Version]      (LEB128)
  [Group Offset]      (Int64)
  [Extension Count]   (LEB128)
    [File Extension]  (String)
  -- Repeats per asset type

-- Validation Tail
[Checksum]            (Byte[])
[BAPX]                (ASCII)
```

---

## Building the Project

Due to the nature of the Unity modding scene, where games run on vastly different engine versions and use different interop libraries, I cannot provide pre-compiled, versioned releases. You must compile the project yourself against the specific DLLs of your target game. I'm sure you already know how to.

There are pre-configured (and gitignored) reference folders where you can drop your target game's DLLs. Ensure you select the correct scripting backend (Mono vs IL2Cpp) via the `.csproj` targets or build arguments (adding `/p:CompileTarget=Mono` or `/p:CompileTarget=Il2Cpp`).

Here are the libraries you need to extract and place into their respective folders:

`UnityEditor` (Found inside your Unity Editor installation: `Editor/Data/Managed/`):
* `UnityEditor.dll`

`UnityMono` (Found inside your target game's directory: `[GameName]_Data/Managed/`):
* `UnityEngine.dll`
* `UnityEngine.AssetBundleModule.dll`
* `UnityEngine.AudioModule.dll`
* `UnityEngine.CoreModule.dll`
* `UnityEngine.ImageConversionModule.dll`
* `UnityEngine.TextRenderingModule.dll`

`UnityIl2Cpp` (Found inside your target game's directory: `BepInEx/interop/` or `MelonLoader/Il2CppAssemblies/`)
* All the `UnityEngine.*` DLLs listed in the Mono section above.
* `Il2CppInterop.Runtime.dll`

This project is a work-in-progress! Feel free to contribute!
