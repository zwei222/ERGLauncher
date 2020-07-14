using System;
using System.Reactive.Disposables;
using System.Windows;
using ERGLauncher.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;

namespace ERGLauncher.ViewModels
{
    /// <summary>
    /// Base ViewModel.
    /// </summary>
    public abstract class ViewModelBase : IDisposable
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="model">Model</param>
        protected ViewModelBase(IModelBase model)
        {
            this.BusyNotifier = new BusyNotifier();
            this.IsBusy = this.BusyNotifier.ToReadOnlyReactivePropertySlim().AddTo(this.Disposable);
            this.Height = model.ToReactivePropertyAsSynchronized(myModel => myModel.Height)
                .AddTo(this.Disposable);
            this.Width = model.ToReactivePropertyAsSynchronized(myModel => myModel.Width)
                .AddTo(this.Disposable);
            this.Top = model.ToReactivePropertyAsSynchronized(myModel => myModel.Top)
                .AddTo(this.Disposable);
            this.Left = model.ToReactivePropertyAsSynchronized(myModel => myModel.Left)
                .AddTo(this.Disposable);
            this.WindowState = model.ToReactivePropertyAsSynchronized(myModel => myModel.WindowState)
                .AddTo(this.Disposable);
        }

        /// <summary>
        /// Window height.
        /// </summary>
        public ReactiveProperty<double> Height { get; }

        /// <summary>
        /// Window width.
        /// </summary>
        public ReactiveProperty<double> Width { get; }

        /// <summary>
        /// Window top position.
        /// </summary>
        public ReactiveProperty<double> Top { get; }

        /// <summary>
        /// Window left position.
        /// </summary>
        public ReactiveProperty<double> Left { get; }

        /// <summary>
        /// Window state.
        /// </summary>
        public ReactiveProperty<WindowState> WindowState { get; }

        /// <summary>
        /// <see langword="true" /> if the resource has been released; otherwise <see langword="false" />.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Composite disposable.
        /// </summary>
        protected CompositeDisposable Disposable { get; } = new CompositeDisposable();

        /// <summary>
        /// <see langword="true" /> if busy; otherwise <see langword="false" />.
        /// </summary>
        public ReadOnlyReactivePropertySlim<bool> IsBusy { get; }

        /// <summary>
        /// Busy notifier.
        /// </summary>
        public BusyNotifier BusyNotifier { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose a managed resource.
        /// </summary>
        protected virtual void DisposeManaged()
        {
            this.Disposable?.Dispose();
        }

        /// <summary>
        /// Dispose a unmanaged resource.
        /// </summary>
        protected virtual void DisposeUnmanaged()
        {
        }

        /// <summary>
        /// Release the used resources.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> if release explicitly; otherwise <see langword="false" /></param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.DisposeUnmanaged();

            if (disposing)
            {
                this.DisposeManaged();
            }

            this.IsDisposed = true;
        }
    }
}
