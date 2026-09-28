using System.Text;

namespace XPAB.Reader;

internal sealed partial class AssetReader(Stream output, Encoding encoding, bool leaveOpen) : BinaryReader(output, encoding, leaveOpen)
{
    public AssetReader(Stream output)
        : this(output, Encoding.UTF8, false)
    {
    }

    public AssetReader(Stream output, Encoding encoding)
        : this(output, encoding, false)
    {
    }

    public AssetReader(Stream output, bool leaveOpen)
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
        var val = ReadPackedUInt64();
        return (long)(val >> 1) ^ -(long)(val & 1);
    }

    public int ReadPackedInt32()
    {
        var val = ReadPackedUInt32();
        return (int)(val >> 1) ^ -(int)(val & 1);
    }

    public byte[] ReadBytesAndSize() => ReadBytes(ReadPackedInt32());

    public T[] ReadArray<T>(int count, Func<AssetReader, T> readAction)
    {
        var array = new T[count];

        for (var i = 0; i < count; i++)
            array[i] = readAction(this);

        return array;
    }

    public T[] ReadArrayAndSize<T>(Func<AssetReader, T> readAction) => ReadArray((int)ReadPackedUInt32(), readAction);

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
}
