using System.Text;

namespace XPAB.Reader;

internal sealed partial class XPABReader(Stream output, Encoding encoding, bool leaveOpen) : BinaryReader(output, encoding, leaveOpen)
{
    public long Position
    {
        get => BaseStream.Position;
        set => BaseStream.Position = value;
    }

    public long Length => BaseStream.Length;

    public XPABReader(Stream output)
        : this(output, Encoding.UTF8, false)
    {
    }

    public XPABReader(Stream output, Encoding encoding)
        : this(output, encoding, false)
    {
    }

    public XPABReader(Stream output, bool leaveOpen)
        : this(output, Encoding.UTF8, leaveOpen)
    {
    }

    public ulong ReadPackedUInt64()
    {
        var result = 0ul;
        var shift = 0;

        while (true)
        {
            var b = ReadByte();
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
            var b = ReadByte();
            result |= (uint)((b & 0x7F) << shift);

            if ((b & 0x80) == 0)
                break;

            shift += 7;

            if (shift >= 35)
                throw new InvalidDataException("VarInt too long");
        }

        return result;
    }

    public ushort ReadPackedUInt16()
    {
        var result = (ushort)0;
        var shift = 0;

        while (true)
        {
            var b = ReadByte();
            result |= (ushort)((b & 0x7F) << shift);

            if ((b & 0x80) == 0)
                break;

            shift += 7;

            if (shift >= 21)
                throw new InvalidDataException("VarInt too long");
        }

        return result;
    }

    public long ReadPackedInt64()
    {
        var val = ReadPackedUInt64();
        return (long)(val >> 1) ^ -(long)(val & 1);
    }

    public int ReadPackedInt32()
    {
        var val = ReadPackedUInt32();
        return (int)(val >> 1) ^ -(int)(val & 1);
    }

    public short ReadPackedInt16()
    {
        var val = ReadPackedUInt16();
        return (short)((val >> 1) ^ -(val & 1));
    }

    public byte[] ReadBytesAndSize() => ReadBytes((int)ReadPackedUInt32());

    public T[] ReadArray<T>(int count, Func<XPABReader, T> readAction)
    {
        var array = new T[count];

        for (var i = 0; i < count; i++)
            array[i] = readAction(this);

        return array;
    }

    public T[] ReadArrayAndSize<T>(Func<XPABReader, T> readAction) => ReadArray((int)ReadPackedUInt32(), readAction);

    public void ReadToStream(Stream stream, long amount)
    {
        var buffer = new byte[8192];
        var remaining = amount;

        while (remaining > 0)
        {
            var bytesToRead = (int)Math.Min(buffer.Length, remaining);
            var bytesRead = BaseStream.Read(buffer, 0, bytesToRead);

            if (bytesRead == 0)
                throw new EndOfStreamException("Reached the end of the stream before reading the requested amount.");

            stream.Write(buffer, 0, bytesRead);
            remaining -= bytesRead;
        }
    }

    public void ReadStreamAndLength(Stream destination)
    {
        var length = (long)ReadPackedUInt64();
        ReadToStream(destination, length);
    }

    public void ReadToFile(string path)
    {
        using var fs = File.Create(path);
        ReadStreamAndLength(fs);
    }

    public T ReadEnum<T>()
        where T : unmanaged, Enum
        => EnumDelegates<T>.Reader(this);

    public T ReadPackedEnum<T>()
        where T : unmanaged, Enum
        => EnumDelegates<T>.PackedReader(this);

    public T ReadEnumString<T>()
        where T : unmanaged, Enum
        => EnumDelegates<T>.StringReader(this);

    private static class EnumDelegates<T>
        where T : unmanaged, Enum
    {
        public static readonly Func<XPABReader, T> Reader = GetReader<T>();
        public static readonly Func<XPABReader, T> PackedReader = GetPackedReader<T>();
        public static readonly Func<XPABReader, T> StringReader = r => Enum.Parse<T>(r.ReadString());
    }

    private static unsafe Func<XPABReader, T> GetReader<T>()
        where T : unmanaged, Enum
    {
        var size = sizeof(T);
        var enumType = typeof(T);
        var underlying = Enum.GetUnderlyingType(enumType);
        return size switch
        {
            1 when underlying == typeof(sbyte) => r => { var v = r.ReadSByte(); return *(T*)&v; },
            1 => r => { var v = r.ReadByte(); return *(T*)&v; },
            2 when underlying == typeof(short) => r => { var v = r.ReadInt16(); return *(T*)&v; },
            2 => r => { var v = r.ReadUInt16(); return *(T*)&v; },
            4 when underlying == typeof(int) => r => { var v = r.ReadInt32(); return *(T*)&v; },
            4 => r => { var v = r.ReadUInt32(); return *(T*)&v; },
            8 when underlying == typeof(long) => r => { var v = r.ReadInt64(); return *(T*)&v; },
            8 => r => { var v = r.ReadUInt64(); return *(T*)&v; },
            _ => throw new NotSupportedException($"Enum {enumType.Name} with underlying type {underlying.Name} is not supported."),
        };
    }

    private static unsafe Func<XPABReader, T> GetPackedReader<T>()
        where T : unmanaged, Enum
    {
        var size = sizeof(T);
        var enumType = typeof(T);
        var underlying = Enum.GetUnderlyingType(enumType);
        return size switch
        {
            1 when underlying == typeof(sbyte) => r => { var v = r.ReadSByte(); return *(T*)&v; },
            1 => r => { var v = r.ReadByte(); return *(T*)&v; },
            2 when underlying == typeof(short) => r => { var v = r.ReadPackedInt16(); return *(T*)&v; },
            2 => r => { var v = r.ReadPackedUInt16(); return *(T*)&v; },
            4 when underlying == typeof(int) => r => { var v = r.ReadPackedInt32(); return *(T*)&v; },
            4 => r => { var v = r.ReadPackedUInt32(); return *(T*)&v; },
            8 when underlying == typeof(long) => r => { var v = r.ReadPackedInt64(); return *(T*)&v; },
            8 => r => { var v = r.ReadPackedUInt64(); return *(T*)&v; },
            _ => throw new NotSupportedException($"Enum {enumType.Name} with underlying type {underlying.Name} is not supported."),
        };
    }
}
