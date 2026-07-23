using System.Collections.ObjectModel;
using ERGLauncher.Core.Services;

namespace ERGLauncher.Core.Models;

public sealed class MainModel : ModelBase, IMainModel
{
    private readonly IFileService fileService;
    private readonly IAppSettingService appSettingService;
    private readonly IGameSettingService gameSettingService;
    private readonly IDialogService dialogService;
    private RootItem rootItem = new([]);
    private HistoryCollection<Item> history = new();
    private Item? selectedItem;

    public MainModel(
        IFileService fileService,
        IAppSettingService appSettingService,
        IGameSettingService gameSettingService,
        IResourceService resourceService,
        IThemeService themeService,
        IDialogService dialogService)
        : base(resourceService, themeService)
    {
        this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        this.appSettingService = appSettingService ?? throw new ArgumentNullException(nameof(appSettingService));
        this.gameSettingService = gameSettingService ?? throw new ArgumentNullException(nameof(gameSettingService));
        this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        this.Title = "ERG Launcher";
        this.Top = double.NaN;
        this.Left = double.NaN;
        this.Height = 450;
        this.Width = 800;
    }

    public bool IsEnabledBack => this.history.IsEnabledUndo;
    public bool IsEnabledForward => this.history.IsEnabledRedo;
    public string? CurrentBrand => this.CurrentItem is Brand brand ? brand.Name : null;
    public ObservableCollection<Item> Items { get; } = [];
    public Item? CurrentItem => this.history.Count == 0 ? null : this.history.CurrentValue;

    public Item? SelectedItem
    {
        get => this.selectedItem;
        set => this.SetProperty(ref this.selectedItem, value);
    }

    public void Back()
    {
        var item = this.history.Back();
        if (item is not null)
        {
            this.ShowChildren(item);
        }
    }

    public void Forward()
    {
        var item = this.history.Forward();
        if (item is not null)
        {
            this.ShowChildren(item);
        }
    }

    public async ValueTask SelectItemAsync(CancellationToken cancellationToken = default)
    {
        switch (this.SelectedItem)
        {
            case Brand brand:
                this.history.Push(brand);
                this.ShowChildren(brand);
                break;
            case Product product:
                if (await this.dialogService.ShowConfirmationAsync(
                    product.Name,
                    this.FormatMessage("DoYouWantToLaunch", product.Name),
                    cancellationToken).ConfigureAwait(false))
                {
                    await this.fileService.ExecuteAsync(product.Path, cancellationToken).ConfigureAwait(false);
                }

                break;
        }
    }

    public async ValueTask AddItemAsync(
        string name,
        string? iconFilePath,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        Item? item = this.CurrentItem switch
        {
            RootItem => await this.gameSettingService.CreateBrandItemAsync(name, iconFilePath, cancellationToken).ConfigureAwait(false),
            Brand brand => await this.gameSettingService.CreateProductItemAsync(name, iconFilePath, brand.Name, filePath, cancellationToken).ConfigureAwait(false),
            _ => null,
        };

        if (item is null)
        {
            return;
        }

        if (this.Items.Any(existing => existing.Name == item.Name))
        {
            await this.ShowAlreadyExistsAsync(item.Name, cancellationToken).ConfigureAwait(false);
            return;
        }

        switch (this.CurrentItem, item)
        {
            case (RootItem root, Brand brand):
                root.Brands.Add(brand);
                this.Items.Add(brand);
                break;
            case (Brand brand, Product product):
                brand.Products.Add(product);
                this.Items.Add(product);
                break;
            default:
                return;
        }

        await this.SaveSettingAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask EditItemAsync(
        string name,
        string? iconFilePath,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var item = this.SelectedItem;
        if (item is null)
        {
            return;
        }

        if (this.Items.Any(existing => !ReferenceEquals(existing, item) && existing.Name == name))
        {
            await this.ShowAlreadyExistsAsync(name, cancellationToken).ConfigureAwait(false);
            return;
        }

        item.Name = name;
        item.IconPath = await this.fileService.CopyIconFileAsync(iconFilePath, cancellationToken).ConfigureAwait(false);
        item.Icon = await this.fileService.CreateBitmapAsync(item.IconPath, cancellationToken).ConfigureAwait(false);
        if (item is Product product)
        {
            product.Path = filePath;
        }

        await this.SaveSettingAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<bool> RemoveItemAsync(CancellationToken cancellationToken = default)
    {
        var item = this.SelectedItem;
        if (item is null)
        {
            return false;
        }

        if (!await this.dialogService.ShowConfirmationAsync(
            item.Name,
            this.FormatMessage("DoYouWantToRemove", item.Name),
            cancellationToken).ConfigureAwait(false))
        {
            return false;
        }

        var removed = (this.CurrentItem, item) switch
        {
            (RootItem root, Brand brand) => root.Brands.Remove(brand),
            (Brand brand, Product product) => brand.Products.Remove(product),
            _ => false,
        };
        if (!removed)
        {
            return false;
        }

        this.Items.Remove(item);
        this.history.Remove(item);
        this.SelectedItem = null;
        await this.SaveSettingAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async ValueTask LoadAppSettingAsync(CancellationToken cancellationToken = default)
    {
        var settings = await this.appSettingService.LoadAppSettingAsync(cancellationToken).ConfigureAwait(false);
        if (settings is null)
        {
            return;
        }

        this.ChangeCulture(settings.Culture);
        await this.ChangeThemeAsync(settings.Theme, cancellationToken).ConfigureAwait(false);
        this.OnPropertyChanged(nameof(this.CurrentCulture));
        this.OnPropertyChanged(nameof(this.CurrentTheme));
    }

    public ValueTask SaveAppSettingAsync(CancellationToken cancellationToken = default) =>
        this.appSettingService.SaveAppSettingAsync(
            new AppSettings { Culture = this.CurrentCulture, Theme = this.CurrentTheme },
            cancellationToken);

    public async ValueTask LoadSettingAsync(CancellationToken cancellationToken = default)
    {
        this.rootItem = await this.gameSettingService.LoadSettingAsync(cancellationToken).ConfigureAwait(false);
        this.history = new HistoryCollection<Item>(this.rootItem);
        this.ShowChildren(this.rootItem);
    }

    public ValueTask SaveSettingAsync(CancellationToken cancellationToken = default) =>
        this.gameSettingService.SaveSettingAsync(this.rootItem, cancellationToken);

    private void ShowChildren(Item item)
    {
        this.Items.Clear();
        var children = item switch
        {
            RootItem root => root.Brands.Cast<Item>(),
            Brand brand => brand.Products.Cast<Item>(),
            _ => [],
        };
        foreach (var child in children)
        {
            this.Items.Add(child);
        }

        this.SelectedItem = null;
        this.OnPropertyChanged(nameof(this.CurrentItem));
        this.OnPropertyChanged(nameof(this.CurrentBrand));
        this.OnPropertyChanged(nameof(this.IsEnabledBack));
        this.OnPropertyChanged(nameof(this.IsEnabledForward));
    }

    private ValueTask ShowAlreadyExistsAsync(string name, CancellationToken cancellationToken) =>
        this.dialogService.ShowMessageAsync(
            name,
            this.FormatMessage("AlreadyExists", name),
            cancellationToken);

    private string FormatMessage(string key, string itemName)
    {
        var format = this.GetCultureString(key);
        return string.IsNullOrWhiteSpace(format) || format == key
            ? $"{key}: {itemName}"
            : string.Format(this.CurrentCulture, format, itemName);
    }
}
