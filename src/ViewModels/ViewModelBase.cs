using System;
using System.ComponentModel;
using System.Threading;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using ERGLauncher.Models;

namespace ERGLauncher.ViewModels;

public abstract partial class ViewModelBase : ObservableObject, IDisposable
{
    private readonly IModelBase model;
    private int busyCount;

    protected ViewModelBase(IModelBase model)
    {
        this.model = model ?? throw new ArgumentNullException(nameof(model));
        height = model.Height;
        width = model.Width;
        top = model.Top;
        left = model.Left;
        windowState = model.WindowState;
        model.PropertyChanged += OnModelPropertyChanged;
    }

    [ObservableProperty]
    private double height;

    [ObservableProperty]
    private double width;

    [ObservableProperty]
    private double top;

    [ObservableProperty]
    private double left;

    [ObservableProperty]
    private WindowState windowState;

    [ObservableProperty]
    private bool isBusy;

    public bool IsDisposed { get; private set; }

    protected IDisposable BeginBusy()
    {
        if (Interlocked.Increment(ref busyCount) == 1)
        {
            IsBusy = true;
        }

        return new BusyScope(this);
    }

    protected virtual void OnBusyStateChanged()
    {
    }

    partial void OnHeightChanged(double value) => model.Height = value;

    partial void OnWidthChanged(double value) => model.Width = value;

    partial void OnTopChanged(double value) => model.Top = value;

    partial void OnLeftChanged(double value) => model.Left = value;

    partial void OnWindowStateChanged(WindowState value) => model.WindowState = value;

    partial void OnIsBusyChanged(bool value) => OnBusyStateChanged();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            model.PropertyChanged -= OnModelPropertyChanged;
            DisposeManaged();
        }

        IsDisposed = true;
    }

    protected virtual void DisposeManaged()
    {
    }

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IModelBase.Height): Height = model.Height; break;
            case nameof(IModelBase.Width): Width = model.Width; break;
            case nameof(IModelBase.Top): Top = model.Top; break;
            case nameof(IModelBase.Left): Left = model.Left; break;
            case nameof(IModelBase.WindowState): WindowState = model.WindowState; break;
            case null:
            case "":
                Height = model.Height;
                Width = model.Width;
                Top = model.Top;
                Left = model.Left;
                WindowState = model.WindowState;
                break;
        }
    }

    private void EndBusy()
    {
        if (Interlocked.Decrement(ref busyCount) == 0)
        {
            IsBusy = false;
        }
    }

    private sealed class BusyScope : IDisposable
    {
        private Action? endBusy;

        public BusyScope(ViewModelBase owner)
        {
            endBusy = owner.EndBusy;
        }

        public void Dispose() => Interlocked.Exchange(ref endBusy, null)?.Invoke();
    }
}
