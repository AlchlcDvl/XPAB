using System.Text;
using XPAB.Assets;

namespace XPAB;

/// <summary>Writes assets that are easily serialisable.</summary>
public static class WriteEasyAssets
{
    private static readonly Action<BinaryWriter, float> WriteFloat = (w, f) => w.Write(f);

    /// <summary>Extension methods.</summary>
    extension(BinaryWriter writer)
    {
        /// <summary>Writes a <see cref="TextAsset"/>.</summary>
        /// <param name="asset">The asset to write.</param>
        public void WriteTextAssetV1(TextAsset asset)
        {
            writer.WritePacked(-1); // No metadata
            writer.WriteBytesAndSize(Encoding.UTF8.GetBytes(asset.text));
        }

        /// <summary>Writes a <see cref="BinaryAsset"/>.</summary>
        /// <param name="asset">The asset to write.</param>
        public void WriteBinaryAssetV1(BinaryAsset asset)
        {
            writer.WritePacked(-1); // No metadata
            writer.WriteBytesAndSize(asset.bytes);
        }

        /// <summary>Writes an <see cref="AudioClip"/>.</summary>
        /// <param name="asset">The asset to write.</param>
        public void WriteAudioClipV1(AudioClip asset)
        {
            using var metadataStream = new MemoryStream();
            using var metaWriter = new BinaryWriter(metadataStream);

            metaWriter.WritePacked(asset.samples);
            metaWriter.WritePacked(asset.channels);
            metaWriter.WritePacked(asset.frequency);

            asset.LoadAudioData();

            var audioData = new float[asset.samples * asset.channels];
            asset.GetData(audioData, 0);

            metaWriter.WriteArrayAndSize(audioData, WriteFloat);

            writer.WriteBytesAndSize(metadataStream.ToArray());

            writer.WritePacked(-1); // No file data
        }
    }
}
