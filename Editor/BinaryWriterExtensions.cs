namespace XPAB;

/// <summary>Defines extensions to <see cref="BinaryWriter"/> to add functionality.</summary>
public static class BinaryWriterExtensions
{
    /// <summary>Extension methods.</summary>
    extension(BinaryWriter writer)
    {
        /// <summary>Writes an unsigned LEB128 long integer.</summary>
        /// <param name="value">The number to write.</param>
        public void WritePacked(ulong value)
        {
            while (value >= 0x80)
            {
                writer.Write((byte)(value | 0x80));
                value >>= 7;
            }

            writer.Write((byte)value);
        }

        /// <summary>Writes an unsigned LEB128 integer.</summary>
        /// <param name="value">The number to write.</param>
        public void WritePacked(uint value)
        {
            while (value >= 0x80)
            {
                writer.Write((byte)(value | 0x80));
                value >>= 7;
            }

            writer.Write((byte)value);
        }

        /// <summary>Writes a signed LEB128 integer.</summary>
        /// <param name="value">The number to write.</param>
        public void WritePacked(int value) => writer.WritePacked((uint)((value << 1) ^ (value >> 31)));

        /// <summary>Writes a signed LEB128 long integer.</summary>
        /// <param name="value">The number to write.</param>
        public void WritePacked(long value) => writer.WritePacked((ulong)((value << 1) ^ (value >> 63)));

        /// <summary>Writes a byte array prefixed by its length.</summary>
        /// <param name="bytes">The bytes to write.</param>
        public void WriteBytesAndSize(byte[] bytes)
        {
            writer.WritePacked(bytes.Length);
            writer.Write(bytes);
        }
    }
}
