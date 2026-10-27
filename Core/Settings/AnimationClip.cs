#if UNITY_EDITOR
using XPAB.Writer;
#else
using XPAB.Reader;
#endif

namespace XPAB.Settings;

/// <summary>A configuration holder that dictates how <see cref="AnimationClip"/>s are serialised.</summary>
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity convention.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
[Serializable]
#if UNITY_EDITOR
[Tooltip("A configuration holder that dictates how AnimationClips are serialised.")]
#endif
public sealed class AnimationClipSettings : IAssetSettings<AnimationClipSettings>
{
    /// <summary>Keyframe reduction algorithm to cull redundant animation keys.</summary>
#if UNITY_EDITOR
    [Tooltip("Keyframe reduction algorithm to cull redundant animation keys.")]
#endif
    public AnimationCompressionMethod compression = AnimationCompressionMethod.Optimal;

    /// <summary>Maximum allowed positional error margin when reducing curve keyframes.</summary>
#if UNITY_EDITOR
    [Tooltip("Maximum allowed positional error margin when reducing curve keyframes.")]
#endif
    public float positionErrorTolerance = 0.001f;

    /// <summary>Maximum allowed rotational error margin in degrees.</summary>
#if UNITY_EDITOR
    [Tooltip("Maximum allowed rotational error margin in degrees.")]
#endif
    public float rotationErrorTolerance = 0.05f;

#if UNITY_EDITOR
    /// <inheritdoc/>
    public void WriteTo(XPABWriter writer)
    {
        writer.WriteEnum(compression);
        writer.Write(positionErrorTolerance);
        writer.Write(rotationErrorTolerance);
    }

    /// <inheritdoc/>
    public void WriteDeltaTo(XPABWriter writer, AnimationClipSettings globalSettings)
    {
        var mask = (byte)0;

        if (compression != globalSettings.compression)
            mask |= 1;

        if (positionErrorTolerance != globalSettings.positionErrorTolerance)
            mask |= 1 << 1;

        if (rotationErrorTolerance != globalSettings.rotationErrorTolerance)
            mask |= 1 << 2;

        writer.Write(mask);

        if ((mask & 1) != 0)
            writer.WriteEnum(compression);

        if ((mask & (1 << 1)) != 0)
            writer.Write(positionErrorTolerance);

        if ((mask & (1 << 2)) != 0)
            writer.Write(rotationErrorTolerance);
    }
#else
    /// <inheritdoc/>
    public void ReadFrom(XPABReader reader)
    {
        compression = reader.ReadEnum<AnimationCompressionMethod>();
        positionErrorTolerance = reader.ReadSingle();
        rotationErrorTolerance = reader.ReadSingle();
    }

    /// <inheritdoc/>
    public void ReadDeltaFrom(XPABReader reader, AnimationClipSettings globalSettings)
    {
        var mask = reader.ReadByte();

        compression = (mask & 1) != 0 ? reader.ReadEnum<AnimationCompressionMethod>() : globalSettings.compression;
        positionErrorTolerance = (mask & (1 << 1)) != 0 ? reader.ReadSingle() : globalSettings.positionErrorTolerance;
        rotationErrorTolerance = (mask & (1 << 2)) != 0 ? reader.ReadSingle() : globalSettings.rotationErrorTolerance;
    }
#endif
}

/// <summary>Specifies how animation curves are compressed during serialisation.</summary>
#if UNITY_EDITOR
[Tooltip("Specifies how animation curves are compressed during serialisation.")]
#endif
public enum AnimationCompressionMethod : byte
{
    /// <summary>No keyframe reduction is applied. The raw curve data is serialised.</summary>
    Off = 0,

    /// <summary>Applies basic keyframe reduction, removing mathematically redundant keys.</summary>
    KeyframeReduction = 1,

    /// <summary>Applies aggressive, optimal keyframe reduction based on strict error tolerances.</summary>
    Optimal = 2,
}
