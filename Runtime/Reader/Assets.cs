using System.IO.Compression;
using System.Text;
using XPAB.Assets;

namespace XPAB.Reader;

internal partial class XPABReader
{
    public TextAsset ReadTextAsset(string name)
    {
        var bytes = ReadBytesAndSize();
        return new TextAsset(TextAsset.DecodeString(bytes)) { name = name };
    }

    public BinaryAsset ReadBinaryAsset(string name, string ext)
    {
        var path = Path.Combine(XPABCore.CachePath, $"{name}.{ext}");

        ReadToFile(path);

        var asset = ScriptableObject.CreateInstance<BinaryAsset>();
        asset.file = path;
        asset.name = name;
        asset.FileStream = File.OpenRead(path);

        return asset;
    }

    public AudioClip ReadAudioClip(string name)
    {
        var samples = ReadPackedInt32();
        var channels = ReadPackedInt32();
        var frequency = ReadPackedInt32();

        using var compressedStream = new MemoryStream();
        ReadStreamAndLength(compressedStream);
        compressedStream.Position = 0;

        var totalSamples = samples * channels;
        var audioData = new float[totalSamples];

        using (var brotliStream = new BrotliStream(compressedStream, CompressionMode.Decompress, true))
        using (var fileReader = new XPABReader(brotliStream, Encoding.UTF8, true))
        {
            for (var i = 0; i < totalSamples; i++)
                audioData[i] = fileReader.ReadInt16() / 32767f;
        }

        var clip = AudioClip.Create(name, samples, channels, frequency, false);
        clip.SetData(audioData, 0);
        return clip;
    }

    public Texture2D ReadTexture2D(string name)
    {
        var filterMode = ReadPackedEnum<FilterMode>();
        var wrapMode = ReadPackedEnum<TextureWrapMode>();
        var wrapModeU = ReadPackedEnum<TextureWrapMode>();
        var wrapModeV = ReadPackedEnum<TextureWrapMode>();
        var wrapModeW = ReadPackedEnum<TextureWrapMode>();
        var anisoLevel = ReadPackedInt32();
        var hasMips = ReadBoolean();
        var isSRGB = ReadBoolean();
        var isReadable = ReadBoolean();
        var bytes = ReadBytesAndSize();

        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, hasMips, isSRGB) { name = name };

        tex.LoadImage(bytes);
        tex.filterMode = filterMode;
        tex.wrapModeU = wrapModeU;
        tex.wrapModeV = wrapModeV;
        tex.wrapModeW = wrapModeW;
        tex.wrapMode = wrapMode;
        tex.anisoLevel = anisoLevel;
        tex.Apply(hasMips, !isReadable);

        return tex;
    }
}
