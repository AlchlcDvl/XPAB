using System.IO.Compression;
using XPAB.Assets;

namespace XPAB.Writer;

internal partial class AssetWriter
{
    public void WriteTextAsset(TextAsset asset) => WriteTextAssetV1(asset);

    public void WriteBinaryAsset(BinaryAsset asset) => WriteBinaryAssetV1(asset);

    public void WriteAudioClip(AudioClip asset) => WriteAudioClipV1(asset);

    public void WriteTexture2D(Texture2D asset) => WriteTexture2DV1(asset);

    private void WriteTextAssetV1(TextAsset asset)
    {
        WritePacked(0u); // No metadata
        WriteBytesAndSize(asset.bytes);
    }

    private void WriteBinaryAssetV1(BinaryAsset asset)
    {
        WritePacked(0u); // No metadata
        WriteFile(asset.file);
    }

    private void WriteAudioClipV1(AudioClip asset)
    {
        using var metadataStream = new MemoryStream();
        using var metaWriter = new AssetWriter(metadataStream);

        metaWriter.WritePacked(asset.samples);
        metaWriter.WritePacked(asset.channels);
        metaWriter.WritePacked(asset.frequency);

        WriteStreamAndLength(metadataStream);

        asset.LoadAudioData();

        var audioData = new float[asset.samples * asset.channels];
        asset.GetData(audioData, 0);

        using var compressedDataStream = new MemoryStream();

        using (var deflateStream = new BrotliStream(compressedDataStream, CompressionLevel.Optimal, true))
        using (var fileWriter = new AssetWriter(deflateStream, true))
        {
            for (var i = 0; i < audioData.Length; i++)
                fileWriter.Write((short)(Math.Clamp(audioData[i], -1f, 1f) * 32767f));
        }

        WriteStreamAndLength(compressedDataStream);
    }

    private void WriteTexture2DV1(Texture2D asset)
    {
        using var metadataStream = new MemoryStream();
        using var metaWriter = new AssetWriter(metadataStream);

        metaWriter.WritePacked((int)asset.filterMode);
        metaWriter.WritePacked((int)asset.wrapMode);
        metaWriter.WritePacked(asset.anisoLevel);

        WriteStreamAndLength(metadataStream);

        var assetPath = AssetDatabase.GetAssetPath(asset);
        var ext = Path.GetExtension(assetPath).ToLowerInvariant();

        if (ext is ".png" or ".jpg" or ".jpeg")
        {
            WriteFile(assetPath);
        }
        else
        {
            var tmp = RenderTexture.GetTemporary(asset.width, asset.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
            Graphics.Blit(asset, tmp);

            var previous = RenderTexture.active;
            RenderTexture.active = tmp;

            var readableTex = new Texture2D(asset.width, asset.height);
            readableTex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            readableTex.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);

            var pngBytes = readableTex.EncodeToPNG();
            UObject.DestroyImmediate(readableTex);

            using var pngStream = new MemoryStream(pngBytes);
            WriteStreamAndLength(pngStream);
        }
    }
}
