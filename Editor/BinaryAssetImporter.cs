using UnityEditor.AssetImporters;
using XPAB.Assets;

namespace XPAB;

[ScriptedImporter(1, ["bin", "dat", "bytes", "raw"])]
internal class BinaryAssetImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var binaryAsset = ScriptableObject.CreateInstance<BinaryAsset>();
        binaryAsset.file = ctx.assetPath;

        ctx.AddObjectToAsset("main", binaryAsset);
        ctx.SetMainObject(binaryAsset);
    }
}
