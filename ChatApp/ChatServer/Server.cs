﻿using Core;
using System.Net;
using System.Net.Sockets;

namespace ChatServer;

/// <summary>
/// Contains methods for sending and receiving packets, as well as the <see cref="TcpListener"/> that receives connections.
/// </summary>
public class Server
{
    private readonly List<Client> _clients = [];
    private readonly TcpListener _listener;

    public static Server Instance { get; } = new Server();

    private Server()
    {
        IPHostEntry? localHostIp = Dns.GetHostEntry(string.Empty);

        // grabs the address, last one should be the ipv4 address
        _listener = localHostIp?.AddressList.Last() is IPAddress ip ?
            new(ip, 32888)
          : new(IPAddress.Parse("127.0.0.1"), 32888);
        Console.WriteLine(_listener?.LocalEndpoint.ToString());

        // TODO find out about backlog
        _listener!.Start();
    }

    /// <summary>
    /// Infinite loop of receiving new connections to the <see cref="TcpListener"/> in this server instance.
    /// </summary>
    public void Start()
    {
        while (true)
        {
            var client = Client.Create(_listener.AcceptTcpClient());
            _clients.Add(client);

            Console.WriteLine($"[{DateTime.UtcNow} server:] Client connected at with username: {client.Username}");
            BroadcastConnection();
        }
    }

    public static void BroadcastMessage(string message)
    {
        var messagePacket = new PacketBuilder();
        messagePacket.WritePacket(5, message);
        BroadcastPacket(messagePacket.GetPacket());
    }

    public static void BroadcastDisconnect(Guid id)
    {
        Client? clintToDC = Instance._clients.FirstOrDefault(client => client.Id == id);
        if (clintToDC == null)
        {
            Console.WriteLine($"[{DateTime.UtcNow} server:] Client could not be disconnected as they could not be found.");
            return;
        }

        _ = Instance._clients.Remove(clintToDC);
        var dcPacket = new PacketBuilder();
        Console.WriteLine($"[{DateTime.UtcNow} server:] Client with ID {clintToDC.Id.ToString()} has disconnected!");
        dcPacket.WritePacket(3, clintToDC.Id.ToString());
        BroadcastPacket(dcPacket.GetPacket());
    }

    private static void BroadcastConnection()
    {
        Instance._clients.ForEach(client =>
        {
            Instance._clients.ForEach(user =>
            {
                var builder = new PacketBuilder();
                var message = user.Username;
                message += '|';
                message += user.Id.ToString();
                builder.WritePacket(1, message);
                client.SendPacket(builder.GetPacket());
            });
        });
    }

    private static void BroadcastPacket(byte[] packet)
    {
        Instance._clients.ForEach(client =>
        {
            client.SendPacket(packet);
        });
    }
}