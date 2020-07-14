using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ERGLauncher.Core;
using ERGLauncher.Core.DialogSettings.Implementations;
using ERGLauncher.Models;
using ERGLauncher.Properties;
using ERGLauncher.Services;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace ERGLauncher.ViewModels
{
    /// <summary>
    /// Add brand ViewModel.
    /// </summary>
    public class AddBrandViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Add brand model.
        /// </summary>
        private readonly IAddBrandModel model;

        /// <summary>
        /// Common dialog service.
        /// </summary>
        private readonly ICommonDialogService commonDialogService;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="commonDialogService">Common dialog service</param>
        public AddBrandViewModel(IAddBrandModel model, ICommonDialogService commonDialogService)
            : base(model)
        {
            this.model = model;
            this.commonDialogService = commonDialogService;

            // properties
            this.Name = this.model.ToReactivePropertyAsSynchronized(myModel => myModel.Name)
                .AddTo(this.Disposable);
            this.IconPath = this.model.ToReactivePropertyAsSynchronized(myModel => myModel.IconPath)
                .AddTo(this.Disposable);
            this.Icon = this.model.ObserveProperty(myModel => myModel.Icon).ToReadOnlyReactivePropertySlim()
                .AddTo(this.Disposable);

            // commands
            this.SelectIconAsyncCommand = new[]
            {
                this.IsBusy.Select(isBusy => !isBusy),
            }.CombineLatest(combined => combined.All(condition => condition)).ToAsyncReactiveCommand()
            .AddTo(this.Disposable);
            this.SelectIconAsyncCommand.Subscribe(this.SelectIconAsync);
            this.AddBrandAsyncCommand = new[]
            {
                this.IsBusy.Select(isBusy => !isBusy),
                this.Name.Select(name => !string.IsNullOrWhiteSpace(name)),
            }.CombineLatest(combined => combined.All(condition => condition)).ToAsyncReactiveCommand()
            .AddTo(this.Disposable);
            this.AddBrandAsyncCommand.Subscribe(this.AddBrandAsync);
            this.CancelCommand = new[]
            {
                this.IsBusy.Select(isBusy => !isBusy),
            }.CombineLatest(combined => combined.All(condition => condition)).ToReactiveCommand()
            .AddTo(this.Disposable);
            this.CancelCommand.Subscribe(this.Cancel);
        }

        /// <summary>
        /// Brand name.
        /// </summary>
        public ReactiveProperty<string> Name { get; }

        /// <summary>
        /// Icon file path.
        /// </summary>
        public ReactiveProperty<string?> IconPath { get; }

        /// <summary>
        /// Icon.
        /// </summary>
        public ReadOnlyReactivePropertySlim<BitmapImage?> Icon { get; }

        /// <summary>
        /// Select icon asynchronous command.
        /// </summary>
        public AsyncReactiveCommand SelectIconAsyncCommand { get; }

        /// <summary>
        /// Add brand asynchronous command.
        /// </summary>
        public AsyncReactiveCommand AddBrandAsyncCommand { get; }

        /// <summary>
        /// Cancel command.
        /// </summary>
        public ReactiveCommand CancelCommand { get; }

        /// <inheritdoc />
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters != null && parameters.TryGetValue<Brand>(nameof(Brand), out var result))
            {
                this.model.LoadBrand(result);
            }
        }

        /// <summary>
        /// Select icon asynchronous.
        /// </summary>
        /// <returns>Task</returns>
        private async Task SelectIconAsync()
        {
            using var process = this.BusyNotifier.ProcessStart();
            var settings = new OpenFileDialogSettings
            {
                Filter = this.model.GetCultureString(nameof(Resources.ImageFiles)),
                CanMultiSelect = false,
                Title = this.model.GetCultureString(nameof(Resources.OpenIconFile)),
            };

            if (!this.commonDialogService.ShowDialog(settings))
            {
                return;
            }

            await this.model.SelectIconAsync(settings.FileName).ConfigureAwait(false);
        }

        /// <summary>
        /// Add brand asynchronous.
        /// </summary>
        /// <returns>Task</returns>
        private async Task AddBrandAsync()
        {
            var brand = await this.model.AddBrandAsync().ConfigureAwait(true);
            var parameter = new DialogParameters
            {
                { nameof(Brand), brand },
            };

            this.RaiseRequestClose(new DialogResult(ButtonResult.OK, parameter));
        }

        /// <summary>
        /// Cancel.
        /// </summary>
        private void Cancel()
        {
            this.RaiseRequestClose(new DialogResult(ButtonResult.Cancel));
        }
    }
}
