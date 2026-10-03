namespace XPAB;

internal static class TextureExtractor
{
    public static byte[] ExtractRawTextureData(Texture2D asset)
    {
        if (asset.isReadable)
            return asset.GetRawTextureData();

        var path = AssetDatabase.GetAssetPath(asset);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter ?? throw new InvalidOperationException($"Could not extract raw data for {asset.name}. Ensure it is a valid texture asset.");

        importer.isReadable = true;
        importer.SaveAndReimport();

        var rawData = asset.GetRawTextureData();

        importer.isReadable = false;
        importer.SaveAndReimport();

        return rawData;
    }

    public static byte[] ExtractViaBlit(Texture2D asset, Texture2DCompressionMethod method)
    {
        var tmp = RenderTexture.GetTemporary(asset.width, asset.height, 0, RenderTextureFormat.Default, asset.isDataSRGB ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear);
        var previous = RenderTexture.active;
        Texture2D? readableTex = null;

        try
        {
            Graphics.Blit(asset, tmp);
            RenderTexture.active = tmp;

            readableTex = new Texture2D(asset.width, asset.height, TextureFormat.RGBA32, false, !asset.isDataSRGB);
            readableTex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            readableTex.Apply();

            return method == Texture2DCompressionMethod.PNG
                ? readableTex.EncodeToPNG()
                : readableTex.EncodeToJPG();
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);

            if (readableTex)
                UObject.DestroyImmediate(readableTex);
        }
    }
}
