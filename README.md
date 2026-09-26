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
- [x] Handle serialising asset bundles into the file
- [ ] Handle deserialising asset bundles from a file
- [ ] Handle serialisation and deserialisation of assets (see table below)
- [ ] Add checksum behaviour
- [ ] Finalise binary
- [ ] Test
- [ ] Update README with actual numbers from tests

| Asset Type                        | Serialisable | Deserialisable |
| --------------------------------- | ------------ | -------------- |
| `TextAsset`                       | [x]          | [x]            |
| `BinaryAsset`                     | [x]          | [x]            |
| `Texture`                         | [ ]          | [ ]            |
| `AudioClip`                       | [x]          | [x]            |
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

Due to the nature of the Unity modding scene, where games run on vastly different engine versions and use different interop libraries, I cannot provide pre-compiled, versioned releases. You must compile the project yourself against the specific DLLs of your target game and mod loader.

There are pre-configured (and gitignored) reference folders where you can drop your target game and mod loader's DLLs.

### Configuration

The project has two build properties to make use of when compiling the project.
- `ModLoader`: Choose between `None` (Standalone, useful for if you want to load it yourself), `BepInEx`, `MelonLoader` and `Custom` (for non-conventional mod loaders, you must write the initialisation logic yourself for this target). This lets the asset reader to be loaded either manually, or by the mod loader it's compiled against.
- `CompileTarget`: Choose between `Mono` and `Il2Cpp` when compiling the project. This corresponds to the scripting backends of Unity with the same name. There are key differences between the two, so it's important that asset creation match the backend exactly lest you feel the wrath of the engine.

Here are the libraries you need to extract and place into their respective folders:

`UnityEditor` (Found in: `[Editor Installation]/Editor/Data/Managed/`):
- `UnityEditor.dll`
- `UnityEditor.CoreModule.dll` (only if some methods are missing)

`UnityMono` (Found in: `[Game Installation]/[GameName]_Data/Managed/`):
- `UnityEngine.dll`
- `UnityEngine.AnimationModule.dll`
- `UnityEngine.AssetBundleModule.dll`
- `UnityEngine.AudioModule.dll`
- `UnityEngine.CoreModule.dll`
- `UnityEngine.ImageConversionModule.dll`
- `UnityEngine.TextRenderingModule.dll`
- `UnityEngine.VideoModule.dll`
- `Unity.TextMeshPro.dll`

`UnityIl2Cpp` (Found in: `[Game Installation]/BepInEx/interop/`, `[Game Installation]/BepInEx/core/` or `[Game Installation]/MelonLoader/Il2CppAssemblies/`)
- All the `UnityEngine.*` and `Unity.*` DLLs listed in the Mono section above.
- `Il2CppInterop.Runtime.dll`
- `Il2Cppmscorlib.dll`

`MelonLoader` (Found in: `[Game Installation]/MelonLoader/net6/`)
- `MelonLoader.dll`

`BepInEx` (Found in: `[Game Installation]/BepInEx/core/`)
- `BepInEx.Core.dll`
- `BepInEx.Unity.Common.dll`
- `BepInEx.Unity.IL2CPP.dll` (for Il2Cpp games)
- `BepInEx.Unity.Mono.dll` (for Mono games)

For the `Custom` loader configuration, navigate to the respective folders and copy them into the `CustomLoader` folder.

Make sure to copy over any other dlls that the aforementioned ones depend on as well.

This project is a work-in-progress! Feel free to contribute! Check back frequently for updates!

---

## The Binary Format

Currently, this is the planned binary format of the `xpab` file. Feel free to suggest improvements until V1 is finalized:

```text
-- Header
[XPAB]                  (ASCII, 4 Bytes)
[File Version]          (ULEB128)

-- Embedded Unity bundles for assets that cannot be made platform agnostic (eg, shaders)
[Asset Bundle Version]  (ULEB128) -- Used to note if a change was made to the asset bundle build settings
[Target Count]          (ULEB128)

  [Target ID]           (Byte)
  [Byte Count]          (ULEB128)
  [Bytes]               (Byte[])
  -- Repeats per platform target

-- String pool to avoid repeated strings
[String Count]          (ULEB128)

  [String]              (String) -- Future string writes will write a packed index instead of the actual string itself, ULEB128 prefixed UTF-8

[Master TOC Pos]        (Int64)

  -- Asset Groups (Chunked by asset type)
  [Asset Group TOC Pos] (Int64)

    -- Assets
    [Asset Data Length] (SLEB128) -- -1 for no metadata
    [Asset Metadata]    (Variable)
    [Byte Count]        (SLEB128) -- -1 for no file data
    [Bytes]             (Byte[])
    -- Repeats per asset

  -- Asset Group Table of Contents
  [Asset Count]         (ULEB128)

    -- Asset Entry
    [Entry Length]      (ULEB128)
    [Path]              (ULEB128)
    [File Name]         (ULEB128)
    [File Extension]    (SByte) -- Refers to the index in the Master TOC's file extension array, -1 for no extension
    [Asset Pos]         (Int64)
    -- Repeats per asset

  -- Repeats per asset type group

-- Master Table of Contents
[Type Count]            (Byte)

  -- Group Entry
  [Entry Length]        (ULEB128)
  [Type ID]             (Byte)
  [Type Version]        (ULEB128)
  [Group Pos]           (Int64)
  [Extension Count]     (ULEB128)

    [File Extension]    (ULEB128)
    -- Repeats per extension

  -- Repeats per asset type

-- Footer
[Checksum]              (Byte[]) -- SHA-256
[BAPX]                  (ASCII, 4 Bytes)
```

---

## License

This project is provided under the MIT license.
