namespace XPAB;

/// <summary>A configuration holder that dictates how <see cref="AudioClip"/>s are serialised.</summary>
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity convention.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
[Serializable]
public sealed class AudioSerialisationSettings
{
    /// <summary>Reduces the baseline bit-depth of the audio samples.</summary>
#if UNITY_EDITOR
    [Tooltip("Reduces the baseline bit-depth of the audio samples.")]
#endif
    public AudioBitDepth bitDepth = AudioBitDepth.Int16;

    /// <summary>The compression algorithm applied to the extracted audio stream.</summary>
#if UNITY_EDITOR
    [Tooltip("The compression algorithm applied to the extracted audio stream.")]
#endif
    public AudioCompressionMethod compressionMethod = AudioCompressionMethod.Vorbis;
}

/// <summary>Specifies the baseline bit-depth format for serialised audio samples.</summary>
public enum AudioBitDepth : byte
{
    /// <summary>Raw 32-bit floating-point audio data.</summary>
    Float32,

    /// <summary>Standard 16-bit PCM audio data.</summary>
    Int16,

    /// <summary>Low-fidelity 8-bit quantized audio data.</summary>
    Int8,
}

/// <summary>Specifies the compression algorithm used to pack audio data.</summary>
public enum AudioCompressionMethod : byte
{
    /// <summary>No compression is applied.</summary>
    None,

    /// <summary>Lossless generic data compression applied over (potentially) quantized audio bytes.</summary>
    Brotli,

    /// <summary>Adaptive Differential PCM compression.</summary>
    ADPCM,

    /// <summary>Lossy Ogg Vorbis compression for maximum file size reduction.</summary>
    Vorbis,
}

/// <summary>A configuration holder that dictates how <see cref="Texture2D"/>s are serialised.</summary>
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity convention.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
[Serializable]
public sealed class Texture2DSerialisationSettings
{
    /// <summary>How the texture bytes are compressed inside the XPAB file.</summary>
#if UNITY_EDITOR
    [Tooltip("How the texture bytes are compressed inside the XPAB file.")]
#endif
    public Texture2DCompressionMethod compressionMethod = Texture2DCompressionMethod.Brotli;
}

/// <summary>Specifies the algorithm and format used to compress texture data.</summary>
public enum Texture2DCompressionMethod : byte
{
    /// <summary>Writes the raw GPU bytes directly with no additional compression.</summary>
    None,

    /// <summary>Compresses the raw GPU bytes using the Brotli algorithm, preserving block compression for fast runtime loading.</summary>
    Brotli,

    /// <summary>Encodes the texture to a PNG format.</summary>
    PNG,

    /// <summary>Encodes the texture to a lossy JPG format.</summary>
    JPG,
}
