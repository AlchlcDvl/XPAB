namespace XPAB;

internal static class BinaryReaderExtensions
{
    extension(BinaryReader reader)
    {
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

        public long ReadPackedInt64()
        {
            var val = reader.ReadPackedUInt64();
            return (long)(val >> 1) ^ -(long)(val & 1);
        }

        public int ReadPackedInt32()
        {
            var val = reader.ReadPackedUInt32();
            return (int)(val >> 1) ^ -(int)(val & 1);
        }

        public byte[] ReadBytesAndSize() => reader.ReadBytes(reader.ReadPackedInt32());

        public T[] ReadArray<T>(int count, Func<BinaryReader, T> readAction)
        {
            var array = new T[count];

            for (var i = 0; i < count; i++)
                array[i] = readAction(reader);

            return array;
        }

        public T[] ReadArrayAndSize<T>(Func<BinaryReader, T> readAction) => reader.ReadArray((int)reader.ReadPackedUInt32(), readAction);
    }
}
