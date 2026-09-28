using System.Text;

namespace XPAB.Writer;

internal sealed partial class AssetWriter(Stream output, Encoding encoding, bool leaveOpen) : BinaryWriter(output, encoding, leaveOpen)
{
    public AssetWriter(Stream output)
        : this(output, Encoding.UTF8, false)
    {
    }

    public AssetWriter(Stream output, Encoding encoding)
        : this(output, encoding, false)
    {
    }

    public AssetWriter(Stream output, bool leaveOpen)
        : this(output, Encoding.UTF8, leaveOpen)
    {
    }

    public void WritePacked(ulong value)
    {
        while (value >= 0x80)
        {
            Write((byte)(value | 0x80));
            value >>= 7;
        }

        Write((byte)value);
    }

    public void WritePacked(uint value)
    {
        while (value >= 0x80)
        {
            Write((byte)(value | 0x80));
            value >>= 7;
        }

        Write((byte)value);
    }

    public void WritePacked(int value) => WritePacked((uint)((value << 1) ^ (value >> 31)));

    public void WritePacked(long value) => WritePacked((ulong)((value << 1) ^ (value >> 63)));

    public void WriteBytesAndSize(byte[] bytes)
    {
        WritePacked((uint)bytes.Length);
        Write(bytes);
    }

    public void WriteArray<T>(T[] values, Action<AssetWriter, T> writeAction)
    {
        for (var i = 0; i < values.Length; i++)
            writeAction(this, values[i]);
    }

    public void WriteArrayAndSize<T>(T[] values, Action<AssetWriter, T> writeAction)
    {
        WritePacked((uint)values.Length);
        WriteArray(values, writeAction);
    }

    public void Write(Stream stream)
    {
        stream.Position = 0;
        stream.CopyTo(BaseStream);
    }

    public void WriteStreamAndLength(Stream stream)
    {
        WritePacked((ulong)stream.Length);
        Write(stream);
    }

    public void WriteFile(string path)
    {
        using var fs = File.OpenRead(path);
        WriteStreamAndLength(fs);
    }
}
