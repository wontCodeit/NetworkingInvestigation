using System.Net.Sockets;
using System.Text;

namespace Core;

/// <summary>
/// Class for reading bytes that are from packets constructed by <see cref="PacketBuilder"/>.
/// </summary>
public class PacketReader: BinaryReader
{
    private readonly NetworkStream _netStream;

    public PacketReader(NetworkStream givenStream) : base(givenStream)
    {
        _netStream = givenStream;
    }

    /// <summary>
    /// Read the ASCII encoded string from a given packet NOTE: OpCode should already be read.
    /// </summary>
    /// <returns></returns>
    public string ReadMessage()
    {
        try
        {
            byte[] byteStore;
            var messageLength = ReadInt32();
            byteStore = new byte[messageLength];
            _ = _netStream.Read(byteStore, 0, messageLength);

            return Encoding.ASCII.GetString(byteStore);
        }
        catch (Exception e) when (e is EndOfStreamException or
                                  ArgumentOutOfRangeException or
                                  IOException or
                                  ObjectDisposedException)
        {
            return $"Message could not be read due to error occurring: {e.Message}";
        }
    }
}
