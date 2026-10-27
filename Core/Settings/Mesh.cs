#if UNITY_EDITOR
using XPAB.Writer;
#else
using XPAB.Reader;
#endif

namespace XPAB.Settings;

/// <summary>A configuration holder that dictates how <see cref="Mesh"/>es are serialised.</summary>
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity convention.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
[Serializable]
#if UNITY_EDITOR
[Tooltip("A configuration holder that dictates how meshes are serialised.")]
#endif
public sealed class MeshSettings : IAssetSettings<MeshSettings>
{
    /// <summary>Precision reduction applied to vertex positions and attributes.</summary>
#if UNITY_EDITOR
    [Tooltip("Precision reduction applied to vertex positions and attributes.")]
#endif
    public MeshQuantization quantization = MeshQuantization.Medium;

    /// <summary>Determines whether 16-bit indices are used when the vertex count is below 65,535.</summary>
#if UNITY_EDITOR
    [Tooltip("Determines whether 16-bit indices are used when the vertex count is below 65,535.")]
#endif
    public bool compactIndexBuffers = true;

    /// <summary>Vertex attributes to strip entirely to save disk space.</summary>
#if UNITY_EDITOR
    [Tooltip("Vertex attributes to strip entirely to save disk space.")]
#endif
    public MeshAttributeStripping strippedAttributes = MeshAttributeStripping.None;

    /// <summary>Whether Unity should keep a CPU-side copy of the mesh buffer at runtime.</summary>
#if UNITY_EDITOR
    [Tooltip("Whether Unity should keep a CPU-side copy of the mesh buffer at runtime.")]
#endif
    public bool readableAtRuntime = false;

    /// <summary>Applies vertex cache optimisation to index buffers before writing.</summary>
#if UNITY_EDITOR
    [Tooltip("Applies vertex cache optimisation to index buffers before writing.")]
#endif
    public bool optimiseVertexOrder = true;

#if UNITY_EDITOR
    /// <inheritdoc/>
    public void WriteTo(XPABWriter writer)
    {
        writer.WriteEnum(quantization);
        writer.Write(compactIndexBuffers);
        writer.WriteEnum(strippedAttributes);
        writer.Write(readableAtRuntime);
        writer.Write(optimiseVertexOrder);
    }

    /// <inheritdoc/>
    public void WriteDeltaTo(XPABWriter writer, MeshSettings globalSettings)
    {
        var mask = (byte)0;

        if (quantization != globalSettings.quantization)
            mask |= 1;

        if (compactIndexBuffers != globalSettings.compactIndexBuffers)
            mask |= 1 << 1;

        if (strippedAttributes != globalSettings.strippedAttributes)
            mask |= 1 << 2;

        if (readableAtRuntime != globalSettings.readableAtRuntime)
            mask |= 1 << 3;

        if (optimiseVertexOrder != globalSettings.optimiseVertexOrder)
            mask |= 1 << 4;

        writer.Write(mask);

        if ((mask & 1) != 0)
            writer.WriteEnum(quantization);

        if ((mask & (1 << 1)) != 0)
            writer.Write(compactIndexBuffers);

        if ((mask & (1 << 2)) != 0)
            writer.WriteEnum(strippedAttributes);

        if ((mask & (1 << 3)) != 0)
            writer.Write(readableAtRuntime);

        if ((mask & (1 << 4)) != 0)
            writer.Write(optimiseVertexOrder);
    }
#else
    /// <inheritdoc/>
    public void ReadFrom(XPABReader reader)
    {
        quantization = reader.ReadEnum<MeshQuantization>();
        compactIndexBuffers = reader.ReadBoolean();
        strippedAttributes = reader.ReadEnum<MeshAttributeStripping>();
        readableAtRuntime = reader.ReadBoolean();
        optimiseVertexOrder = reader.ReadBoolean();
    }

    /// <inheritdoc/>
    public void ReadDeltaFrom(XPABReader reader, MeshSettings globalSettings)
    {
        var mask = reader.ReadByte();

        quantization = (mask & 1) != 0 ? reader.ReadEnum<MeshQuantization>() : globalSettings.quantization;
        compactIndexBuffers = (mask & (1 << 1)) != 0 ? reader.ReadBoolean() : globalSettings.compactIndexBuffers;
        strippedAttributes = (mask & (1 << 2)) != 0 ? reader.ReadEnum<MeshAttributeStripping>() : globalSettings.strippedAttributes;
        readableAtRuntime = (mask & (1 << 3)) != 0 ? reader.ReadBoolean() : globalSettings.readableAtRuntime;
        optimiseVertexOrder = (mask & (1 << 4)) != 0 ? reader.ReadBoolean() : globalSettings.optimiseVertexOrder;
    }
#endif
}

/// <summary>Specifies the precision format for serialised mesh vertices.</summary>
#if UNITY_EDITOR
[Tooltip("Specifies the precision format for serialised mesh vertices.")]
#endif
public enum MeshQuantization : byte
{
    /// <summary>Full 32-bit floats for all attributes.</summary>
    Lossless = 0,

    /// <summary>16-bit half floats for UVs, Normals, and Tangents; 32-bit for Positions.</summary>
    Medium = 1,

    /// <summary>16-bit half floats across all vertex channels.</summary>
    Aggressive = 2,
}

/// <summary>Specifies which mesh attributes are removed during serialisation.</summary>
#if UNITY_EDITOR
[Tooltip("Specifies which mesh attributes are removed during serialisation.")]
#endif
[Flags]
public enum MeshAttributeStripping : byte
{
    /// <summary>No attributes are stripped. All available mesh data is preserved.</summary>
    None = 0,

    /// <summary>Strips vertex color channels (RGBA) from the mesh.</summary>
    VertexColors = 1 << 0,

    /// <summary>Strips vertex normals from the mesh.</summary>
    Normals = 1 << 1,

    /// <summary>Strips tangent space data from the mesh.</summary>
    Tangents = 1 << 2,

    /// <summary>Strips all secondary UV channels (UV2, UV3, UV4, etc.) from the mesh.</summary>
    SecondaryUVs = 1 << 3,

    /// <summary>Strips bone weights for static meshes.</summary>
    BoneWeights = 1 << 4,

    /// <summary>Strips bind poses for meshes that are not animated.</summary>
    Bindposes = 1 << 5,

    /// <summary>Strips expressions and other such animations from the mesh.</summary>
    BlendShapes = 1 << 6,
}
