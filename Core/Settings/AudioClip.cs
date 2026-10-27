#if UNITY_EDITOR
using XPAB.Writer;
#else
using XPAB.Reader;
#endif

namespace XPAB.Settings;

/// <summary>A configuration holder that dictates how <see cref="AudioClip"/>s are serialised.</summary>
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity convention.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
[Serializable]
#if UNITY_EDITOR
[Tooltip("A configuration holder that dictates how AudioClips are serialised.")]
#endif
public sealed class AudioClipSettings : IAssetSettings<AudioClipSettings>
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

    /// <summary>Lossy compression quality from 0.0 (smallest) to 1.0 (highest fidelity).</summary>
#if UNITY_EDITOR
    [Tooltip("Lossy compression quality from 0.0 (smallest) to 1.0 (highest fidelity).")]
    [Range(0f, 1f)]
#endif
    public float quality = 0.9f;

    /// <summary>Converts multi-channel audio to single-channel mono.</summary>
#if UNITY_EDITOR
    [Tooltip("Converts multi-channel audio to single-channel mono.")]
#endif
    public bool forceToMono = false;

    /// <summary>Optional sample-rate downsampling for audio clips.</summary>
#if UNITY_EDITOR
    [Tooltip("Optional sample-rate downsampling for audio clips.")]
#endif
    public AudioSampleRateOverride sampleRateOverride = AudioSampleRateOverride.Preserve;

#if UNITY_EDITOR
    /// <inheritdoc/>
    public void WriteTo(XPABWriter writer)
    {
        writer.WriteEnum(bitDepth);
        writer.WriteEnum(compressionMethod);
        writer.Write(quality);
        writer.Write(forceToMono);
        writer.WriteEnum(sampleRateOverride);
    }

    /// <inheritdoc/>
    public void WriteDeltaTo(XPABWriter writer, AudioClipSettings globalSettings)
    {
        var mask = (byte)0;

        if (bitDepth != globalSettings.bitDepth)
            mask |= 1;

        if (compressionMethod != globalSettings.compressionMethod)
            mask |= 1 << 1;

        if (quality != globalSettings.quality)
            mask |= 1 << 2;

        if (forceToMono != globalSettings.forceToMono)
            mask |= 1 << 3;

        if (sampleRateOverride != globalSettings.sampleRateOverride)
            mask |= 1 << 4;

        writer.Write(mask);

        if ((mask & 1) != 0)
            writer.WriteEnum(bitDepth);

        if ((mask & (1 << 1)) != 0)
            writer.WriteEnum(compressionMethod);

        if ((mask & (1 << 2)) != 0)
            writer.Write(quality);

        if ((mask & (1 << 3)) != 0)
            writer.Write(forceToMono);

        if ((mask & (1 << 4)) != 0)
            writer.WriteEnum(sampleRateOverride);
    }
#else
    /// <inheritdoc/>
    public void ReadFrom(XPABReader reader)
    {
        bitDepth = reader.ReadEnum<AudioBitDepth>();
        compressionMethod = reader.ReadEnum<AudioCompressionMethod>();
        quality = reader.ReadSingle();
        forceToMono = reader.ReadBoolean();
        sampleRateOverride = reader.ReadEnum<AudioSampleRateOverride>();
    }

    /// <inheritdoc/>
    public void ReadDeltaFrom(XPABReader reader, AudioClipSettings globalSettings)
    {
        var mask = reader.ReadByte();

        bitDepth = (mask & 1) != 0 ? reader.ReadEnum<AudioBitDepth>() : globalSettings.bitDepth;
        compressionMethod = (mask & (1 << 1)) != 0 ? reader.ReadEnum<AudioCompressionMethod>() : globalSettings.compressionMethod;
        quality = (mask & (1 << 2)) != 0 ? reader.ReadSingle() : globalSettings.quality;
        forceToMono = (mask & (1 << 3)) != 0 ? reader.ReadBoolean() : globalSettings.forceToMono;
        sampleRateOverride = (mask & (1 << 4)) != 0 ? reader.ReadEnum<AudioSampleRateOverride>() : globalSettings.sampleRateOverride;
    }
#endif
}

/// <summary>Specifies how audio sample rates are modified during serialisation.</summary>
#if UNITY_EDITOR
[Tooltip("Specifies how audio sample rates are modified during serialisation.")]
#endif
public enum AudioSampleRateOverride : byte
{
    /// <summary>Maintains the original sample rate of the audio clip.</summary>
    Preserve = 0,

    /// <summary>Forces the sample rate to 44,100 Hz (Standard CD quality).</summary>
    DownsampleTo44100 = 1,

    /// <summary>Forces the sample rate to 22,050 Hz (Half rate, optimal for voice lines).</summary>
    DownsampleTo22050 = 2,

    /// <summary>Forces the sample rate to 11,025 Hz (Quarter rate, optimal for lo-fi effects).</summary>
    DownsampleTo11025 = 3,
}

/// <summary>Specifies the baseline bit-depth format for serialised audio samples.</summary>
#if UNITY_EDITOR
[Tooltip("Specifies the baseline bit-depth format for serialised audio samples.")]
#endif
public enum AudioBitDepth : byte
{
    /// <summary>32-bit floating-point audio data.</summary>
    Float32,

    /// <summary>24-bit squished audio data.</summary>
    Float24,

    /// <summary>16-bit PCM audio data.</summary>
    Int16,

    /// <summary>8-bit quantized audio data.</summary>
    Int8,
}

/// <summary>Specifies the compression algorithm used to pack audio data.</summary>
#if UNITY_EDITOR
[Tooltip("Specifies the compression algorithm used to pack audio data.")]
#endif
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
