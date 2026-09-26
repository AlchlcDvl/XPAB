using UnityEditor.AssetImporters;
using XPAB.Assets;

/// <summary>A scripted asset importer for <see cref="BinaryAsset"/>.</summary>
[ScriptedImporter(1, ["bin", "dat", "bytes", "raw"])]
[SuppressMessage("Design", "CA1050:Declare types in namespaces", Justification = "Unity convention.")]
[SuppressMessage("Major Bug", "S3903:Types should be defined in named namespaces", Justification = "Read above.")]
public class BinaryAssetImporter : ScriptedImporter
{
    /// <inheritdoc/>
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var binaryAsset = ScriptableObject.CreateInstance<BinaryAsset>();
        binaryAsset.bytes = File.ReadAllBytes(ctx.assetPath);

        ctx.AddObjectToAsset("main", binaryAsset);
        ctx.SetMainObject(binaryAsset);
    }
}
