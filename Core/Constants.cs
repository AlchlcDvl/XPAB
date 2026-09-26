namespace XPAB;

/// <summary>The constants for each asset type's serialisation version.</summary>
public static class Constants
{
    /// <summary>The overarching serialisation version of the XPAB file format.</summary>
    public const uint FileVersion = 1;

    /// <summary>The serialisation version for embedded, platform-specific Unity asset bundles.</summary>
    public const uint AssetBundleVersion = 1;

    /// <summary>The serialisation version for <see cref="TextAsset"/> data.</summary>
    public const uint TextAssetVersion = 1;

    /// <summary>The serialisation version for <see cref="Texture"/> data.</summary>
    public const uint TextureVersion = 1;

    /// <summary>The serialisation version for <see cref="AudioClip"/> data.</summary>
    public const uint AudioClipVersion = 1;

    /// <summary>The serialisation version for <see cref="Shader"/> data.</summary>
    public const uint ShaderVersion = 1;

    /// <summary>The serialisation version for <see cref="UnityEngine.Video.VideoClip"/> data.</summary>
    public const uint VideoClipVersion = 1;

    /// <summary>The serialisation version for standard <see cref="Font"/> data.</summary>
    public const uint FontVersion = 1;

    /// <summary>The serialisation version for <see cref="TMPro.TMP_FontAsset"/> data.</summary>
    public const uint FontAssetVersion = 1;

    /// <summary>The serialisation version for <see cref="Mesh"/> data.</summary>
    public const uint MeshVersion = 1;

    /// <summary>The serialisation version for <see cref="Sprite"/> data.</summary>
    public const uint SpriteVersion = 1;

    /// <summary>The serialisation version for <see cref="AnimationClip"/> data.</summary>
    public const uint AnimationClipVersion = 1;

    /// <summary>The serialisation version for <see cref="ScriptableObject"/> data.</summary>
    public const uint DataConfigVersion = 1;

    /// <summary>The serialisation version for <see cref="GameObject"/> prefabs.</summary>
    public const uint PrefabVersion = 1;

    /// <summary>The default file extension used for native Unity asset files.</summary>
    public const string AssetFileExtension = "asset";

    /// <summary>The magic ASCII string used as a header to mark the start of a valid XPAB file.</summary>
    public const string Header = "XPAB";

    /// <summary>The magic ASCII string used as a footer to mark the end of a valid XPAB file.</summary>
    public const string Footer = "BAPX";

    /// <summary>Gets a dictionary mapping Unity asset types to arrays of their supported file extensions.</summary>
    public static Dictionary<Type, string[]> FileExtensions { get; } = new()
    {
        // To be filled
    };
}
