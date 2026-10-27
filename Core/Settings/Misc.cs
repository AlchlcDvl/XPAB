#if UNITY_EDITOR
using XPAB.Writer;
#else
using XPAB.Reader;
#endif

namespace XPAB.Settings;

/// <summary>An interface used to mark a class as a settings type.</summary>
public interface IAssetSettings
{
#if UNITY_EDITOR
    /// <summary>Writes the absolute data of the current instance.</summary>
    /// <param name="writer">The writer to write the data to.</param>
    void WriteTo(XPABWriter writer);
#else
    /// <summary>Reads the absolute data to the current instance.</summary>
    /// <param name="reader">The reader to read the data from.</param>
    void ReadFrom(XPABReader reader);
#endif
}

/// <summary>An interface used to mark a class as a settings type that serialises a delta against global settings.</summary>
/// <typeparam name="T">The type of the asset.</typeparam>
public interface IAssetSettings<T> : IAssetSettings
    where T : IAssetSettings<T>
{
#if UNITY_EDITOR
    /// <summary>Writes a delta state of the current instance with respect to the global settings.</summary>
    /// <param name="writer">The writer to write the data to.</param>
    /// <param name="globalSettings">The global settings to serialise a delta against.</param>
    void WriteDeltaTo(XPABWriter writer, T globalSettings);
#else
    /// <summary>Reads a delta state to the current instance, falling back to global settings for missing fields.</summary>
    /// <param name="reader">The reader to read the data from.</param>
    /// <param name="globalSettings">The global settings to deserialise a delta against.</param>
    void ReadDeltaFrom(XPABReader reader, T globalSettings);
#endif
}

/// <summary>Specifies the compression type for asset bundles.</summary>
#if UNITY_EDITOR
[Tooltip("Specifies the compression type for asset bundles.")]
#endif
public enum AssetBundleCompressionType : byte
{
    /// <summary>The bundle is fully compressed, producing smaller file sizes but requires more decompression.</summary>
    FullyCompressed,

    /// <summary>There is no compression used.</summary>
    Uncompressed,

    /// <summary>Assets are compressed by chunks, producing a somewhat compressed file with a faster asset read time.</summary>
    ChunkedCompression,
}
