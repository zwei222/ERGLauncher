using System;
using CommunityToolkit.Mvvm.ComponentModel;
using ERGLauncher.Models;

namespace ERGLauncher.ViewModels;

public abstract partial class DialogViewModelBase : ViewModelBase
{
    protected DialogViewModelBase(IModelBase model)
        : base(model)
    {
    }

    [ObservableProperty]
    private string title = string.Empty;

    public event EventHandler<DialogCloseRequestedEventArgs>? RequestClose;

    public virtual bool CanCloseDialog() => true;

    public virtual void OnDialogOpened(object? parameter)
    {
    }

    public virtual void OnDialogClosed()
    {
    }

    protected void RaiseRequestClose(bool accepted, object? value = null) =>
        RequestClose?.Invoke(this, new DialogCloseRequestedEventArgs(accepted, value));
}

public sealed class DialogCloseRequestedEventArgs(bool accepted, object? value) : EventArgs
{
    public bool Accepted { get; } = accepted;

    public object? Value { get; } = value;
}
