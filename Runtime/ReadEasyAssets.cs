using System.Text;
using XPAB.Assets;

namespace XPAB;

/// <summary>Reads assets that are easily serialisable.</summary>
public static class ReadEasyAssets
{
    private static readonly Func<BinaryReader, float> ReadFloat = r => r.ReadSingle();

    /// <summary>Extension methods.</summary>
    extension(BinaryReader reader)
    {
        /// <summary>Reads a <see cref="TextAsset"/>.</summary>
        /// <returns>A deserialised instance of <see cref="TextAsset"/>.</returns>
        public TextAsset ReadTextAssetV1()
        {
            _ = reader.ReadPackedInt32(); // No metadata

            var bytes = reader.ReadBytesAndSize();
            var text = Encoding.UTF8.GetString(bytes);

            return new TextAsset(text);
        }

        /// <summary>Reads a <see cref="BinaryAsset"/>.</summary>
        /// <returns>A deserialised instance of <see cref="BinaryAsset"/>.</returns>
        public BinaryAsset ReadBinaryAssetV1()
        {
            _ = reader.ReadPackedInt32(); // No metadata

            var bytes = reader.ReadBytesAndSize();

            var asset = ScriptableObject.CreateInstance<BinaryAsset>();
            asset.bytes = bytes;
            return asset;
        }

        /// <summary>Reads a <see cref="AudioClip"/>.</summary>
        /// <returns>A deserialised instance of <see cref="AudioClip"/>.</returns>
        public AudioClip ReadAudioClipV1()
        {
            var samples = reader.ReadPackedInt32();
            var channels = reader.ReadPackedInt32();
            var frequency = reader.ReadPackedInt32();

            var data = reader.ReadArrayAndSize(ReadFloat);

            var clip = AudioClip.Create("temp", samples, channels, frequency, false);
            clip.SetData(data, 0);

            _ = reader.ReadPackedInt32(); // No file data

            return clip;
        }
    }
}
