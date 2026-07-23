using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERGLauncher.Core.DialogSettings.Implementations;
using ERGLauncher.Models;

namespace ERGLauncher.ViewModels;

public partial class MessageViewModel : DialogViewModelBase
{
    private readonly IMessageModel model;

    public MessageViewModel(IMessageModel model)
        : base(model)
    {
        this.model = model ?? throw new ArgumentNullException(nameof(model));
        Title = model.Title ?? string.Empty;
        message = model.Message;
        details = model.Details;
        isShowDetails = model.IsShowDetails;
        model.PropertyChanged += OnModelPropertyChanged;
        CloseCommand = new RelayCommand(Close, () => !IsBusy);
    }

    [ObservableProperty]
    private string message;

    [ObservableProperty]
    private string? details;

    [ObservableProperty]
    private bool isShowDetails;

    public IRelayCommand CloseCommand { get; }

    public override void OnDialogOpened(object? parameter)
    {
        if (parameter is MessageDialogSettings settings)
        {
            model.LoadSettings(settings);
            Refresh();
        }
    }

    protected override void OnBusyStateChanged() => CloseCommand.NotifyCanExecuteChanged();

    protected override void DisposeManaged()
    {
        model.PropertyChanged -= OnModelPropertyChanged;
        base.DisposeManaged();
    }

    private void Close() => RaiseRequestClose(true);

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IMessageModel.Title): Title = model.Title ?? string.Empty; break;
            case nameof(IMessageModel.Message): Message = model.Message; break;
            case nameof(IMessageModel.Details): Details = model.Details; break;
            case nameof(IMessageModel.IsShowDetails): IsShowDetails = model.IsShowDetails; break;
            case null:
            case "": Refresh(); break;
        }
    }

    private void Refresh()
    {
        Title = model.Title ?? string.Empty;
        Message = model.Message;
        Details = model.Details;
        IsShowDetails = model.IsShowDetails;
    }
}
