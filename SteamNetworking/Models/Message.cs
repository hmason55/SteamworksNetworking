
using Newtonsoft.Json;
using System.Text;

namespace SteamworksNetworking.Models;

public class Message
{
    private ushort _readPosition;
    private ushort _writePosition;

    public int UnreadLength => _writePosition - _readPosition;
    public int WrittenLength => _writePosition;

    internal int UnwrittenLength => Buffer.Count - _writePosition;

    public List<byte> Buffer { get; private set; } = new();

    public Message(ushort id)
    {
        WriteUShort(id);
    }

    public Message(ushort id, byte[] data)
    {
        WriteUShort(id);
        WriteBytes(data);
    }


    public Message(byte[] data)
    {
        WriteBytes(data);
    }

    public static Message Create<T>(ushort id, T obj)
    {
        Message message = new(id);
        message.Write(obj);
        return message;
    }

    public Message Initialize()
    {
        _readPosition = 0;
        _writePosition = 0;
        return this;
    }

    public ushort ReadUShort()
    {
        ushort data = BitConverter.ToUInt16(Buffer.ToArray(), _readPosition);
        _readPosition += sizeof(ushort);
        return data;
    }

    public Message WriteUShort(ushort data)
    {
        Buffer.AddRange(BitConverter.GetBytes(data));
        _writePosition += sizeof(ushort);
        return this;
    }

    public byte[] ReadBytes()
    {
        byte[] data = Buffer.GetRange(_readPosition, UnreadLength).ToArray();
        _readPosition += (ushort)data.Length;
        return data;
    }

    public Message WriteBytes(byte[] value)
    {
        Buffer.AddRange(value);
        _writePosition += (ushort)value.Length;
        return this;
    }

    public T Read<T>()
    {
        T data = JsonConvert.DeserializeObject<T>(Encoding.ASCII.GetString(Buffer.ToArray(), _readPosition, UnreadLength));
        _readPosition += (ushort)UnreadLength;
        return data;
    }

    public Message Write<T>(T obj)
    {
        byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(obj, Formatting.None));
        Buffer.AddRange(data);
        _writePosition += (ushort)data.Length;
        return this;
    }
}
