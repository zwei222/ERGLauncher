using System;
using ERGLauncher.Models;
using Prism.Services.Dialogs;

namespace ERGLauncher.ViewModels
{
    /// <summary>
    /// Base dialog ViewModel.
    /// </summary>
    public abstract class DialogViewModelBase : ViewModelBase, IDialogAware
    {
        /// <inheritdoc />
        public event Action<IDialogResult> RequestClose = null!;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="model">Model</param>
        protected DialogViewModelBase(IModelBase model)
            : base(model)
        {
            this.Title = string.Empty;
        }

        /// <inheritdoc />
        public string Title { get; protected set; }

        /// <inheritdoc />
        public virtual bool CanCloseDialog()
        {
            return true;
        }

        /// <inheritdoc />
        public virtual void OnDialogClosed()
        {
            this.RequestClose -= this.OnDialogClosing;
        }

        /// <inheritdoc />
        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            this.RequestClose += this.OnDialogClosing;
        }

        /// <summary>
        /// Called when the dialog is closing.
        /// </summary>
        /// <param name="dialogResult">Dialog result</param>
        protected virtual void OnDialogClosing(IDialogResult dialogResult)
        {
        }

        /// <summary>
        /// Raise RequestClose event
        /// </summary>
        /// <param name="dialogResult">Dialog result</param>
        protected virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            this.RequestClose?.Invoke(dialogResult);
        }
    }
}
