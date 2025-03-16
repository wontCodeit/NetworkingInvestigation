using Core;
using System.Net;
using System.Net.Sockets;

namespace ChatClient.Net;

public class ServerAccess
{
    private readonly TcpClient _client;

    public PacketReader? PacketReader { get; private set; }

    public event Action? ConnectedEvent;
    public event Action? UserDisconnectEvent;
    public event Action? ReceiveMessageEvent;

    public bool IsConnected { get; private set; }
    public ServerAccess()
    {
        _client = new TcpClient();
    }

    public void ConnectToServer(string username, string address = "127.0.0.1")
    {
        if (IsConnected || string.IsNullOrEmpty(username))
        {
            return;
        }

        // IP address, arbitrary port- TODO error checking, basically across the whole program...
        _client.Connect(IPAddress.Parse(address), 32888);
        IsConnected = _client.Connected;
        PacketReader = new PacketReader(_client.GetStream());

        var connectPacket = new PacketBuilder();
        connectPacket.WritePacket(0, username);
        _ = _client.Client.Send(connectPacket.GetPacket());

        ReadServerPackets();
    }

    public void SendMessage(string message)
    {
        var messagePacket = new PacketBuilder();
        messagePacket.WritePacket(2, message);
        _ = _client.Client.Send(messagePacket.GetPacket());
    }

    /// <summary>
    /// Offload packet reading to a separate, untracked thread TODO: learn about "tracking" threads...
    /// </summary>
    private void ReadServerPackets()
    {
        _ = Task.Run(() =>
        {
            while (true)
            {
                var opCode = PacketReader?.ReadByte();

                switch (opCode)
                {
                    case 1:
                        ConnectedEvent?.Invoke();
                        break;
                    case 3:
                        UserDisconnectEvent?.Invoke();
                        break;
                    case 5:
                        ReceiveMessageEvent?.Invoke();
                        break;
                    default:
                        Console.WriteLine("Unrecognised OpCode received");
                        break;
                }
            }
        });
    }
}
