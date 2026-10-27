#if UNITY_EDITOR
using XPAB.Writer;
#else
using XPAB.Reader;
#endif

namespace XPAB.Settings;

/// <summary>A configuration holder that dictates how <see cref="GameObject"/>s and <see cref="ScriptableObject"/>s are serialised.</summary>
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity convention.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
[Serializable]
#if UNITY_EDITOR
[Tooltip("A configuration holder that dictates how GameObjects and ScriptableObjects are serialised.")]
#endif
public sealed class DataSettings : IAssetSettings<DataSettings>
{
    /// <summary>How serialised fields and hierarchies are formatted.</summary>
#if UNITY_EDITOR
    [Tooltip("How serialised fields and hierarchies are formatted.")]
#endif
    public DataSerialisationFormat format = DataSerialisationFormat.Binary;

    /// <summary>How to handle missing MonoBehaviour or ScriptableObject script definitions.</summary>
#if UNITY_EDITOR
    [Tooltip("How to handle missing MonoBehaviour or ScriptableObject script definitions.")]
#endif
    public MissingScriptHandling missingScripts = MissingScriptHandling.WarnAndSkip;

#if UNITY_EDITOR
    /// <inheritdoc/>
    public void WriteTo(XPABWriter writer)
    {
        writer.WriteEnum(format);
        writer.WriteEnum(missingScripts);
    }

    /// <inheritdoc/>
    public void WriteDeltaTo(XPABWriter writer, DataSettings globalSettings)
    {
        var mask = (byte)0;

        if (format != globalSettings.format)
            mask |= 1;

        if (missingScripts != globalSettings.missingScripts)
            mask |= 1 << 1;

        writer.Write(mask);

        if ((mask & 1) != 0)
            writer.WriteEnum(format);

        if ((mask & (1 << 1)) != 0)
            writer.WriteEnum(missingScripts);
    }
#else
    /// <inheritdoc/>
    public void ReadFrom(XPABReader reader)
    {
        format = reader.ReadEnum<DataSerialisationFormat>();
        missingScripts = reader.ReadEnum<MissingScriptHandling>();
    }

    /// <inheritdoc/>
    public void ReadDeltaFrom(XPABReader reader, DataSettings globalSettings)
    {
        var mask = reader.ReadByte();

        format = (mask & 1) != 0 ? reader.ReadEnum<DataSerialisationFormat>() : globalSettings.format;
        missingScripts = (mask & (1 << 1)) != 0 ? reader.ReadEnum<MissingScriptHandling>() : globalSettings.missingScripts;
    }
#endif
}

/// <summary>Specifies the format used to serialise component data.</summary>
#if UNITY_EDITOR
[Tooltip("Specifies the format used to serialise component data.")]
#endif
public enum DataSerialisationFormat : byte
{
    /// <summary>Compact binary representation.</summary>
    Binary = 0,

    /// <summary>Human-readable JSON.</summary>
    Json = 1,
}

/// <summary>Specifies the behavior when a required script type cannot be found.</summary>
#if UNITY_EDITOR
[Tooltip("Specifies the behavior when a required script type cannot be found.")]
#endif
public enum MissingScriptHandling : byte
{
    /// <summary>Logs a warning in the console but continues serialisation, omitting the missing script.</summary>
    WarnAndSkip = 0,

    /// <summary>Throws a hard exception and immediately halts the serialisation process.</summary>
    ThrowError = 1,

    /// <summary>Silently removes the missing script component without logging any warnings.</summary>
    StripSilently = 2,
}
