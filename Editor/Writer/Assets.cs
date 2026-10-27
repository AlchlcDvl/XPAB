using System.IO.Compression;
using XPAB.Assets;
using XPAB.Settings;

namespace XPAB.Writer;

public partial class XPABWriter
{
    public void WriteTextAsset(TextAsset asset) => WriteBytesAndSize(asset.bytes);

    public void WriteBinaryAsset(BinaryAsset asset) => WriteFile(asset.file);

    public void WriteAudioClip(AudioClip asset, AudioClipSettings settings)
    {
        WritePacked(asset.samples);
        WritePacked(asset.channels);
        WritePacked(asset.frequency);
        WritePackedEnum(asset.loadType);

        if (!asset.LoadAudioData())
            throw new InvalidOperationException("Received a malformed asset.");

        var audioData = new float[asset.samples * asset.channels];
        asset.GetData(audioData, 0);

        using var dataStream = new MemoryStream();

        switch (settings.compressionMethod)
        {
            case AudioCompressionMethod.Vorbis:
            {
                AudioEncoder.EncodeVorbis(audioData, asset.channels, asset.frequency, dataStream);
                break;
            }

            case AudioCompressionMethod.ADPCM:
            {
                AudioEncoder.EncodeAdpcm(audioData, asset.channels, dataStream);
                break;
            }

            default:
            {
                Stream targetStream = dataStream;

                try
                {
                    if (settings.compressionMethod == AudioCompressionMethod.Brotli)
                        targetStream = new BrotliStream(dataStream, CompressionLevel.Optimal, true);

                    using var writer = new XPABWriter(targetStream, true);
                    AudioEncoder.QuantizeAudio(audioData, settings.bitDepth, writer);
                }
                finally
                {
                    if (targetStream is BrotliStream brotli)
                        brotli.Dispose();
                }

                break;
            }
        }

        WriteStreamAndLength(dataStream);
    }

    public void WriteTexture2D(Texture2D asset, Texture2DCompressionMethod method)
    {
        var isEncoded = method is Texture2DCompressionMethod.PNG or Texture2DCompressionMethod.JPG;

        WritePacked((uint)asset.width);
        WritePacked((uint)asset.height);
        WritePackedEnum(isEncoded ? TextureFormat.RGBA32 : asset.format);
        WritePacked((uint)asset.mipmapCount);

        WritePackedEnum(asset.filterMode);
        WritePackedEnum(asset.wrapModeU);
        WritePackedEnum(asset.wrapModeV);
        WritePackedEnum(asset.wrapModeW);
        WritePacked(asset.anisoLevel);
        Write(asset.isDataSRGB);
        Write(asset.isReadable);

        asset.EncodeToEXR();

        var textureData = isEncoded
            ? TextureExtractor.ExtractViaBlit(asset, method)
            : TextureExtractor.ExtractRawTextureData(asset);

        using var dataStream = new MemoryStream();
        Stream targetStream = dataStream;

        try
        {
            if (method == Texture2DCompressionMethod.Brotli)
                targetStream = new BrotliStream(dataStream, CompressionLevel.Optimal, true);

            targetStream.Write(textureData);
        }
        finally
        {
            if (targetStream is BrotliStream brotli)
                brotli.Dispose();
        }

        WriteStreamAndLength(dataStream);
    }
}
