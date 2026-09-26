namespace XPAB.Assets;

/// <summary>An asset type to hold binary data. Alternative to the conventional usage of <see cref="TextAsset"/> to hold binary data.</summary>
/// <remarks>It is highly recommended that you use this to hold your binary data, as a <see cref="TextAsset"/> cannot be simply be rebuilt from arbitrary bytes (that's a Unity thing).</remarks>
public sealed class BinaryAsset : ScriptableObject
{
    /// <summary>The byte array of the binary file.</summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity field.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
    public byte[] bytes;
}
