#if !UNITY_EDITOR
using XPAB.Reader;
#endif

using XPAB.Settings;

namespace XPAB;

/// <summary>A configuration holder that dictates how an XPAB file is serialised.</summary>
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity convention.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
[CreateAssetMenu(menuName = "XPAB/Create Config", fileName = "xpab_config.asset")]
public sealed class XPABConfig : ScriptableObject
{
#if UNITY_EDITOR
    /// <summary>The name of the file.</summary>
    [Tooltip("The name of the file.")]
    public string fileName = string.Empty;

    /// <summary>The path where the file will be saved upon build completion.</summary>
    [Tooltip("The path where the file will be saved upon build completion.")]
    public string[] filePath = [];

    /// <summary>An array of platforms that the XPAB file is built for (for assets that cannot be made platform agnostic).</summary>
    [Tooltip("An array of platforms that the XPAB file is built for (for assets that cannot be made platform agnostic).")]
    public TargetPlatform[] targets = [];

    /// <summary>The font assets to include in this bundle.</summary>
    [Tooltip("The font assets to include in this bundle.")]
    public FontEntry[] fonts = [];

    /// <summary>The shader assets to include in this bundle.</summary>
    [Tooltip("The shader assets to include in this bundle.")]
    public ShaderEntry[] shaders = [];

    /// <summary>The text assets to include in this bundle.</summary>
    [Tooltip("The text assets to include in this bundle.")]
    public TextEntry[] texts = [];

    /// <summary>The binary assets to include in this bundle.</summary>
    [Tooltip("The binary assets to include in this bundle.")]
    public BinaryEntry[] binaries = [];

    /// <summary>The video clip assets to include in this bundle.</summary>
    [Tooltip("The video clip assets to include in this bundle.")]
    public VideoClipEntry[] videos = [];

    /// <summary>The TextMeshPro font assets to include in this bundle.</summary>
    [Tooltip("The TextMeshPro font assets to include in this bundle.")]
    public FontAssetEntry[] fontAssets = [];

    /// <summary>The mesh assets to include in this bundle.</summary>
    [Tooltip("The mesh assets to include in this bundle.")]
    public MeshEntry[] meshes = [];

    /// <summary>The sprite assets to include in this bundle.</summary>
    [Tooltip("The sprite assets to include in this bundle.")]
    public SpriteEntry[] sprites = [];

    /// <summary>The prefab (GameObject) assets to include in this bundle.</summary>
    [Tooltip("The prefab (GameObject) assets to include in this bundle.")]
    public PrefabEntry[] prefabs = [];

    /// <summary>The texture assets to include in this bundle.</summary>
    [Tooltip("The texture assets to include in this bundle.")]
    public Texture2DEntry[] textures = [];

    /// <summary>The material assets to include in this bundle.</summary>
    [Tooltip("The material assets to include in this bundle.")]
    public MaterialEntry[] materials = [];

    /// <summary>The audio clip assets to include in this bundle.</summary>
    [Tooltip("The audio clip assets to include in this bundle.")]
    public AudioClipEntry[] audioClips = [];

    /// <summary>The scriptable object assets to include in this bundle.</summary>
    [Tooltip("The scriptable object assets to include in this bundle.")]
    public ScriptableEntry[] scriptableObjects = [];

    /// <summary>The animation clip assets to include in this bundle.</summary>
    [Tooltip("The animation clip assets to include in this bundle.")]
    public AnimationClipEntry[] animationClips = [];

    /// <summary>The specific way an asset bundle is compressed when being built.</summary>
    [Tooltip("The specific way an asset bundle is compressed when being built.")]
    public AssetBundleCompressionType bundleCompression = AssetBundleCompressionType.ChunkedCompression;
#endif

    /// <summary>The settings that dictate how <see cref="AudioClip"/> assets are serialised.</summary>
#if UNITY_EDITOR
    [Tooltip("The settings that dictate how AudioClip assets are serialised.")]
#endif
    public AudioClipSettings audioSettings = new();

    /// <summary>The settings that dictate how <see cref="Texture2D"/> and <see cref="Sprite"/> assets are serialised.</summary>
#if UNITY_EDITOR
    [Tooltip("The settings that dictate how Texture2D and Sprite assets are serialised.")]
#endif
    public ImageSettings imageSettings = new();

    /// <summary>The settings that dictate how <see cref="Mesh"/> assets are serialised.</summary>
#if UNITY_EDITOR
    [Tooltip("The settings that dictate how Mesh assets are serialised.")]
#endif
    public MeshSettings meshSettings = new();

    /// <summary>The settings that dictate how <see cref="AnimationClip"/> assets are serialised.</summary>
#if UNITY_EDITOR
    [Tooltip("The settings that dictate how AnimationClip assets are serialised.")]
#endif
    public AnimationClipSettings animationSettings = new();

    /// <summary>The settings that dictate how <see cref="Material"/> assets are serialised.</summary>
#if UNITY_EDITOR
    [Tooltip("The settings that dictate how Material assets are serialised.")]
#endif
    public MaterialSettings materialSettings = new();

    /// <summary>The settings that dictate how <see cref="GameObject"/> and <see cref="ScriptableObject"/> assets are serialised.</summary>
#if UNITY_EDITOR
    [Tooltip("The settings that dictate how GameObject and ScriptableObject assets are serialised.")]
#endif
    public DataSettings dataSettings = new();

#if UNITY_EDITOR
    internal IAssetSettings[] GetAllSettings() => [audioSettings, imageSettings, meshSettings, animationSettings, materialSettings, dataSettings];

    internal void GetAllAssetPaths()
    {
        IAssetEntry[] entries = [.. fonts, .. shaders, .. texts, .. binaries, .. videos, .. fontAssets, .. meshes, .. sprites, .. prefabs, .. textures, .. materials, .. audioClips, .. scriptableObjects, .. animationClips];

        foreach (var entry in entries)
            entry.SetMetadata();
    }
#else
    internal void ReadAllSettings(XPABReader reader)
    {
        audioSettings = new();
        audioSettings.ReadFrom(reader);

        imageSettings = new();
        imageSettings.ReadFrom(reader);

        meshSettings = new();
        meshSettings.ReadFrom(reader);

        animationSettings = new();
        animationSettings.ReadFrom(reader);

        materialSettings = new();
        materialSettings.ReadFrom(reader);

        dataSettings = new();
        dataSettings.ReadFrom(reader);
    }
#endif
}
