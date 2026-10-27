#if UNITY_EDITOR
using XPAB.Writer;
#else
using XPAB.Reader;
#endif

namespace XPAB.Settings;

/// <summary>A configuration holder that dictates how <see cref="Material"/>s are serialised.</summary>
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity convention.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
[Serializable]
#if UNITY_EDITOR
[Tooltip("A configuration holder that dictates how Materials are serialised.")]
#endif
public sealed class MaterialSettings : IAssetSettings<MaterialSettings>
{
    /// <summary>Determines how missing shaders are resolved at runtime.</summary>
#if UNITY_EDITOR
    [Tooltip("Determines how missing shaders are resolved at runtime.")]
#endif
    public ShaderResolutionStrategy shaderResolution = ShaderResolutionStrategy.FindInGame;

    /// <summary>Fallback shader name to use if the original cannot be found in the host game.</summary>
#if UNITY_EDITOR
    [Tooltip("Fallback shader name to use if the original cannot be found in the host game.")]
#endif
    public string fallbackShaderName = "Standard";

#if UNITY_EDITOR
    /// <inheritdoc/>
    public void WriteTo(XPABWriter writer)
    {
        writer.WriteEnum(shaderResolution);
        writer.Write(fallbackShaderName);
    }

    /// <inheritdoc/>
    public void WriteDeltaTo(XPABWriter writer, MaterialSettings globalSettings)
    {
        var mask = (byte)0;

        if (shaderResolution != globalSettings.shaderResolution)
            mask |= 1;

        if (fallbackShaderName != globalSettings.fallbackShaderName)
            mask |= 1 << 1;

        writer.Write(mask);

        if ((mask & 1) != 0)
            writer.WriteEnum(shaderResolution);

        if ((mask & (1 << 1)) != 0)
            writer.Write(fallbackShaderName);
    }
#else
    /// <inheritdoc/>
    public void ReadFrom(XPABReader reader)
    {
        shaderResolution = reader.ReadEnum<ShaderResolutionStrategy>();
        fallbackShaderName = reader.ReadString();
    }

    /// <inheritdoc/>
    public void ReadDeltaFrom(XPABReader reader, MaterialSettings globalSettings)
    {
        var mask = reader.ReadByte();

        shaderResolution = (mask & 1) != 0 ? reader.ReadEnum<ShaderResolutionStrategy>() : globalSettings.shaderResolution;
        fallbackShaderName = (mask & (1 << 1)) != 0 ? reader.ReadString() : globalSettings.fallbackShaderName;
    }
#endif
}

/// <summary>Specifies how shaders are assigned to materials at runtime.</summary>
#if UNITY_EDITOR
[Tooltip("Specifies how shaders are assigned to materials at runtime.")]
#endif
public enum ShaderResolutionStrategy : byte
{
    /// <summary>Calls Shader.Find(name) against the game's loaded shaders.</summary>
    FindInGame = 0,

    /// <summary>Loads an embedded platform-specific bundle containing the compiled shader.</summary>
    LoadFromPlatformPayload = 1,

    /// <summary>Always applies the specified fallback shader.</summary>
    ForceFallback = 2,
}
