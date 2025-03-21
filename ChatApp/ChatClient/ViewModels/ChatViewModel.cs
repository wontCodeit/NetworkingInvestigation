using ChatClient.Net;
using ChatClient.Utilities;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChatClient.ViewModels;

public class ChatViewModel: ViewModelBase
{
    private readonly ServerAccess _serverAccess;

    public ObservableCollection<string> Messages { get; } = [];
    public ObservableCollection<UserModel> Users { get; } = [];
    public RelayCommand ConnectCommand { get; private set; }
    public RelayCommand SendMessageCommand { get; private set; }
    public string? Username { get; set; }
    public string? Message { get; set; }
    public string? Host { get; set; }
    public int? Port { get; set; }

    public ChatViewModel()
    {
        _serverAccess = new ServerAccess();
        _serverAccess.ConnectedEvent += OnUserConnected;
        _serverAccess.ReceiveMessageEvent += OnMessageReceived;
        _serverAccess.UserDisconnectEvent += OnUserDisconnect;

        ConnectCommand = new(o => _serverAccess.ConnectToServer(Username ?? "Formless", Host ?? "127.0.0.1", Port ?? 80),
                             o => ValidateConnect(Username ?? string.Empty));

        SendMessageCommand = new(o => _serverAccess.SendMessage(Message ?? "None"),
                                 o => ValidateSend(Message ?? string.Empty));
    }

    private void OnUserConnected()
    {
        var message = _serverAccess.PacketReader?.ReadMessage() ?? "Formless|No Id";
        var components = message.Split(['|']);
        var newUser = new UserModel
        {
            Username = components[0],
            Id = components[1]
        };

        // This check should also be performed server side, or eventually only server side
        if (Users.Any(u => u.Id == newUser.Id))
        {
            return;
        }

        // Because the event this method is subscribed to is invoked in a different thread AND this is wpf,
        // you have to return in some way(?) to the main thread
        Application.Current.Dispatcher.Invoke(() => Users.Add(newUser));
    }

    private void OnMessageReceived()
    {
        var message = _serverAccess.PacketReader?.ReadMessage() ?? $"[{DateTime.UtcNow}] Message Read Fail";
        Application.Current.Dispatcher.Invoke(() => Messages.Add(message));
    }

    private void OnUserDisconnect()
    {
        var id = _serverAccess.PacketReader?.ReadMessage() ?? "";
        var message = $"[{DateTime.UtcNow}] Client that disconnected could not be found";

        if (Users.FirstOrDefault(user => user.Id == id) is UserModel user)
        {
            message = $"[{DateTime.UtcNow} server:] {user.Username} disconnected";
            _ = Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }

        Application.Current.Dispatcher.Invoke(() => Messages.Add(message));
    }

    // how DRY is necessary? wondering if with lambdas there is a way to pass these more elegantly
    private bool ValidateConnect(string username) => !_serverAccess.IsConnected &&
                                                     !string.IsNullOrEmpty(username) &&
                                                     !string.IsNullOrEmpty(Host) &&
                                                     !username.Contains('|');

    private bool ValidateSend(string sendString) => _serverAccess.IsConnected &&
                                                    !string.IsNullOrEmpty(sendString) &&
                                                    !sendString.Contains('|');
}