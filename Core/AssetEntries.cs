using UnityEngine.Video;
using XPAB.Assets;
using XPAB.Settings;

namespace XPAB;

/// <summary>An interface used to mark a class as an asset entry.</summary>
public interface IAssetEntry
{
#if UNITY_EDITOR
    /// <summary>Gets the path of the asset.</summary>
    string AssetPath { get; }

    /// <summary>Sets the asset path of the entry.</summary>
    void SetMetadata();
#endif
}

/// <summary>The base class for an asset entry in an XPAB configuration.</summary>
/// <typeparam name="TAsset">The type of the asset being bundled.</typeparam>
[Serializable]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity convention.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
public abstract class AssetEntry<TAsset> : IAssetEntry
    where TAsset : UObject
{
    /// <summary>The asset to be bundled.</summary>
#if UNITY_EDITOR
    [Tooltip("The asset to be bundled.")]
#endif
    public TAsset asset;

#if UNITY_EDITOR
    [NonSerialized]
#endif
    internal string assetPath;

#if UNITY_EDITOR
    /// <inheritdoc/>
    string IAssetEntry.AssetPath => assetPath;

    /// <inheritdoc/>
    public virtual void SetMetadata() => assetPath = AssetDatabase.GetAssetPath(asset);
#endif
}

/// <summary>The base class for an asset entry that includes specific local serialisation settings.</summary>
/// <typeparam name="TAsset">The type of the asset being bundled.</typeparam>
/// <typeparam name="TSettings">The type of the settings used to serialise the asset.</typeparam>
[Serializable]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity convention.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
public abstract class AssetEntry<TAsset, TSettings> : AssetEntry<TAsset>
    where TAsset : UObject
    where TSettings : IAssetSettings<TSettings>
{
    /// <summary>The local serialisation settings for this specific asset. If left unset, the global configuration settings will be used.</summary>
#if UNITY_EDITOR
    [Tooltip("The local serialisation settings for this specific asset. If left unset, the global configuration settings will be used.")]
    internal Optional<TSettings> settings;
#else
    public TSettings? settings;
#endif
}

/// <summary>An entry for a standard Unity <see cref="Font"/> asset.</summary>
#if UNITY_EDITOR
[Tooltip("An entry for a standard Unity Font asset.")]
#endif
[Serializable]
public sealed class FontEntry : AssetEntry<Font>;

/// <summary>An entry for a compiled <see cref="Shader"/> asset.</summary>
#if UNITY_EDITOR
[Tooltip("An entry for a compiled Shader asset.")]
#endif
[Serializable]
public sealed class ShaderEntry : AssetEntry<Shader>;

/// <summary>An entry for a raw <see cref="TextAsset"/> string payload.</summary>
#if UNITY_EDITOR
[Tooltip("An entry for a raw TextAsset string payload.")]
#endif
[Serializable]
public sealed class TextEntry : AssetEntry<TextAsset>;

/// <summary>An entry for a raw <see cref="BinaryAsset"/> byte payload.</summary>
#if UNITY_EDITOR
[Tooltip("An entry for a raw BinaryAsset byte payload.")]
#endif
[Serializable]
public sealed class BinaryEntry : AssetEntry<BinaryAsset>;

/// <summary>An entry for a streaming <see cref="VideoClip"/> asset.</summary>
#if UNITY_EDITOR
[Tooltip("An entry for a streaming VideoClip asset.")]
#endif
[Serializable]
public sealed class VideoClipEntry : AssetEntry<VideoClip>;

/// <summary>An entry for a TextMeshPro <see cref="TMP_FontAsset"/>.</summary>
#if UNITY_EDITOR
[Tooltip("An entry for a TextMeshPro TMP_FontAsset.")]
#endif
[Serializable]
public sealed class FontAssetEntry : AssetEntry<TMP_FontAsset>;

/// <summary>An entry for a <see cref="Mesh"/> asset with configurable quantization and stripping.</summary>
#if UNITY_EDITOR
[Tooltip("An entry for a Mesh asset with configurable quantization and stripping.")]
#endif
[Serializable]
public sealed class MeshEntry : AssetEntry<Mesh, MeshSettings>;

/// <summary>An entry for a <see cref="Sprite"/> asset with configurable compression and encoding.</summary>
#if UNITY_EDITOR
[Tooltip("An entry for a Sprite asset with configurable compression and encoding.")]
#endif
[Serializable]
public sealed class SpriteEntry : AssetEntry<Sprite, ImageSettings>;

/// <summary>An entry for a <see cref="GameObject"/> prefab hierarchy and its serialised components.</summary>
#if UNITY_EDITOR
[Tooltip("An entry for a GameObject prefab hierarchy and its serialised components.")]
#endif
[Serializable]
public sealed class PrefabEntry : AssetEntry<GameObject, DataSettings>;

/// <summary>An entry for a <see cref="Texture2D"/> asset with configurable compression and encoding.</summary>
#if UNITY_EDITOR
[Tooltip("An entry for a Texture2D asset with configurable compression and encoding.")]
#endif
[Serializable]
public sealed class Texture2DEntry : AssetEntry<Texture2D, ImageSettings>;

/// <summary>An entry for a <see cref="Material"/> asset and its shader resolution overrides.</summary>
#if UNITY_EDITOR
[Tooltip("An entry for a Material asset and its shader resolution overrides.")]
#endif
[Serializable]
public sealed class MaterialEntry : AssetEntry<Material, MaterialSettings>;

/// <summary>An entry for an <see cref="AudioClip"/> asset with configurable sample rates and compression.</summary>
#if UNITY_EDITOR
[Tooltip("An entry for an AudioClip asset with configurable sample rates and compression.")]
#endif
[Serializable]
public sealed class AudioClipEntry : AssetEntry<AudioClip, AudioClipSettings>;

/// <summary>An entry for a <see cref="ScriptableObject"/> asset data block.</summary>
#if UNITY_EDITOR
[Tooltip("An entry for a ScriptableObject asset data block.")]
#endif
[Serializable]
public sealed class ScriptableEntry : AssetEntry<ScriptableObject, DataSettings>;

/// <summary>An entry for an <see cref="AnimationClip"/> asset with configurable keyframe reduction.</summary>
#if UNITY_EDITOR
[Tooltip("An entry for an AnimationClip asset with configurable keyframe reduction.")]
#endif
[Serializable]
public sealed class AnimationClipEntry : AssetEntry<AnimationClip, AnimationClipSettings>;
