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

    /// <summary>The assets to include in this bundle.</summary>
    [Tooltip("The assets to include in this bundle.")]
    public UObject[] assetsToBundle = [];

    /// <summary>The specific way an asset bundle is compressed when being built.</summary>
    [Tooltip("The specific way an asset bundle is compressed when being built.")]
    public AssetBundleCompressionType bundleCompression = AssetBundleCompressionType.FullyCompressed;
#endif

    /// <summary>How the texture bytes are compressed inside the XPAB file.</summary>
#if UNITY_EDITOR
    [Tooltip("How the texture bytes are compressed inside the XPAB file.")]
#endif
    public Texture2DCompressionMethod compressionMethod = Texture2DCompressionMethod.Brotli;

    /// <summary>The settings that dictate how <see cref="AudioClip"/> assets are serialised.</summary>
#if UNITY_EDITOR
    [Tooltip("The settings that dictate how AudioClip assets are serialised.")]
#endif
    public AudioSerialisationSettings audioSettings = new();
}
