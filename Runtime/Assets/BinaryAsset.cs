namespace XPAB.Assets;

/// <summary>An asset type to hold binary data. Alternative to the conventional usage of <see cref="TextAsset"/> to hold binary data.</summary>
public sealed class BinaryAsset : ScriptableObject
{
    /// <summary>The byte array help by the bytes.</summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity field.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
    public byte[] bytes;
}
