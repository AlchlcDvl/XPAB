using System.IO.Compression;
using System.Text;
using XPAB.Assets;

namespace XPAB.Reader;

internal partial class AssetReader
{
    private static readonly Func<byte[], string> DecodeString = (Func<byte[], string>)typeof(TextAsset).GetMethod("DecodeString").CreateDelegate(typeof(Func<byte[], string>));

    public TextAsset ReadTextAssetV1(string name)
    {
        _ = ReadPackedInt32(); // No metadata
        var bytes = ReadBytesAndSize();
        return new TextAsset(DecodeString(bytes)) { name = name };
    }

    public BinaryAsset ReadBinaryAssetV1(string name, string ext)
    {
        _ = ReadPackedInt32(); // No metadata

        var path = Path.Combine(XPABCore.CachePath, $"{name}.{ext}");

        ReadToFile(path);

        var asset = ScriptableObject.CreateInstance<BinaryAsset>();
        asset.file = path;
        asset.name = name;
        asset.FileStream = File.OpenRead(path);

        return asset;
    }

    public AudioClip ReadAudioClipV1(string name)
    {
        using var metadataStream = new MemoryStream();
        ReadStreamAndLength(metadataStream);
        metadataStream.Position = 0;

        using var metaReader = new AssetReader(metadataStream);
        var samples = metaReader.ReadPackedInt32();
        var channels = metaReader.ReadPackedInt32();
        var frequency = metaReader.ReadPackedInt32();

        using var compressedStream = new MemoryStream();
        ReadStreamAndLength(compressedStream);
        compressedStream.Position = 0;

        var totalSamples = samples * channels;
        var audioData = new float[totalSamples];

        using (var brotliStream = new BrotliStream(compressedStream, CompressionMode.Decompress, true))
        using (var fileReader = new AssetReader(brotliStream, Encoding.UTF8, true))
        {
            for (var i = 0; i < totalSamples; i++)
                audioData[i] = fileReader.ReadInt16() / 32767f;
        }

        var clip = AudioClip.Create(name, samples, channels, frequency, false);
        clip.SetData(audioData, 0);

        return clip;
    }

    public Texture2D ReadTexture2DV1(string name)
    {
        using var metadataStream = new MemoryStream();
        ReadStreamAndLength(metadataStream);
        metadataStream.Position = 0;

        using var metaReader = new AssetReader(metadataStream);
        var filterMode = (FilterMode)metaReader.ReadPackedInt32();
        var wrapMode = (TextureWrapMode)metaReader.ReadPackedInt32();
        var anisoLevel = metaReader.ReadPackedInt32();

        using var imageStream = new MemoryStream();
        ReadStreamAndLength(imageStream);

        var tex = new Texture2D(2, 2) { name = name };

        ImageConversion.LoadImage(tex, imageStream.ToArray());

        tex.filterMode = filterMode;
        tex.wrapMode = wrapMode;
        tex.anisoLevel = anisoLevel;
        tex.Apply();

        return tex;
    }
}
