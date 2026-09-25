namespace XPAB;

/// <summary>An enum that denotes which asset type an asset is.</summary>
public enum AssetType : byte
{
    /// <summary><see cref="UnityEngine.TextAsset"/>.</summary>
    TextAsset,

    /// <summary><see cref="XPAB.Assets.BinaryAsset"/>.</summary>
    BinaryAsset,

    /// <summary><see cref="UnityEngine.Texture"/>.</summary>
    Texture,

    /// <summary><see cref="UnityEngine.AudioClip"/>.</summary>
    AudioClip,

    /// <summary><see cref="UnityEngine.Shader"/>.</summary>
    Shader,

    /// <summary><see cref="UnityEngine.Video.VideoClip"/>.</summary>
    VideoClip,

    /// <summary><see cref="UnityEngine.Font"/>.</summary>
    Font,

    /// <summary><see cref="TMPro.TMP_FontAsset"/>.</summary>
    FontAsset,

    /// <summary><see cref="UnityEngine.Mesh"/>.</summary>
    Mesh,

    /// <summary><see cref="UnityEngine.Material"/>.</summary>
    Material,

    /// <summary><see cref="UnityEngine.Sprite"/>.</summary>
    Sprite,

    /// <summary><see cref="UnityEngine.AnimationClip"/>.</summary>
    AnimationClip,

    /// <summary><see cref="ScriptableObject"/>.</summary>
    DataConfig,

    /// <summary><see cref="GameObject"/>.</summary>
    Prefab,
}
