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

    public BinaryAsset ReadBinaryAsset(string bundleName, string name, string ext, List<IDisposable> registry)
    {
        var expectedLength = (long)ReadPackedUInt64();
        var safeFileName = $"{bundleName}_{name}.{ext}";
        var path = Path.Combine(XPABCore.CachePath, safeFileName);

        if (!File.Exists(path) || new FileInfo(path).Length != expectedLength)
        {
            using var fs = File.Create(path);
            ReadToStream(fs, expectedLength);
        }
        else
        {
            Position += expectedLength;
        }

        var asset = ScriptableObject.CreateInstance<BinaryAsset>();
        asset.file = path;
        asset.name = name;
        asset.FileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        registry.Add(asset);
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

        if (!clip.SetData(audioData, 0))
            throw new InvalidDataException("The audio data could not be set correctly.");

        return clip;
    }

    public Texture2D ReadTexture2D(string name, Texture2DCompressionMethod method)
    {
        var width = ReadPackedUInt32();
        var height = ReadPackedUInt32();
        var format = ReadPackedEnum<TextureFormat>();
        var mipmapCount = ReadPackedUInt32();

        var filterMode = ReadPackedEnum<FilterMode>();
        var wrapModeU = ReadPackedEnum<TextureWrapMode>();
        var wrapModeV = ReadPackedEnum<TextureWrapMode>();
        var wrapModeW = ReadPackedEnum<TextureWrapMode>();
        var anisoLevel = ReadPackedInt32();
        var isSRGB = ReadBoolean();
        var isReadable = ReadBoolean();

        var payloadLength = (long)ReadPackedUInt64();
        byte[] textureData;

        if (method == Texture2DCompressionMethod.Brotli)
        {
            using var brotli = new BrotliStream(BaseStream, CompressionMode.Decompress, true);
            using var ms = new MemoryStream();
            brotli.CopyTo(ms);
            textureData = ms.ToArray();
        }
        else
        {
            textureData = ReadBytes((int)payloadLength);
        }

        var tex = new Texture2D((int)width, (int)height, format, (int)mipmapCount, !isSRGB)
        {
            name = name,
            filterMode = filterMode,
            wrapModeU = wrapModeU,
            wrapModeV = wrapModeV,
            wrapModeW = wrapModeW,
            anisoLevel = anisoLevel,
        };

        var isEncoded = method is Texture2DCompressionMethod.PNG or Texture2DCompressionMethod.JPG;

        if (isEncoded)
        {
            if (!tex.LoadImage(textureData))
                throw new InvalidDataException("Could not correctly decode image payload.");
        }
        else
        {
            tex.LoadRawTextureData(textureData);
        }

        tex.Apply(isEncoded && mipmapCount > 1, !isReadable);
        return tex;
    }
}
