extern alias MigratedCore;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessageDialogSettings = MigratedCore::ERGLauncher.Core.Models.MessageDialogSettings;

namespace ERGLauncher.ViewModels;

/// <summary>
/// View model for the message dialog content view.
/// </summary>
public sealed partial class MessageViewModel : DialogViewModelBase
{
    [ObservableProperty]
    private string message = string.Empty;

    [ObservableProperty]
    private string? details;

    [ObservableProperty]
    private bool isShowDetails;

    public MessageViewModel()
    {
        CloseCommand = new RelayCommand(Close, () => !IsBusy);
    }

    public IRelayCommand CloseCommand { get; }

    public override void OnDialogOpened(object? parameter)
    {
        if (parameter is MessageDialogSettings settings)
        {
            Title = settings.Title ?? string.Empty;
            Message = settings.Message;
            Details = settings.Details;
            IsShowDetails = !string.IsNullOrEmpty(settings.Details);
        }
    }

    protected override void OnBusyStateChanged() => CloseCommand.NotifyCanExecuteChanged();

    private void Close() => RaiseRequestClose(true);
}
