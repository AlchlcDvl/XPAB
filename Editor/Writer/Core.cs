using System.Text;

namespace XPAB.Writer;

internal sealed partial class XPABWriter(Stream output, Encoding encoding, bool leaveOpen) : BinaryWriter(output, encoding, leaveOpen)
{
    public long Position
    {
        get => BaseStream.Position;
        set => BaseStream.Position = value;
    }

    public XPABWriter(Stream output)
        : this(output, Encoding.UTF8, false)
    {
    }

    public XPABWriter(Stream output, Encoding encoding)
        : this(output, encoding, false)
    {
    }

    public XPABWriter(Stream output, bool leaveOpen)
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

    public void WritePacked(ushort value)
    {
        while (value >= 0x80)
        {
            Write((byte)(value | 0x80));
            value >>= 7;
        }

        Write((byte)value);
    }

    public void WritePacked(short value) => WritePacked((ushort)((value << 1) ^ (value >> 15)));

    public void WritePacked(int value) => WritePacked((uint)((value << 1) ^ (value >> 31)));

    public void WritePacked(long value) => WritePacked((ulong)((value << 1) ^ (value >> 63)));

    public void WriteBytesAndSize(byte[] bytes)
    {
        WritePacked((uint)bytes.Length);
        Write(bytes);
    }

    public void WriteArray<T>(T[] values, Action<XPABWriter, T> writeAction)
    {
        for (var i = 0; i < values.Length; i++)
            writeAction(this, values[i]);
    }

    public void WriteArrayAndSize<T>(T[] values, Action<XPABWriter, T> writeAction)
    {
        WritePacked((uint)values.Length);
        WriteArray(values, writeAction);
    }

    public void Write(Stream stream)
    {
        if (stream.CanSeek)
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

    public void WriteEnum<T>(T value)
        where T : unmanaged, Enum
        => EnumDelegates<T>.Writer(this, value);

    public void WritePackedEnum<T>(T value)
        where T : unmanaged, Enum
        => EnumDelegates<T>.PackedWriter(this, value);

    public void WriteEnumString<T>(T value)
        where T : unmanaged, Enum
        => EnumDelegates<T>.StringWriter(this, value);

    private static class EnumDelegates<T>
        where T : unmanaged, Enum
    {
        public static readonly Action<XPABWriter, T> Writer = GetWriter<T>();
        public static readonly Action<XPABWriter, T> PackedWriter = GetPackedWriter<T>();
        public static readonly Action<XPABWriter, T> StringWriter = (w, v) => w.Write(v.ToString());
    }

    private static unsafe Action<XPABWriter, T> GetWriter<T>()
        where T : unmanaged, Enum
    {
        var size = sizeof(T);
        var enumType = typeof(T);
        var underlying = Enum.GetUnderlyingType(enumType);
        return size switch
        {
            1 when underlying == typeof(sbyte) => (w, v) => w.Write(*(sbyte*)&v),
            1 => (w, v) => w.Write(*(byte*)&v),
            2 when underlying == typeof(short) => (w, v) => w.Write(*(short*)&v),
            2 => (w, v) => w.Write(*(ushort*)&v),
            4 when underlying == typeof(int) => (w, v) => w.Write(*(int*)&v),
            4 => (w, v) => w.Write(*(uint*)&v),
            8 when underlying == typeof(long) => (w, v) => w.Write(*(long*)&v),
            8 => (w, v) => w.Write(*(ulong*)&v),
            _ => throw new NotSupportedException($"Enum {enumType.Name} with underlying type {underlying.Name} is not supported."),
        };
    }

    private static unsafe Action<XPABWriter, T> GetPackedWriter<T>()
        where T : unmanaged, Enum
    {
        var size = sizeof(T);
        var enumType = typeof(T);
        var underlying = Enum.GetUnderlyingType(enumType);
        return size switch
        {
            1 when underlying == typeof(sbyte) => (w, v) => w.Write(*(sbyte*)&v),
            1 => (w, v) => w.Write(*(byte*)&v),
            2 when underlying == typeof(short) => (w, v) => w.WritePacked(*(short*)&v),
            2 => (w, v) => w.WritePacked(*(ushort*)&v),
            4 when underlying == typeof(int) => (w, v) => w.WritePacked(*(int*)&v),
            4 => (w, v) => w.WritePacked(*(uint*)&v),
            8 when underlying == typeof(long) => (w, v) => w.WritePacked(*(long*)&v),
            8 => (w, v) => w.WritePacked(*(ulong*)&v),
            _ => throw new NotSupportedException($"Enum {enumType.Name} with underlying type {underlying.Name} is not supported."),
        };
    }
}
