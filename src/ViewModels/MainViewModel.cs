using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ERGLauncher.Core;
using ERGLauncher.Models;
using ERGLauncher.Views;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace ERGLauncher.ViewModels
{
    /// <summary>
    /// Main ViewModel.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Main model.
        /// </summary>
        private readonly IMainModel model;

        /// <summary>
        /// Dialog service.
        /// </summary>
        private readonly IDialogService dialogService;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="dialogService">Dialog service</param>
        public MainViewModel(IMainModel model, IDialogService dialogService)
            : base(model)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            this.dialogService = dialogService;

            // properties
            this.Title = this.model.ObserveProperty(myModel => myModel.Title).ToReadOnlyReactivePropertySlim()
                .AddTo(this.Disposable);
            this.IsEnabledBack = this.model.ObserveProperty(myModel => myModel.IsEnabledBack)
                .ToReadOnlyReactivePropertySlim().AddTo(this.Disposable);
            this.IsEnabledForward = this.model.ObserveProperty(myModel => myModel.IsEnabledForward)
                .ToReadOnlyReactivePropertySlim().AddTo(this.Disposable);
            this.CurrentBrand = this.model.ObserveProperty(myModel => myModel.CurrentBrand)
                .ToReadOnlyReactivePropertySlim().AddTo(this.Disposable);
            this.Items = this.model.Items.ToReadOnlyReactiveCollection().AddTo(this.Disposable);
            this.SelectedItem = this.model.ToReactivePropertyAsSynchronized(myModel => myModel.SelectedItem)
                .AddTo(this.Disposable);
            this.CurrentItem = this.model.ObserveProperty(myModel => myModel.CurrentItem)
                .ToReadOnlyReactivePropertySlim().AddTo(this.Disposable);

            // commands
            this.BackCommand = new[]
            {
                this.IsBusy.Select(isBusy => !isBusy),
                this.IsEnabledBack.Select(isEnabledBack => isEnabledBack),
            }
            .CombineLatest(combined => combined.All(condition => condition))
            .ToReactiveCommand()
            .AddTo(this.Disposable);
            this.BackCommand.Subscribe(this.Back);
            this.ForwardCommand = new[]
            {
                this.IsBusy.Select(isBusy => !isBusy),
                this.IsEnabledForward.Select(isEnabledForward => isEnabledForward),
            }
            .CombineLatest(combined => combined.All(condition => condition))
            .ToReactiveCommand()
            .AddTo(this.Disposable);
            this.ForwardCommand.Subscribe(this.Forward);
            this.SelectItemAsyncCommand = new[]
            {
                this.IsBusy.Select(isBusy => !isBusy),
            }
            .CombineLatest(combined => combined.All(condition => condition))
            .ToAsyncReactiveCommand()
            .AddTo(this.Disposable);
            this.SelectItemAsyncCommand.Subscribe(this.SelectItemAsync);
            this.AddItemAsyncCommand = new[]
            {
                this.IsBusy.Select(isBusy => !isBusy),
            }
            .CombineLatest(combined => combined.All(condition => condition))
            .ToAsyncReactiveCommand()
            .AddTo(this.Disposable);
            this.AddItemAsyncCommand.Subscribe(this.AddItemAsync);
            this.EditItemAsyncCommand = new[]
            {
                this.IsBusy.Select(isBusy => !isBusy),
                this.SelectedItem.Select(selectedItem => selectedItem != null),
            }
            .CombineLatest(combined => combined.All(condition => condition))
            .ToAsyncReactiveCommand()
            .AddTo(this.Disposable);
            this.EditItemAsyncCommand.Subscribe(this.EditItemAsync);
            this.RemoveItemAsyncCommand = new[]
            {
                this.IsBusy.Select(isBusy => !isBusy),
                this.SelectedItem.Select(selectedItem => selectedItem != null),
            }
            .CombineLatest(combined => combined.All(condition => condition))
            .ToAsyncReactiveCommand()
            .AddTo(this.Disposable);
            this.RemoveItemAsyncCommand.Subscribe(this.RemoveItemAsync);
            this.OpenSettingCommand = new[]
            {
                this.IsBusy.Select(isBusy => !isBusy),
            }
            .CombineLatest(combined => combined.All(condition => condition)).ToReactiveCommand()
            .AddTo(this.Disposable);
            this.OpenSettingCommand.Subscribe(this.OpenSetting);
            this.LoadSettingAsyncCommand = new[]
            {
                this.IsBusy.Select(isBusy => !isBusy),
            }
            .CombineLatest(combined => combined.All(condition => condition))
            .ToAsyncReactiveCommand()
            .AddTo(this.Disposable);
            this.LoadSettingAsyncCommand.Subscribe(this.LoadSettingAsync);
            this.SaveAppSettingAsyncCommand = this.IsBusy.Select(isBusy => !isBusy).ToAsyncReactiveCommand()
                .WithSubscribe(this.SaveAppSettingAsync).AddTo(this.Disposable);
        }

        /// <summary>
        /// Title.
        /// </summary>
        public ReadOnlyReactivePropertySlim<string?> Title { get; }

        /// <summary>
        /// <see langword="true" /> if you can go back; otherwise <see langword="false" />.
        /// </summary>
        public ReadOnlyReactivePropertySlim<bool> IsEnabledBack { get; }

        /// <summary>
        /// <see langword="true" /> if you can go forward; otherwise <see langword="false" />.
        /// </summary>
        public ReadOnlyReactivePropertySlim<bool> IsEnabledForward { get; }

        /// <summary>
        /// Current brand name.
        /// </summary>
        public ReadOnlyReactivePropertySlim<string?> CurrentBrand { get; }

        /// <summary>
        /// Display items.
        /// </summary>
        public ReadOnlyReactiveCollection<Item> Items { get; }

        /// <summary>
        /// Selected item.
        /// </summary>
        public ReactiveProperty<Item?> SelectedItem { get; }

        /// <summary>
        /// Current item.
        /// </summary>
        public ReadOnlyReactivePropertySlim<Item?> CurrentItem { get; }

        /// <summary>
        /// Back command.
        /// </summary>
        public ReactiveCommand BackCommand { get; }

        /// <summary>
        /// Forward command.
        /// </summary>
        public ReactiveCommand ForwardCommand { get; }

        /// <summary>
        /// Selected item asynchronous command.
        /// </summary>
        public AsyncReactiveCommand SelectItemAsyncCommand { get; }

        /// <summary>
        /// Add item asynchronous command.
        /// </summary>
        public AsyncReactiveCommand AddItemAsyncCommand { get; }

        /// <summary>
        /// Edit item asynchronous command.
        /// </summary>
        public AsyncReactiveCommand EditItemAsyncCommand { get; }

        /// <summary>
        /// Remove item asynchronous command.
        /// </summary>
        public AsyncReactiveCommand RemoveItemAsyncCommand { get; }

        /// <summary>
        /// Open setting command.
        /// </summary>
        public ReactiveCommand OpenSettingCommand { get; }

        /// <summary>
        /// Load settings asynchronous command.
        /// </summary>
        public AsyncReactiveCommand LoadSettingAsyncCommand { get; }

        /// <summary>
        /// Save application settings asynchronous command.
        /// </summary>
        public AsyncReactiveCommand SaveAppSettingAsyncCommand { get; }

        /// <summary>
        /// Go back history.
        /// </summary>
        private void Back()
        {
            this.model.Back();
        }

        /// <summary>
        /// Go forward history.
        /// </summary>
        private void Forward()
        {
            this.model.Forward();
        }

        /// <summary>
        /// Select item asynchronous.
        /// </summary>
        /// <returns>Task</returns>
        private async Task SelectItemAsync()
        {
            await this.model.SelectItemAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Add item asynchronous.
        /// </summary>
        /// <returns>Task</returns>
        private async Task AddItemAsync()
        {
            var isCanceled = true;
            string name = string.Empty;
            string? iconPath = null;
            string path = string.Empty;

            switch (this.CurrentItem.Value)
            {
                case RootItem _:
                    this.dialogService.ShowDialog(nameof(AddBrandView), new DialogParameters(), result =>
                    {
                        var dialogResult = result;

                        if (dialogResult.Result != ButtonResult.OK)
                        {
                            return;
                        }

                        var newBrand = dialogResult.Parameters.GetValue<Brand>(nameof(Brand));

                        name = newBrand.Name;
                        iconPath = newBrand.IconPath;
                        isCanceled = false;
                    });

                    break;
                case Brand _:
                    this.dialogService.ShowDialog(nameof(AddProductView), new DialogParameters(), result =>
                    {
                        var dialogResult = result;

                        if (dialogResult.Result != ButtonResult.OK)
                        {
                            return;
                        }

                        var newProduct = dialogResult.Parameters.GetValue<Product>(nameof(Product));

                        name = newProduct.Name;
                        iconPath = newProduct.IconPath;
                        path = newProduct.Path;
                        isCanceled = false;
                    });

                    break;
                default:
                    return;
            }

            if (isCanceled)
            {
                return;
            }

            await this.model.AddItemAsync(name, iconPath, path).ConfigureAwait(false);
        }

        private async Task EditItemAsync()
        {
            var isCanceled = true;
            string name = string.Empty;
            string? iconPath = null;
            string path = string.Empty;

            switch (this.CurrentItem.Value)
            {
                case RootItem _:
                    this.dialogService.ShowDialog(nameof(AddBrandView), new DialogParameters { { nameof(Brand), this.SelectedItem.Value } }, result =>
                    {
                        var dialogResult = result;

                        if (dialogResult.Result != ButtonResult.OK)
                        {
                            return;
                        }

                        var newBrand = dialogResult.Parameters.GetValue<Brand>(nameof(Brand));

                        name = newBrand.Name;
                        iconPath = newBrand.IconPath;
                        isCanceled = false;
                    });

                    break;
                case Brand _:
                    this.dialogService.ShowDialog(nameof(AddProductView), new DialogParameters { { nameof(Product), this.SelectedItem.Value } }, result =>
                    {
                        var dialogResult = result;

                        if (dialogResult.Result != ButtonResult.OK)
                        {
                            return;
                        }

                        var newProduct = dialogResult.Parameters.GetValue<Product>(nameof(Product));

                        name = newProduct.Name;
                        iconPath = newProduct.IconPath;
                        path = newProduct.Path;
                        isCanceled = true;
                    });

                    break;
                default:
                    return;
            }

            if (isCanceled)
            {
                return;
            }

            await this.model.EditItemAsync(name, iconPath, path).ConfigureAwait(false);
        }

        private async Task RemoveItemAsync()
        {
            if (!await this.model.RemoveItemAsync().ConfigureAwait(true))
            {
                return;
            }

            using var process = this.BusyNotifier.ProcessStart();

            await this.model.SaveSettingAsync().ConfigureAwait(false);
        }

        private void OpenSetting()
        {
            this.dialogService.ShowDialog(nameof(SettingView), new DialogParameters(), _ => { });
        }

        private async Task LoadSettingAsync()
        {
            using var process = this.BusyNotifier.ProcessStart();

            await this.model.LoadAppSettingAsync().ConfigureAwait(true);
            await this.model.LoadSettingAsync().ConfigureAwait(false);
        }

        private async Task SaveAppSettingAsync()
        {
            using var process = this.BusyNotifier.ProcessStart();

            await this.model.SaveAppSettingAsync().ConfigureAwait(false);
        }
    }
}
