#if UNITY_EDITOR
using XPAB.Writer;
#else
using XPAB.Reader;
#endif

namespace XPAB.Settings;

/// <summary>A configuration holder that dictates how <see cref="Texture2D"/>s are serialised.</summary>
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity convention.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
[Serializable]
#if UNITY_EDITOR
[Tooltip("A configuration holder that dictates how Texture2Ds are serialised.")]
#endif
public sealed class ImageSettings : IAssetSettings<ImageSettings>
{
    /// <summary>The compression format used for image byte streams.</summary>
#if UNITY_EDITOR
    [Tooltip("The compression format used for image byte streams.")]
#endif
    public Texture2DCompressionAlgorithm compressionAlgorithm = Texture2DCompressionAlgorithm.Brotli;

    /// <summary>The encoding format used for image bytes. The compression algorithm setting is mutually exclusive when this setting is set to PNG, JPG or EXR.</summary>
#if UNITY_EDITOR
    [Tooltip("The encoding format used for image bytes. The compression algorithm setting is mutually exclusive when this setting is set to PNG or JPG.")]
#endif
    public Texture2DEncoding encoding = Texture2DEncoding.Native;

    /// <summary>Quality setting when using lossy JPG encoding (1 - 100).</summary>
#if UNITY_EDITOR
    [Tooltip("Quality setting when using lossy JPG encoding (1 - 100).")]
    [Range(1, 100)]
#endif
    public int lossyQuality = 85;

    /// <summary>Whether the texture stays readable in CPU RAM after uploading to the GPU.</summary>
#if UNITY_EDITOR
    [Tooltip("Whether the texture stays readable in CPU RAM after uploading to the GPU.")]
#endif
    public bool readableAtRuntime = false;

#if UNITY_EDITOR
    /// <inheritdoc/>
    public void WriteTo(XPABWriter writer)
    {
        writer.WriteEnum(compressionAlgorithm);
        writer.WriteEnum(encoding);
        writer.Write(lossyQuality);
        writer.Write(readableAtRuntime);
    }

    /// <inheritdoc/>
    public void WriteDeltaTo(XPABWriter writer, ImageSettings globalSettings)
    {
        var mask = (byte)0;

        if (compressionAlgorithm != globalSettings.compressionAlgorithm)
            mask |= 1;

        if (encoding != globalSettings.encoding)
            mask |= 1 << 1;

        if (lossyQuality != globalSettings.lossyQuality)
            mask |= 1 << 2;

        if (readableAtRuntime != globalSettings.readableAtRuntime)
            mask |= 1 << 3;

        writer.Write(mask);

        if ((mask & 1) != 0)
            writer.WriteEnum(compressionAlgorithm);

        if ((mask & (1 << 1)) != 0)
            writer.WriteEnum(encoding);

        if ((mask & (1 << 2)) != 0)
            writer.Write(lossyQuality);

        if ((mask & (1 << 3)) != 0)
            writer.Write(readableAtRuntime);
    }
#else
    /// <inheritdoc/>
    public void ReadFrom(XPABReader reader)
    {
        compressionAlgorithm = reader.ReadEnum<Texture2DCompressionAlgorithm>();
        encoding = reader.ReadEnum<Texture2DEncoding>();
        lossyQuality = reader.ReadInt32();
        readableAtRuntime = reader.ReadBoolean();
    }

    /// <inheritdoc/>
    public void ReadDeltaFrom(XPABReader reader, ImageSettings globalSettings)
    {
        var mask = reader.ReadByte();

        compressionAlgorithm = (mask & 1) != 0 ? reader.ReadEnum<Texture2DCompressionAlgorithm>() : globalSettings.compressionAlgorithm;
        encoding = (mask & (1 << 1)) != 0 ? reader.ReadEnum<Texture2DEncoding>() : globalSettings.encoding;
        lossyQuality = (mask & (1 << 2)) != 0 ? reader.ReadInt32() : globalSettings.lossyQuality;
        readableAtRuntime = (mask & (1 << 3)) != 0 ? reader.ReadBoolean() : globalSettings.readableAtRuntime;
    }
#endif
}

/// <summary>Specifies the image format used to encode texture data.</summary>
#if UNITY_EDITOR
[Tooltip("Specifies the image format used to encode texture data.")]
#endif
public enum Texture2DEncoding : byte
{
    /// <summary>Raw, uncompressed 32-bit RGBA pixel data.</summary>
    Raw = 0,

    /// <summary>Platform-optimised block compression.</summary>
    Native = 1,

    /// <summary>Lossless PNG encoding.</summary>
    PNG = 2,

    /// <summary>Lossy JPG encoding.</summary>
    JPG = 3,
}

/// <summary>Specifies the payload compression algorithm applied to the encoded texture bytes.</summary>
#if UNITY_EDITOR
[Tooltip("Specifies the payload compression algorithm applied to the encoded texture bytes.")]
#endif
public enum Texture2DCompressionAlgorithm : byte
{
    /// <summary>No additional payload compression. Fast disk reads but larger file sizes.</summary>
    None = 0,

    /// <summary>High-ratio Brotli compression for maximum disk space savings. Slower runtime decompression.</summary>
    Brotli = 1,

    /// <summary>Chunk-based LZ4 compression for a balance of file size and fast streaming speeds.</summary>
    LZ4 = 2,
}
