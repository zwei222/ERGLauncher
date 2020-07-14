using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ERGLauncher.Core;
using ERGLauncher.Models;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace ERGLauncher.ViewModels
{
    /// <summary>
    /// Setting ViewModel.
    /// </summary>
    public class SettingViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Setting model.
        /// </summary>
        private readonly ISettingModel model;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="model"></param>
        public SettingViewModel(ISettingModel model)
            : base(model)
        {
            this.model = model;

            // properties
            this.SelectedLanguage = this.model.ToReactivePropertyAsSynchronized(myModel => myModel.SelectedLanguage)
                .AddTo(this.Disposable);
            this.SelectedTheme = this.model.ToReactivePropertyAsSynchronized(myModel => myModel.SelectedTheme)
                .AddTo(this.Disposable);

            // commands
            this.OkAsyncCommand = new[]
            {
                this.IsBusy.Select(isBusy => !isBusy),
            }.CombineLatest(combined => combined.All(condition => condition)).ToAsyncReactiveCommand()
            .WithSubscribe(this.OkAsync).AddTo(this.Disposable);
            this.ApplyAsyncCommand = new[]
            {
                this.IsBusy.Select(isBusy => !isBusy),
            }.CombineLatest(combined => combined.All(condition => condition)).ToAsyncReactiveCommand()
            .WithSubscribe(this.ApplyAsync).AddTo(this.Disposable);
        }

        /// <summary>
        /// Selected culture.
        /// </summary>
        public ReactiveProperty<CultureInfo?> SelectedLanguage { get; }

        /// <summary>
        /// Selected theme.
        /// </summary>
        public ReactiveProperty<Theme> SelectedTheme { get; }

        /// <summary>
        /// OK asynchronous command.
        /// </summary>
        public AsyncReactiveCommand OkAsyncCommand { get; }

        /// <summary>
        /// Apply asynchronous command.
        /// </summary>
        public AsyncReactiveCommand ApplyAsyncCommand { get; }

        /// <summary>
        /// The process when the OK button is pressed is executed asynchronously.
        /// </summary>
        /// <returns>Task</returns>
        private async Task OkAsync()
        {
            await this.ApplyAsync().ConfigureAwait(false);
            this.RaiseRequestClose(new DialogResult(ButtonResult.OK));
        }

        /// <summary>
        /// The process when the Apply button is pressed is executed asynchronously.
        /// </summary>
        /// <returns></returns>
        private async Task ApplyAsync()
        {
            await this.model.ApplyAsync().ConfigureAwait(false);
        }
    }
}
