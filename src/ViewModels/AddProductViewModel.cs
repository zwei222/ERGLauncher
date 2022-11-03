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
    /// Add product ViewModel.
    /// </summary>
    public class AddProductViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Add product model.
        /// </summary>
        private readonly IAddProductModel model;

        /// <summary>
        /// Common dialog service.
        /// </summary>
        private readonly ICommonDialogService commonDialogService;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="commonDialogService">Common dialog service</param>
        public AddProductViewModel(IAddProductModel model, ICommonDialogService commonDialogService)
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
            this.Path = this.model.ToReactivePropertyAsSynchronized(myModel => myModel.Path)
                .AddTo(this.Disposable);

            // commands
            this.SelectIconAsyncCommand = new[]
            {
                this.IsBusy.Select(isBusy => !isBusy),
            }.CombineLatest(combined => combined.All(condition => condition)).ToAsyncReactiveCommand()
            .AddTo(this.Disposable);
            this.SelectIconAsyncCommand.Subscribe(this.SelectIconAsync);
            this.SelectFileAsyncCommand = new[]
            {
                this.IsBusy.Select(isBusy => !isBusy),
            }.CombineLatest(combined => combined.All(condition => condition)).ToAsyncReactiveCommand()
            .AddTo(this.Disposable);
            this.SelectFileAsyncCommand.Subscribe(this.SelectFileAsync);
            this.AddProductAsyncCommand = new[]
            {
                this.IsBusy.Select(isBusy => !isBusy),
                this.Name.Select(name => !string.IsNullOrWhiteSpace(name)),
                this.Path.Select(path => !string.IsNullOrWhiteSpace(path)),
            }.CombineLatest(combined => combined.All(condition => condition)).ToAsyncReactiveCommand()
            .AddTo(this.Disposable);
            this.AddProductAsyncCommand.Subscribe(this.AddProductAsync);
            this.CancelCommand = new[]
            {
                this.IsBusy.Select(isBusy => !isBusy),
            }.CombineLatest(combined => combined.All(condition => condition)).ToReactiveCommand()
            .AddTo(this.Disposable);
            this.CancelCommand.Subscribe(this.Cancel);
        }

        /// <summary>
        /// Product name.
        /// </summary>
        public ReactiveProperty<string> Name { get; }

        /// <summary>
        /// Icon file path.
        /// </summary>
        public ReactiveProperty<string?> IconPath { get; }

        /// <summary>
        /// Icon
        /// </summary>
        public ReadOnlyReactivePropertySlim<BitmapImage?> Icon { get; }

        /// <summary>
        /// Executable file path.
        /// </summary>
        public ReactiveProperty<string> Path { get; }

        /// <summary>
        /// Select icon asynchronous command.
        /// </summary>
        public AsyncReactiveCommand SelectIconAsyncCommand { get; }

        /// <summary>
        /// Select executable file asynchronous command.
        /// </summary>
        public AsyncReactiveCommand SelectFileAsyncCommand { get; }

        /// <summary>
        /// Add product asynchronous command.
        /// </summary>
        public AsyncReactiveCommand AddProductAsyncCommand { get; }

        /// <summary>
        /// Cancel command.
        /// </summary>
        public ReactiveCommand CancelCommand { get; }

        /// <inheritdoc />
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters != null && parameters.TryGetValue<Product>(nameof(Product), out var result))
            {
                this.model.LoadProduct(result);
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
        /// Select executable file asynchronous.
        /// </summary>
        /// <returns>Task</returns>
        private async Task SelectFileAsync()
        {
            using var process = this.BusyNotifier.ProcessStart();
            var settings = new OpenFileDialogSettings
            {
                Filter = this.model.GetCultureString(nameof(Resources.ExecutableFiles)),
                CanMultiSelect = false,
                Title = this.model.GetCultureString(nameof(Resources.OpenExecutableFile)),
            };

            if (!this.commonDialogService.ShowDialog(settings))
            {
                return;
            }

            await this.model.SelectFileAsync(settings.FileName).ConfigureAwait(true);
        }

        /// <summary>
        /// Add product asynchronous.
        /// </summary>
        /// <returns>Task</returns>
        private async Task AddProductAsync()
        {
            var brand = await this.model.AddProductAsync().ConfigureAwait(true);
            var parameter = new DialogParameters
            {
                { nameof(Product), brand },
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
