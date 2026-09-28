namespace XPAB.Assets;

/// <summary>An asset type to hold binary data. Alternative to the conventional usage of <see cref="TextAsset"/> to hold binary data.</summary>
/// <remarks>It is highly recommended that you use this to hold your binary data, as a <see cref="TextAsset"/> cannot be simply be rebuilt from arbitrary bytes (that's a Unity thing).</remarks>
#if UNITY_EDITOR
public sealed class BinaryAsset : ScriptableObject
#else
public sealed class BinaryAsset : ScriptableObject, IDisposable
#endif
{
    /// <summary>The path of the binary file.</summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity field.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
    [NonSerialized]
    public string file;

#if !UNITY_EDITOR
    /// <summary>Gets the <see cref="System.IO.FileStream"/> of the file this asset represents.</summary>
    public FileStream FileStream { get; internal set; }

    /// <inheritdoc/>
    public void Dispose() => FileStream.Close();
#endif
}
