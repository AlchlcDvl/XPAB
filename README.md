# XPAB

Short for "Cross-Platform Asset Bundle", `xpab` is an attempt at creating a cross-platform asset bundle format for Unity modding in C#.

Unity's native asset bundle system is inherently flawed for modding: if a game exists on multiple platforms (Windows, Mac, Linux, Android, etc), mod sizes bloat massively in an attempt to maintain compatibility because assets must be duplicated and compiled for each specific target.

This file type isn't perfect, it cannot be *truly* platform agnostic because engine-specific assets like shaders require specific instruction sets. This project aims to drastically reduce mod size bloat by isolating platform dependent data and generalizing everything else.

Say we have an asset bundle that is 10MB, of which 1MB is shaders. The target game runs on Windows, Mac, and Linux.

Normally, to support all platforms, the modder must embed three separate 10MB bundles. The mod's size balloons to **30MB** (excluding other data within the dll, like code).

If an `.xpab` file is used instead, the 9MB of generic assets (textures, audio, meshes) are stored once, alongside the three 1MB platform-specific shader bundles. The resulting mod size is **12MB** (9 + 1 + 1 + 1). That's a 60% reduction in file size!

This is just a proof of concept, as numbers will be provided once the project is actually completed for the most part.

Best of all, these files are engineered to be both backward and forward compatible as much as possible.

The project comes with an Editor dll (the serialiser) for you to drop into your editor to begin compiling, and a Runtime dll (the deserialiser) for you to load into BepInEx/MelonLoader (can be loaded by either) and get started with loading assets.

---

## Supported Asset Types

The following are assets that are planned to have support from the start. There will be more assets supported in the future:

- `TextAsset`
- `BinaryAsset`
- `Texture`
- `AudioClip`
- `Shader`
- `VideoClip`
- `Font`
- `TMP_FontAsset`
- `Mesh`
- `Material`
- `Sprite`
- `AnimationClip`
- Data configs (`ScriptableObject`)
- Prefabs (`GameObject`)

---

## TODO

- [x] Set up project and configurations
- [ ] Set up build options in Editor project
- [ ] Handle serialisation and deserialisation of assets (see table below)
- [ ] Add checksum behaviour
- [ ] Finalise binary
- [ ] Test
- [ ] Update README with actual numbers from tests

| Asset Type                        | Serialisable | Deserialisable |
| --------------------------------- | ------------ | -------------- |
| `TextAsset`                       | [ ]          | [ ]            |
| `BinaryAsset`                     | [ ]          | [ ]            |
| `Texture`                         | [ ]          | [ ]            |
| `AudioClip`                       | [ ]          | [ ]            |
| `Shader`                          | [ ]          | [ ]            |
| `VideoClip`                       | [ ]          | [ ]            |
| `Font`                            | [ ]          | [ ]            |
| `TMP_FontAsset`                   | [ ]          | [ ]            |
| `Mesh`                            | [ ]          | [ ]            |
| `Material`                        | [ ]          | [ ]            |
| `Sprite`                          | [ ]          | [ ]            |
| `AnimationClip`                   | [ ]          | [ ]            |
| Data configs (`ScriptableObject`) | [ ]          | [ ]            |
| Prefabs (`GameObject`)            | [ ]          | [ ]            |

---

## Building the Project

Due to the nature of the Unity modding scene, where games run on vastly different engine versions and use different interop libraries, I cannot provide pre-compiled, versioned releases. You must compile the project yourself against the specific DLLs of your target game. I'm sure you already know how to.

There are pre-configured (and gitignored) reference folders where you can drop your target game's DLLs. Ensure you select the correct scripting backend (Mono vs IL2Cpp) via the `.csproj` targets or build arguments (adding `/p:CompileTarget=Mono` or `/p:CompileTarget=Il2Cpp`).

Here are the libraries you need to extract and place into their respective folders:

`UnityEditor` (Found inside your Unity Editor installation: `Editor/Data/Managed/`):
- `UnityEditor.dll`

`UnityMono` (Found inside your target game's directory: `[GameName]_Data/Managed/`):
- `UnityEngine.dll`
- `UnityEngine.AnimationModule.dll`
- `UnityEngine.AssetBundleModule.dll`
- `UnityEngine.AudioModule.dll`
- `UnityEngine.CoreModule.dll`
- `UnityEngine.ImageConversionModule.dll`
- `UnityEngine.TextRenderingModule.dll`
- `UnityEngine.VideoModule.dll`

`UnityIl2Cpp` (Found inside your target game's directory: `BepInEx/interop/`, `BepInEx/core/` or `MelonLoader/Il2CppAssemblies/`)
- All the `UnityEngine.*` DLLs listed in the Mono section above.
- `Il2CppInterop.Runtime.dll`
- `Il2Cppmscorlib.dll`

This project is a work-in-progress! Feel free to contribute! Check back frequently for updates!

---

## The Binary Format

Currently, this is the planned binary format of the `xpab` file. Feel free to suggest improvements until V1 is finalized:

```text
-- Header
[XPAB]                  (ASCII)
[File Version]          (ULEB128)

-- Embedded Unity bundles for assets that cannot be made platform agnostic (eg, shaders)
[Target Count]          (Byte)

  [Target ID]           (Byte)
  [Byte Count]          (ULEB128)
  [Bytes]               (Byte[])
  -- Repeats per platform target

-- String pool to avoid repeated strings
[String Count]          (ULEB128)

  [String]              (String) -- Future string writes will write a packed index instead of the actual string itself

[Master TOC Pos]        (Int64)

  -- Asset Groups (Chunked by asset type)
  [Asset Group TOC Pos] (Int64)

    -- Assets
    [Asset Data Length] (SLEB128) -- -1 is used to denote if the asset has no metadata
    [Asset Metadata]    (Variable)
    [Byte Count]        (SLEB128) -- -1 is used to denote if the asset has no file data
    [Bytes]             (Byte[])
    -- Repeats per asset

  -- Asset Group Table of Contents
  [Asset Count]         (ULEB128)

    [Entry Length]      (ULEB128)
    [Path]              (ULEB128)
    [File Name]         (ULEB128)
    [File Extension]    (SByte) -- Refers to the index in the Master TOC's file extension array
    [Asset Pos]         (Int64)
    [Raw Length]        (ULEB128)
    [Compressed Length] (SLEB128) -- -1 if no compression is used, non-negative otherwise (uses LZ4)
    -- Repeats per asset

  -- Repeats per asset type group

-- Master Table of Contents
[Type Count]            (Byte)

  [Entry Length]        (ULEB128)
  [Type ID]             (Byte)
  [Type Version]        (ULEB128)
  [Group Pos]           (Int64)
  [Extension Count]     (ULEB128)

    [File Extension]    (ULEB128)
    -- Repeats per extension

  -- Repeats per asset type

-- Footer
[Checksum]              (Byte[])
[BAPX]                  (ASCII)
```

A couple things to note:

- The binary will be little endian
- Strings are written in a length prefixed (ULEB128) UTF-8 format
- The checksum is a SHA-256 calculation of bytes starting from the first byte of the bundle count up until the last byte of the master TOC.

---

## License

This project is provided under the MIT license.
