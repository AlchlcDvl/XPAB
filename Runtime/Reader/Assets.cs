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

    public AudioClip ReadAudioClip(string name, AudioSerialisationSettings settings)
    {
        var samples = ReadPackedInt32();
        var channels = ReadPackedInt32();
        var frequency = ReadPackedInt32();

        using var dataStream = new MemoryStream();
        ReadStreamAndLength(dataStream);
        dataStream.Position = 0;

        var totalSamples = samples * channels;
        var audioData = new float[totalSamples];

        switch (settings.compressionMethod)
        {
            case AudioCompressionMethod.Vorbis:
            {
                AudioDecoder.DecodeVorbis(dataStream, audioData, totalSamples);
                break;
            }

            case AudioCompressionMethod.ADPCM:
            {
                AudioDecoder.DecodeAdpcm(dataStream, audioData, channels);
                break;
            }

            default:
            {
                Stream targetStream = dataStream;

                try
                {
                    if (settings.compressionMethod == AudioCompressionMethod.Brotli)
                        targetStream = new BrotliStream(dataStream, CompressionMode.Decompress, true);

                    using var reader = new XPABReader(targetStream, Encoding.UTF8, true);
                    AudioDecoder.DequantizeAudio(audioData, settings.bitDepth, reader);
                }
                finally
                {
                    if (targetStream is BrotliStream brotli)
                        brotli.Dispose();
                }

                break;
            }
        }

        var clip = AudioClip.Create(name, samples, channels, frequency, false);

        if (clip.SetData(audioData, 0))
            throw new InvalidDataException("The audio data could not be set correctly.");

        return clip;
    }

    public Texture2D ReadTexture2D(string name, Texture2DSerialisationSettings settings)
    {
        var width = ReadPackedUInt32();
        var height = ReadPackedUInt32();
        var format = ReadPackedEnum<TextureFormat>();
        var mipmapCount = ReadPackedUInt32();

        var filterMode = ReadPackedEnum<FilterMode>();
        var wrapModeU = ReadPackedEnum<TextureWrapMode>();
        var wrapModeV = ReadPackedEnum<TextureWrapMode>();
        var wrapModeW = ReadPackedEnum<TextureWrapMode>();
        var wrapMode = ReadPackedEnum<TextureWrapMode>();
        var anisoLevel = ReadPackedInt32();
        var isSRGB = ReadBoolean();
        var isReadable = ReadBoolean();

        using var dataStream = new MemoryStream();
        ReadStreamAndLength(dataStream);
        dataStream.Position = 0;

        Stream targetStream = dataStream;
        byte[] textureData;

        try
        {
            if (settings.compressionMethod == Texture2DCompressionMethod.Brotli)
                targetStream = new BrotliStream(dataStream, CompressionMode.Decompress, true);

            using var fileReader = new XPABReader(targetStream, Encoding.UTF8, true);
            textureData = fileReader.ReadBytesAndSize();
        }
        finally
        {
            if (targetStream is BrotliStream brotli)
                brotli.Dispose();
        }

        var tex = new Texture2D((int)width, (int)height, format, (int)mipmapCount, !isSRGB)
        {
            name = name,
            filterMode = filterMode,
            wrapModeU = wrapModeU,
            wrapModeV = wrapModeV,
            wrapModeW = wrapModeW,
            wrapMode = wrapMode,
            anisoLevel = anisoLevel,
        };

        if (settings.compressionMethod is Texture2DCompressionMethod.PNG or Texture2DCompressionMethod.JPG)
        {
            if (!tex.LoadImage(textureData))
                throw new InvalidDataException("Could not correctly read file data.");
        }
        else
        {
            tex.LoadRawTextureData(textureData);
        }

        tex.Apply(mipmapCount > 1, !isReadable);

        return tex;
    }
}
