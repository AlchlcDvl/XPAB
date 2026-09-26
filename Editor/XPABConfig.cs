namespace XPAB;

/// <summary>A configuration holder that dictates how an XPAB file is serialised.</summary>
[Serializable]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity convention.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
public sealed class XPABConfig
{
    /// <summary>The name of the file. Also used for part of the asset label.</summary>
    public string name;

    /// <summary>The output path to where the XPAB file is built.</summary>
    public string[] path = ["Assets", "StreamingAssets"];

    /// <summary>An array of platforms that the XPAB file is built for (for assets that cannot be made platform agnostic).</summary>
    public TargetPlatform[] targets;
}
