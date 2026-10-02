using System.IO.Compression;
using XPAB.Assets;

namespace XPAB.Writer;

internal partial class XPABWriter
{
    public void WriteTextAsset(TextAsset asset) => WriteBytesAndSize(asset.bytes);

    public void WriteBinaryAsset(BinaryAsset asset) => WriteFile(asset.file);

    public void WriteAudioClip(AudioClip asset)
    {
        WritePacked(asset.samples);
        WritePacked(asset.channels);
        WritePacked(asset.frequency);

        if (!asset.LoadAudioData())
            throw new InvalidOperationException("Received a malformed asset.");

        var audioData = new float[asset.samples * asset.channels];
        asset.GetData(audioData, 0);

        using var compressedDataStream = new MemoryStream();
        using (var deflateStream = new BrotliStream(compressedDataStream, CompressionLevel.Optimal, true))
        using (var fileWriter = new XPABWriter(deflateStream, true))
        {
            for (var i = 0; i < audioData.Length; i++)
                fileWriter.Write((short)(Math.Clamp(audioData[i], -1f, 1f) * 32767f));
        }

        WriteStreamAndLength(compressedDataStream);
    }

    public void WriteTexture2D(Texture2D asset)
    {
        WritePackedEnum(asset.filterMode);
        WritePackedEnum(asset.wrapModeU);
        WritePackedEnum(asset.wrapModeV);
        WritePackedEnum(asset.wrapModeW);
        WritePackedEnum(asset.wrapMode);
        WritePacked(asset.anisoLevel);
        Write(asset.mipmapCount > 1);
        Write(asset.isDataSRGB);
        Write(asset.isReadable);

        var assetPath = AssetDatabase.GetAssetPath(asset);
        var ext = Path.GetExtension(assetPath).ToLowerInvariant();

        if (ext is ".png" or ".jpg" or ".jpeg")
        {
            WriteBytesAndSize(File.ReadAllBytes(assetPath));
            return;
        }

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

            WriteBytesAndSize(readableTex.EncodeToPNG());
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
