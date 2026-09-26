namespace XPAB;

internal static class BinaryWriterExtensions
{
    extension(BinaryWriter writer)
    {
        public void WritePacked(ulong value)
        {
            while (value >= 0x80)
            {
                writer.Write((byte)(value | 0x80));
                value >>= 7;
            }

            writer.Write((byte)value);
        }

        public void WritePacked(uint value)
        {
            while (value >= 0x80)
            {
                writer.Write((byte)(value | 0x80));
                value >>= 7;
            }

            writer.Write((byte)value);
        }

        public void WritePacked(int value) => writer.WritePacked((uint)((value << 1) ^ (value >> 31)));

        public void WritePacked(long value) => writer.WritePacked((ulong)((value << 1) ^ (value >> 63)));

        public void WriteBytesAndSize(byte[] bytes)
        {
            writer.WritePacked((uint)bytes.Length);
            writer.Write(bytes);
        }

        public void WriteArray<T>(T[] values, Action<BinaryWriter, T> writeAction)
        {
            for (var i = 0; i < values.Length; i++)
                writeAction(writer, values[i]);
        }

        public void WriteArrayAndSize<T>(T[] values, Action<BinaryWriter, T> writeAction)
        {
            writer.WritePacked((uint)values.Length);
            writer.WriteArray(values, writeAction);
        }
    }
}
