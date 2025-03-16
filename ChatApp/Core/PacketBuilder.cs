using System.Text;

namespace Core;

/// <summary>
/// A class for making data packets.
/// </summary>
public class PacketBuilder
{
    private readonly MemoryStream _stream;

    /// <summary>
    /// Instantiates a <see cref="PacketBuilder"/>.
    /// </summary>
    public PacketBuilder()
    {
        _stream = new MemoryStream();
    }

    /// <summary>
    /// Create a packet that sends an OpCode and an ASCII encoded string 
    /// Packet OpCode schema:
    /// OpCodes from client
    /// 0 - attempt connection
    /// 2 - send (chat) message
    /// OpCodes from server:
    /// 1 - new client has connected
    /// 3 - client has disconnected
    /// 5 - new message
    /// </summary>
    /// <param name="opCode"> The OpCode that this packet will use. </param>
    /// <param name="message"> The message to be sent with the packet. </param>
    public void WritePacket(byte opCode, string message)
    {
        _stream.WriteByte(opCode);
        _stream.Write(BitConverter.GetBytes(message.Length));
        _stream.Write(Encoding.ASCII.GetBytes(message));
    }

    /// <summary>
    /// Read all bytes currently stored in the <see cref="PacketBuilder"/>'s <see cref="MemoryStream"/>.
    /// </summary>
    /// <returns> The packet as a byte array. </returns>
    public byte[] GetPacket() => _stream.ToArray();
}
