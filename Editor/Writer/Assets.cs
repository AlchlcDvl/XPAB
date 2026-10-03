using System.IO.Compression;
using XPAB.Assets;

namespace XPAB.Writer;

internal partial class XPABWriter
{
    public void WriteTextAsset(TextAsset asset) => WriteBytesAndSize(asset.bytes);

    public void WriteBinaryAsset(BinaryAsset asset) => WriteFile(asset.file);

    public void WriteAudioClip(AudioClip asset, AudioSerialisationSettings settings)
    {
        WritePacked(asset.samples);
        WritePacked(asset.channels);
        WritePacked(asset.frequency);

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

    public void WriteTexture2D(Texture2D asset, Texture2DSerialisationSettings settings)
    {
        WritePacked((uint)asset.width);
        WritePacked((uint)asset.height);
        WritePackedEnum(asset.format);
        WritePacked((uint)asset.mipmapCount);

        WritePackedEnum(asset.filterMode);
        WritePackedEnum(asset.wrapModeU);
        WritePackedEnum(asset.wrapModeV);
        WritePackedEnum(asset.wrapModeW);
        WritePackedEnum(asset.wrapMode);
        WritePacked(asset.anisoLevel);
        Write(asset.isDataSRGB);
        Write(asset.isReadable);

        var textureData = settings.compressionMethod is Texture2DCompressionMethod.PNG or Texture2DCompressionMethod.JPG
            ? TextureExtractor.ExtractViaBlit(asset, settings.compressionMethod)
            : TextureExtractor.ExtractRawTextureData(asset);

        using var dataStream = new MemoryStream();
        Stream targetStream = dataStream;

        try
        {
            if (settings.compressionMethod == Texture2DCompressionMethod.Brotli)
                targetStream = new BrotliStream(dataStream, CompressionLevel.Optimal, true);

            using var fileWriter = new XPABWriter(targetStream, true);
            fileWriter.WriteBytesAndSize(textureData);
        }
        finally
        {
            if (targetStream is BrotliStream brotli)
                brotli.Dispose();
        }

        WriteStreamAndLength(dataStream);
    }
}
