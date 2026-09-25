namespace XPAB;

/// <summary>Defines extensions to <see cref="BinaryReader"/> to add functionality.</summary>
public static class BinaryReaderExtensions
{
    /// <summary>Extension methods.</summary>
    extension(BinaryReader reader)
    {
        /// <summary>Writes an unsigned LEB128 long integer.</summary>
        /// <returns>An unsigned LEB128 long integer.</returns>
        public ulong ReadPackedUInt64()
        {
            var result = 0ul;
            var shift = 0;

            while (true)
            {
                var b = reader.ReadByte();
                result |= (ulong)(b & 0x7F) << shift;

                if ((b & 0x80) == 0)
                    break;

                shift += 7;

                if (shift >= 70)
                    throw new InvalidDataException("VarInt too long");
            }

            return result;
        }

        /// <summary>Writes an unsigned LEB128 integer.</summary>
        /// <returns>An unsigned LEB128 integer.</returns>
        public uint ReadPackedUInt32()
        {
            var result = 0u;
            var shift = 0;

            while (true)
            {
                var b = reader.ReadByte();
                result |= (uint)(b & 0x7F) << shift;

                if ((b & 0x80) == 0)
                    break;

                shift += 7;

                if (shift >= 35)
                    throw new InvalidDataException("VarInt too long");
            }

            return result;
        }

        /// <summary>Writes a signed LEB128 long integer.</summary>
        /// <returns>A signed LEB128 long integer.</returns>
        public long ReadPackedInt64()
        {
            var val = reader.ReadPackedUInt64();
            return (long)(val >> 1) ^ -(long)(val & 1);
        }

        /// <summary>Writes a signed LEB128 integer.</summary>
        /// <returns>A signed LEB128 integer.</returns>
        public int ReadPackedInt32()
        {
            var val = reader.ReadPackedUInt32();
            return (int)(val >> 1) ^ -(int)(val & 1);
        }

        /// <summary>Reads a byte array that's prefixed by its size.</summary>
        /// <returns>The byte array that was written.</returns>
        public byte[] ReadBytesAndSize() => reader.ReadBytes(reader.ReadPackedInt32());
    }
}
