namespace ChatClient.ViewModels;

public sealed class MainViewModel
{
    public ViewModelBase SelectedViewModel { get; private set; } = new ChatViewModel();
}
