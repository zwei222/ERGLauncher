using System;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ERGLauncher.ViewModels;

/// <summary>
/// Base class for all view models. Provides a shared busy-state scope that
/// disables commands while an asynchronous operation is running.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject, IDisposable
{
    private int busyCount;

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
            DisposeManaged();
        }

        IsDisposed = true;
    }

    protected virtual void DisposeManaged()
    {
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
