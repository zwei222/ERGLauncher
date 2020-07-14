using System;
using System.Reactive.Linq;
using ERGLauncher.Core.DialogSettings.Implementations;
using ERGLauncher.Models;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace ERGLauncher.ViewModels
{
    /// <summary>
    /// Message dialog ViewModel.
    /// </summary>
    public class MessageViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Message model.
        /// </summary>
        private readonly IMessageModel model;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="model">Model</param>
        public MessageViewModel(IMessageModel model)
            : base(model)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            var title = this.model.Title;

            if (title != null)
            {
                this.Title = title;
            }

            // properties
            this.Message = this.model.ObserveProperty(myModel => myModel.Message).ToReadOnlyReactivePropertySlim()
                .AddTo(this.Disposable);
            this.Details = this.model.ObserveProperty(myModel => myModel.Details).ToReadOnlyReactivePropertySlim()
                .AddTo(this.Disposable);
            this.IsShowDetails = this.model.ObserveProperty(myModel => myModel.IsShowDetails).ToReadOnlyReactivePropertySlim()
                .AddTo(this.Disposable);

            // commands
            this.CloseCommand = this.IsBusy.Select(isBusy => !isBusy).ToReactiveCommand().AddTo(this.Disposable);
            this.CloseCommand.Subscribe(this.Close);
        }

        /// <summary>
        /// Message.
        /// </summary>
        public ReadOnlyReactivePropertySlim<string> Message { get; }

        /// <summary>
        /// Detailed message.
        /// </summary>
        public ReadOnlyReactivePropertySlim<string?> Details { get; }

        /// <summary>
        /// <see langword="true" /> if show detailed message; otherwise <see langword="false" />.
        /// </summary>
        public ReadOnlyReactivePropertySlim<bool> IsShowDetails { get; }

        /// <summary>
        /// Close command.
        /// </summary>
        public ReactiveCommand CloseCommand { get; }

        /// <inheritdoc />
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters != null && parameters.TryGetValue<MessageDialogSettings>(nameof(MessageDialogSettings), out var result))
            {
                this.model.LoadSettings(result);
            }
        }

        /// <summary>
        /// Close dialog
        /// </summary>
        private void Close()
        {
            this.RaiseRequestClose(new DialogResult(ButtonResult.OK));
        }
    }
}
