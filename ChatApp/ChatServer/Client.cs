using Core;
using System.Net.Sockets;

namespace ChatServer;
public class Client
{
    private readonly TcpClient _tcpClient;

    public string Username { get; private set; } = "No Username";
    public Guid Id { get; }

    public static Client Create(TcpClient tcpClient)
    {
        var client = new Client(tcpClient);
        var reader = new PacketReader(tcpClient.GetStream());
        if (reader.ReadByte() != 0)
        {
            throw new ArgumentException("Invalid OpCode for client instantiation");
        }

        client.Username = reader.ReadMessage();

        _ = Task.Run(client.Process);
        return client;
    }

    public void SendPacket(byte[] packet)
    {
        _tcpClient.GetStream().Write(packet);
    }

    private void Process()
    {
        var reader = new PacketReader(_tcpClient.GetStream());
        while (true)
        {
            try
            {
                byte opCode = reader.ReadByte();
                switch (opCode)
                {
                    case 2:
                        string message = reader.ReadMessage();
                        Console.WriteLine($"[{DateTime.UtcNow} {Username}:] {message}");
                        Server.BroadcastMessage($"[{DateTime.UtcNow.ToShortTimeString()} {Username}:] {message}");
                        break;
                    default:
                        Console.WriteLine("[{DateTime.UtcNow} {Username}] Unknown packet received!");
                        break;
                }
            }
            catch (Exception e) when (e is EndOfStreamException or
                                      ObjectDisposedException or
                                      IOException)
            {
                Console.WriteLine($"[{DateTime.UtcNow} {Username}:] Error occurred in Processing: {e.Message}");
                Server.BroadcastDisconnect(Id);
                _tcpClient.Close();
                break;
            }
        }
    }

    private Client(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;
        Id = Guid.NewGuid();
    }
}
