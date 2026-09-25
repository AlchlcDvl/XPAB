using System.Text;
using XPAB.Assets;

namespace XPAB;

/// <summary>Writes assets that are easily serialisable.</summary>
public static class WriteEasyAssets
{
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
    }
}
