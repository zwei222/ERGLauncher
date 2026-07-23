using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERGLauncher.Core;
using ERGLauncher.Models;
using ERGLauncher.Services;

namespace ERGLauncher.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IMainModel model;
    private readonly IViewDialogService dialogService;

    public MainViewModel(IMainModel model, IViewDialogService dialogService)
        : base(model)
    {
        this.model = model ?? throw new ArgumentNullException(nameof(model));
        this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        title = model.Title;
        isEnabledBack = model.IsEnabledBack;
        isEnabledForward = model.IsEnabledForward;
        currentBrand = model.CurrentBrand;
        selectedItem = model.SelectedItem;
        currentItem = model.CurrentItem;
        Items = model.Items;
        model.PropertyChanged += OnModelPropertyChanged;

        BackCommand = new RelayCommand(model.Back, CanGoBack);
        ForwardCommand = new RelayCommand(model.Forward, CanGoForward);
        SelectItemAsyncCommand = new AsyncRelayCommand(SelectItemAsync, () => !IsBusy);
        AddItemAsyncCommand = new AsyncRelayCommand(AddItemAsync, () => !IsBusy);
        EditItemAsyncCommand = new AsyncRelayCommand(EditItemAsync, CanEditOrRemove);
        RemoveItemAsyncCommand = new AsyncRelayCommand(RemoveItemAsync, CanEditOrRemove);
        OpenSettingCommand = new AsyncRelayCommand(OpenSettingAsync, () => !IsBusy);
        LoadSettingAsyncCommand = new AsyncRelayCommand(LoadSettingAsync, () => !IsBusy);
        SaveAppSettingAsyncCommand = new AsyncRelayCommand(SaveAppSettingAsync, () => !IsBusy);
    }

    [ObservableProperty]
    private string? title;

    [ObservableProperty]
    private bool isEnabledBack;

    [ObservableProperty]
    private bool isEnabledForward;

    [ObservableProperty]
    private string? currentBrand;

    [ObservableProperty]
    private Item? selectedItem;

    [ObservableProperty]
    private Item? currentItem;

    public ObservableCollection<Item> Items { get; }

    public IRelayCommand BackCommand { get; }

    public IRelayCommand ForwardCommand { get; }

    public IAsyncRelayCommand SelectItemAsyncCommand { get; }

    public IAsyncRelayCommand AddItemAsyncCommand { get; }

    public IAsyncRelayCommand EditItemAsyncCommand { get; }

    public IAsyncRelayCommand RemoveItemAsyncCommand { get; }

    public IAsyncRelayCommand OpenSettingCommand { get; }

    public IAsyncRelayCommand LoadSettingAsyncCommand { get; }

    public IAsyncRelayCommand SaveAppSettingAsyncCommand { get; }

    partial void OnSelectedItemChanged(Item? value)
    {
        model.SelectedItem = value;
        EditItemAsyncCommand.NotifyCanExecuteChanged();
        RemoveItemAsyncCommand.NotifyCanExecuteChanged();
    }

    protected override void OnBusyStateChanged() => NotifyCommandStates();

    protected override void DisposeManaged()
    {
        model.PropertyChanged -= OnModelPropertyChanged;
        base.DisposeManaged();
    }

    private bool CanGoBack() => !IsBusy && IsEnabledBack;

    private bool CanGoForward() => !IsBusy && IsEnabledForward;

    private bool CanEditOrRemove() => !IsBusy && SelectedItem is not null;

    private async Task SelectItemAsync()
    {
        using var busy = BeginBusy();
        await model.SelectItemAsync().ConfigureAwait(true);
    }

    private async Task AddItemAsync()
    {
        var dialogName = CurrentItem switch
        {
            RootItem => "AddBrand",
            Brand => "AddProduct",
            _ => null,
        };

        if (dialogName is null)
        {
            return;
        }

        using var busy = BeginBusy();
        var result = await dialogService.ShowDialogAsync(dialogName).ConfigureAwait(true);
        if (!result.Accepted)
        {
            return;
        }

        switch (result.Value)
        {
            case Brand brand:
                await model.AddItemAsync(brand.Name, brand.IconPath, string.Empty).ConfigureAwait(true);
                break;
            case Product product:
                await model.AddItemAsync(product.Name, product.IconPath, product.Path).ConfigureAwait(true);
                break;
        }
    }

    private async Task EditItemAsync()
    {
        var dialogName = CurrentItem switch
        {
            RootItem => "AddBrand",
            Brand => "AddProduct",
            _ => null,
        };

        if (dialogName is null || SelectedItem is null)
        {
            return;
        }

        using var busy = BeginBusy();
        var result = await dialogService.ShowDialogAsync(dialogName, SelectedItem).ConfigureAwait(true);
        if (!result.Accepted)
        {
            return;
        }

        switch (result.Value)
        {
            case Brand brand:
                await model.EditItemAsync(brand.Name, brand.IconPath, string.Empty).ConfigureAwait(true);
                break;
            case Product product:
                await model.EditItemAsync(product.Name, product.IconPath, product.Path).ConfigureAwait(true);
                break;
        }
    }

    private async Task RemoveItemAsync()
    {
        using var busy = BeginBusy();
        if (await model.RemoveItemAsync().ConfigureAwait(true))
        {
            await model.SaveSettingAsync().ConfigureAwait(true);
        }
    }

    private async Task OpenSettingAsync()
    {
        using var busy = BeginBusy();
        await dialogService.ShowDialogAsync("Setting").ConfigureAwait(true);
    }

    private async Task LoadSettingAsync()
    {
        using var busy = BeginBusy();
        await model.LoadAppSettingAsync().ConfigureAwait(true);
        await model.LoadSettingAsync().ConfigureAwait(true);
    }

    private async Task SaveAppSettingAsync()
    {
        using var busy = BeginBusy();
        await model.SaveAppSettingAsync().ConfigureAwait(true);
    }

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IMainModel.Title): Title = model.Title; break;
            case nameof(IMainModel.IsEnabledBack): IsEnabledBack = model.IsEnabledBack; BackCommand.NotifyCanExecuteChanged(); break;
            case nameof(IMainModel.IsEnabledForward): IsEnabledForward = model.IsEnabledForward; ForwardCommand.NotifyCanExecuteChanged(); break;
            case nameof(IMainModel.CurrentBrand): CurrentBrand = model.CurrentBrand; break;
            case nameof(IMainModel.SelectedItem): SelectedItem = model.SelectedItem; break;
            case nameof(IMainModel.CurrentItem): CurrentItem = model.CurrentItem; break;
            case null:
            case "": Refresh(); break;
        }
    }

    private void Refresh()
    {
        Title = model.Title;
        IsEnabledBack = model.IsEnabledBack;
        IsEnabledForward = model.IsEnabledForward;
        CurrentBrand = model.CurrentBrand;
        SelectedItem = model.SelectedItem;
        CurrentItem = model.CurrentItem;
        NotifyCommandStates();
    }

    private void NotifyCommandStates()
    {
        BackCommand.NotifyCanExecuteChanged();
        ForwardCommand.NotifyCanExecuteChanged();
        SelectItemAsyncCommand.NotifyCanExecuteChanged();
        AddItemAsyncCommand.NotifyCanExecuteChanged();
        EditItemAsyncCommand.NotifyCanExecuteChanged();
        RemoveItemAsyncCommand.NotifyCanExecuteChanged();
        OpenSettingCommand.NotifyCanExecuteChanged();
        LoadSettingAsyncCommand.NotifyCanExecuteChanged();
        SaveAppSettingAsyncCommand.NotifyCanExecuteChanged();
    }
}
