extern alias MigratedCore;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERGLauncher.Core;
using ERGLauncher.Services;
using ERGLauncher.Views;
using CoreBrand = MigratedCore::ERGLauncher.Core.Models.Brand;
using CoreProduct = MigratedCore::ERGLauncher.Core.Models.Product;
using CoreRootItem = MigratedCore::ERGLauncher.Core.Models.RootItem;
using IAppSettingService = MigratedCore::ERGLauncher.Core.Services.IAppSettingService;
using ICoreDialogService = MigratedCore::ERGLauncher.Core.Services.IDialogService;
using IFileService = MigratedCore::ERGLauncher.Core.Services.IFileService;
using IGameSettingService = MigratedCore::ERGLauncher.Core.Services.IGameSettingService;
using IResourceService = MigratedCore::ERGLauncher.Core.Services.IResourceService;
using IThemeService = MigratedCore::ERGLauncher.Core.Services.IThemeService;
using CoreAppSettings = MigratedCore::ERGLauncher.Core.Models.AppSettings;

namespace ERGLauncher.ViewModels;

/// <summary>
/// Presentation model backing <see cref="MainView"/>. Restores the command surface
/// (Back/Forward/Add/Edit/Remove/Setting/Load/Save) that was lost when the Avalonia
/// ViewModel layer was deleted, and wires it to the migrated core services.
/// </summary>
public sealed partial class MainViewModel : ViewModelBase, IMainViewDataContext
{
    private readonly IFileService fileService;
    private readonly IAppSettingService appSettingService;
    private readonly IGameSettingService gameSettingService;
    private readonly IResourceService resourceService;
    private readonly IThemeService themeService;
    private readonly ICoreDialogService dialogService;
    private readonly IViewDialogService viewDialogService;

    private readonly List<Item> history = [];
    private readonly Dictionary<Item, Item?> pageSelections = [];
    private int historyIndex = -1;
    private CoreRootItem coreRoot = new((ICollection<CoreBrand>?)null);

    [ObservableProperty]
    private string? title = "ERG Launcher";

    [ObservableProperty]
    private bool isEnabledBack;

    [ObservableProperty]
    private bool isEnabledForward;

    [ObservableProperty]
    private string? currentBrand;

    [ObservableProperty]
    private Item? selectedItem;

    private Item? currentItem;

    public MainViewModel(
        IFileService fileService,
        IAppSettingService appSettingService,
        IGameSettingService gameSettingService,
        IResourceService resourceService,
        IThemeService themeService,
        ICoreDialogService dialogService,
        IViewDialogService viewDialogService)
    {
        this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        this.appSettingService = appSettingService ?? throw new ArgumentNullException(nameof(appSettingService));
        this.gameSettingService = gameSettingService ?? throw new ArgumentNullException(nameof(gameSettingService));
        this.resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
        this.themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        this.viewDialogService = viewDialogService ?? throw new ArgumentNullException(nameof(viewDialogService));

        BackCommand = new RelayCommand(GoBack, () => !IsBusy && IsEnabledBack);
        ForwardCommand = new RelayCommand(GoForward, () => !IsBusy && IsEnabledForward);
        SelectItemAsyncCommand = new AsyncRelayCommand<Item>(SelectItemAsync, _ => !IsBusy);
        AddItemAsyncCommand = new AsyncRelayCommand(AddItemAsync, () => !IsBusy);
        EditItemAsyncCommand = new AsyncRelayCommand<Item>(EditItemAsync, CanEditOrRemove);
        RemoveItemAsyncCommand = new AsyncRelayCommand<Item>(RemoveItemAsync, CanEditOrRemove);
        OpenSettingCommand = new AsyncRelayCommand(OpenSettingAsync, () => !IsBusy);
        LoadSettingAsyncCommand = new AsyncRelayCommand(LoadSettingAsync, () => !IsBusy);
        SaveAppSettingAsyncCommand = new AsyncRelayCommand(SaveAppSettingAsync, () => !IsBusy);
    }

    public ObservableCollection<Item> Items { get; } = [];

    public IRelayCommand BackCommand { get; }

    public IRelayCommand ForwardCommand { get; }

    public IAsyncRelayCommand<Item> SelectItemAsyncCommand { get; }

    public IAsyncRelayCommand AddItemAsyncCommand { get; }

    public IAsyncRelayCommand<Item> EditItemAsyncCommand { get; }

    public IAsyncRelayCommand<Item> RemoveItemAsyncCommand { get; }

    public IAsyncRelayCommand OpenSettingCommand { get; }

    public IAsyncRelayCommand LoadSettingAsyncCommand { get; }

    public IAsyncRelayCommand SaveAppSettingAsyncCommand { get; }

    ICommand IMainViewDataContext.BackCommand => BackCommand;

    ICommand IMainViewDataContext.ForwardCommand => ForwardCommand;

    ICommand IMainViewDataContext.SelectItemAsyncCommand => SelectItemAsyncCommand;

    ICommand IMainViewDataContext.AddItemAsyncCommand => AddItemAsyncCommand;

    ICommand IMainViewDataContext.EditItemAsyncCommand => EditItemAsyncCommand;

    ICommand IMainViewDataContext.RemoveItemAsyncCommand => RemoveItemAsyncCommand;

    ICommand IMainViewDataContext.OpenSettingCommand => OpenSettingCommand;

    ICommand IMainViewDataContext.LoadSettingAsyncCommand => LoadSettingAsyncCommand;

    ICommand IMainViewDataContext.SaveAppSettingAsyncCommand => SaveAppSettingAsyncCommand;

    private Item? CurrentItem => currentItem;

    protected override void OnBusyStateChanged() => NotifyCommandStates();

    private bool CanEditOrRemove(Item? item) => !IsBusy && item is not null;

    private void GoBack()
    {
        if (historyIndex <= 0)
        {
            return;
        }

        SaveCurrentPageSelection();
        historyIndex--;
        ShowChildren(history[historyIndex], RestorePageSelection(history[historyIndex]));
        UpdateNavigationState();
    }

    private void GoForward()
    {
        if (historyIndex < 0 || historyIndex >= history.Count - 1)
        {
            return;
        }

        SaveCurrentPageSelection();
        historyIndex++;
        ShowChildren(history[historyIndex], RestorePageSelection(history[historyIndex]));
        UpdateNavigationState();
    }

    private async Task SelectItemAsync(Item? item)
    {
        using var busy = BeginBusy();
        switch (item)
        {
            case Brand brand:
                SaveCurrentPageSelection();
                PushHistory(brand);
                ShowChildren(brand);
                UpdateNavigationState();
                break;
            case Product product:
                if (await dialogService.ShowConfirmationAsync(
                        product.Name,
                        FormatMessage("DoYouWantToLaunch", product.Name)).ConfigureAwait(true))
                {
                    await fileService.ExecuteAsync(product.Path).ConfigureAwait(true);
                }

                break;
        }
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
        var result = await viewDialogService.ShowDialogAsync(dialogName).ConfigureAwait(true);
        if (!result.Accepted)
        {
            return;
        }

        switch (result.Value)
        {
            case Brand brand:
                await AddCoreBrandAsync(brand).ConfigureAwait(true);
                break;
            case Product product:
                await AddCoreProductAsync(product).ConfigureAwait(true);
                break;
        }
    }

    private async Task EditItemAsync(Item? item)
    {
        var dialogName = CurrentItem switch
        {
            RootItem => "AddBrand",
            Brand => "AddProduct",
            _ => null,
        };

        if (dialogName is null || item is null)
        {
            return;
        }

        using var busy = BeginBusy();
        var result = await viewDialogService.ShowDialogAsync(dialogName, item).ConfigureAwait(true);
        if (!result.Accepted)
        {
            return;
        }

        switch (result.Value)
        {
            case Brand brand:
                await EditCoreItemAsync(item, brand.Name, brand.IconPath, string.Empty).ConfigureAwait(true);
                break;
            case Product product:
                await EditCoreItemAsync(item, product.Name, product.IconPath, product.Path).ConfigureAwait(true);
                break;
        }
    }

    private async Task RemoveItemAsync(Item? item)
    {
        if (item is null)
        {
            return;
        }

        using var busy = BeginBusy();
        if (!await dialogService.ShowConfirmationAsync(
                item.Name,
                FormatMessage("DoYouWantToRemove", item.Name)).ConfigureAwait(true))
        {
            return;
        }

        RemoveFromCore(item);
        await gameSettingService.SaveSettingAsync(coreRoot).ConfigureAwait(true);
        await RefreshCurrentViewAsync().ConfigureAwait(true);
    }

    private async Task OpenSettingAsync()
    {
        using var busy = BeginBusy();
        await viewDialogService.ShowDialogAsync("Setting").ConfigureAwait(true);
    }

    private async Task LoadSettingAsync()
    {
        using var busy = BeginBusy();
        var appSettings = await appSettingService.LoadAppSettingAsync().ConfigureAwait(true);
        if (appSettings is not null)
        {
            resourceService.ChangeCulture(appSettings.Culture);
            await themeService.ChangeThemeAsync(appSettings.Theme).ConfigureAwait(true);
        }

        coreRoot = await gameSettingService.LoadSettingAsync().ConfigureAwait(true);
        var viewRoot = await ItemConversion.ToViewRootAsync(coreRoot, fileService).ConfigureAwait(true);
        history.Clear();
        pageSelections.Clear();
        history.Add(viewRoot);
        historyIndex = 0;
        ShowChildren(viewRoot);
        UpdateNavigationState();
    }

    private async Task SaveAppSettingAsync()
    {
        using var busy = BeginBusy();
        await appSettingService.SaveAppSettingAsync(new CoreAppSettings
        {
            Culture = resourceService.CurrentCulture,
            Theme = themeService.CurrentTheme,
        }).ConfigureAwait(true);
    }

    private async Task AddCoreBrandAsync(Brand brand)
    {
        var coreBrand = await gameSettingService.CreateBrandItemAsync(brand.Name, brand.IconPath).ConfigureAwait(true);
        if (coreRoot.Brands.Any(existing => existing.Name == coreBrand.Name))
        {
            await ShowAlreadyExistsAsync(coreBrand.Name).ConfigureAwait(true);
            return;
        }

        coreRoot.Brands.Add(coreBrand);
        await gameSettingService.SaveSettingAsync(coreRoot).ConfigureAwait(true);
        await RefreshCurrentViewAsync().ConfigureAwait(true);
    }

    private async Task AddCoreProductAsync(Product product)
    {
        if (CurrentItem is not Brand currentBrandItem)
        {
            return;
        }

        var coreBrand = coreRoot.Brands.FirstOrDefault(brand => brand.Name == currentBrandItem.Name);
        if (coreBrand is null)
        {
            return;
        }

        var coreProduct = await gameSettingService.CreateProductItemAsync(
            product.Name,
            product.IconPath,
            coreBrand.Name,
            product.Path).ConfigureAwait(true);
        if (coreBrand.Products.Any(existing => existing.Name == coreProduct.Name))
        {
            await ShowAlreadyExistsAsync(coreProduct.Name).ConfigureAwait(true);
            return;
        }

        coreBrand.Products.Add(coreProduct);
        await gameSettingService.SaveSettingAsync(coreRoot).ConfigureAwait(true);
        await RefreshCurrentViewAsync().ConfigureAwait(true);
    }

    private async Task EditCoreItemAsync(Item target, string name, string? iconPath, string filePath)
    {
        switch (CurrentItem, target)
        {
            case (RootItem, Brand brandView):
            {
                var coreBrand = coreRoot.Brands.FirstOrDefault(brand => brand.Name == brandView.Name);
                if (coreBrand is null)
                {
                    return;
                }

                coreBrand.Name = name;
                coreBrand.IconPath = await fileService.CopyIconFileAsync(iconPath).ConfigureAwait(true);
                break;
            }

            case (Brand parentBrandView, Product productView):
            {
                var coreBrand = coreRoot.Brands.FirstOrDefault(brand => brand.Name == parentBrandView.Name);
                var coreProduct = coreBrand?.Products.FirstOrDefault(product => product.Name == productView.Name);
                if (coreProduct is null)
                {
                    return;
                }

                coreProduct.Name = name;
                coreProduct.IconPath = await fileService.CopyIconFileAsync(iconPath).ConfigureAwait(true);
                coreProduct.Path = filePath;
                break;
            }
        }

        await gameSettingService.SaveSettingAsync(coreRoot).ConfigureAwait(true);
        await RefreshCurrentViewAsync().ConfigureAwait(true);
    }

    private void RemoveFromCore(Item item)
    {
        switch (CurrentItem, item)
        {
            case (RootItem, Brand brandView):
            {
                var coreBrand = coreRoot.Brands.FirstOrDefault(brand => brand.Name == brandView.Name);
                if (coreBrand is not null)
                {
                    coreRoot.Brands.Remove(coreBrand);
                }

                break;
            }

            case (Brand parentBrandView, Product productView):
            {
                var coreBrand = coreRoot.Brands.FirstOrDefault(brand => brand.Name == parentBrandView.Name);
                var coreProduct = coreBrand?.Products.FirstOrDefault(product => product.Name == productView.Name);
                if (coreBrand is not null && coreProduct is not null)
                {
                    coreBrand.Products.Remove(coreProduct);
                }

                break;
            }
        }
    }

    private async Task RefreshCurrentViewAsync()
    {
        var viewRoot = await ItemConversion.ToViewRootAsync(coreRoot, fileService).ConfigureAwait(true);

        // Preserve the current navigation depth by name so the visible list stays in place.
        var brandName = CurrentItem is Brand brand ? brand.Name : null;
        history.Clear();
        pageSelections.Clear();
        history.Add(viewRoot);
        historyIndex = 0;

        Item target = viewRoot;
        if (brandName is not null)
        {
            var reloadedBrand = viewRoot.Brands.FirstOrDefault(b => b.Name == brandName);
            if (reloadedBrand is not null)
            {
                history.Add(reloadedBrand);
                historyIndex = 1;
                target = reloadedBrand;
            }
        }

        ShowChildren(target);
        UpdateNavigationState();
    }

    private void PushHistory(Item item)
    {
        if (historyIndex < history.Count - 1)
        {
            history.RemoveRange(historyIndex + 1, history.Count - historyIndex - 1);
        }

        history.Add(item);
        historyIndex = history.Count - 1;
    }

    private void SaveCurrentPageSelection()
    {
        if (currentItem is not null)
        {
            pageSelections[currentItem] = SelectedItem;
        }
    }

    private Item? RestorePageSelection(Item page) =>
        pageSelections.TryGetValue(page, out var selection)
            ? selection
            : null;

    private void ShowChildren(Item item, Item? selection = null)
    {
        currentItem = item;
        SelectedItem = null;
        Items.Clear();
        IEnumerable<Item> children = item switch
        {
            RootItem root => root.Brands,
            Brand brand => brand.Products,
            _ => [],
        };
        foreach (var child in children)
        {
            Items.Add(child);
        }

        SelectedItem = selection is not null && Items.Contains(selection) ? selection : null;
        CurrentBrand = item is Brand currentBrandItem ? currentBrandItem.Name : null;
    }

    private void UpdateNavigationState()
    {
        IsEnabledBack = historyIndex > 0;
        IsEnabledForward = historyIndex >= 0 && historyIndex < history.Count - 1;
        NotifyCommandStates();
    }

    private ValueTask ShowAlreadyExistsAsync(string name) =>
        dialogService.ShowMessageAsync(name, FormatMessage("AlreadyExists", name));

    private string FormatMessage(string key, string itemName)
    {
        var format = resourceService.GetCultureString(key);
        return string.IsNullOrWhiteSpace(format) || format == key
            ? $"{key}: {itemName}"
            : string.Format(resourceService.CurrentCulture, format, itemName);
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
