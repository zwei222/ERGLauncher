using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Cysharp.Diagnostics;
using ERGLauncher.Core;
using ERGLauncher.Services;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace ERGLauncher.Models.Implementations
{
    /// <summary>
    /// Main model.
    /// </summary>
    public class MainModel : ModelBase, IMainModel
    {
        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<MainModel> logger;

        /// <summary>
        /// File service.
        /// </summary>
        private readonly IFileService fileService;

        /// <summary>
        /// App setting service.
        /// </summary>
        private readonly IAppSettingService appSettingService;

        /// <summary>
        /// Game setting service.
        /// </summary>
        private readonly IGameSettingService gameSettingService;

        /// <summary>
        /// <see langword="true" /> if you can go back; otherwise <see langword="false" />.
        /// </summary>
        private bool isEnabledBack;

        /// <summary>
        /// <see langword="true" /> if you can go forward; otherwise <see langword="false" />.
        /// </summary>
        private bool isEnabledForward;

        /// <summary>
        /// Current brand.
        /// </summary>
        private string currentBrand;

        /// <summary>
        /// Selected item.
        /// </summary>
        private Item? selectedItem;

        /// <summary>
        /// Current item.
        /// </summary>
        private Item? currentItem;

        /// <summary>
        /// Item history.
        /// </summary>
        private HistoryCollection<Item> itemHistory;

        /// <summary>
        /// Root item.
        /// </summary>
        private RootItem? rootItem;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="resourceService">Resource service</param>
        /// <param name="themeService">Theme service</param>
        /// <param name="fileService">File service</param>
        /// <param name="appSettingService">App setting service</param>
        /// <param name="gameSettingService">Game setting service</param>
        /// <param name="dialogCoordinator">Dialog coordinator</param>
        public MainModel(
            ILogger<MainModel> logger,
            IResourceService resourceService,
            IThemeService themeService,
            IFileService fileService,
            IAppSettingService appSettingService,
            IGameSettingService gameSettingService,
            IDialogCoordinator dialogCoordinator)
            : base(resourceService, themeService)
        {
            this.logger = logger;
            this.fileService = fileService;
            this.appSettingService = appSettingService;
            this.gameSettingService = gameSettingService;
            this.itemHistory = new HistoryCollection<Item>();
            this.DialogCoordinator = dialogCoordinator;
            this.Title = "ERG Launcher";
            this.Top = double.NaN;
            this.Left = double.NaN;
            this.Height = 450;
            this.Width = 800;
            this.WindowState = WindowState.Normal;
            this.IsEnabledBack = false;
            this.IsEnabledForward = false;
            this.Items = new ObservableCollection<Item>();
            this.currentBrand = string.Empty;
        }

        /// <inheritdoc />
        public IDialogCoordinator DialogCoordinator { get; }

        /// <inheritdoc />
        public bool IsEnabledBack
        {
            get => this.isEnabledBack;
            private set => this.SetProperty(ref this.isEnabledBack, value);
        }

        /// <inheritdoc />
        public bool IsEnabledForward
        {
            get => this.isEnabledForward;
            private set => this.SetProperty(ref this.isEnabledForward, value);
        }

        /// <inheritdoc />
        public string CurrentBrand
        {
            get => this.currentBrand;
            private set => this.SetProperty(ref this.currentBrand, value);
        }

        /// <inheritdoc />
        public ObservableCollection<Item> Items { get; }

        /// <inheritdoc />
        public Item? SelectedItem
        {
            get => this.selectedItem;
            set => this.SetProperty(ref this.selectedItem, value);
        }

        /// <inheritdoc />
        public Item? CurrentItem
        {
            get => this.currentItem;
            private set => this.SetProperty(ref this.currentItem, value);
        }

        /// <inheritdoc />
        public void Back()
        {
            this.itemHistory.Back();
            this.UpdateCollection(this.itemHistory.CurrentValue);
        }

        /// <inheritdoc />
        public void Forward()
        {
            this.itemHistory.Forward();
            this.UpdateCollection(this.itemHistory.CurrentValue);
        }

        /// <inheritdoc />
        public async ValueTask SelectItemAsync()
        {
            var item = this.SelectedItem;

            if (item == null)
            {
                return;
            }

            if (item is Product product)
            {
                var dialogSettings = new MetroDialogSettings
                {
                    AffirmativeButtonText = "OK",
                    NegativeButtonText = "Cancel",
                    AnimateHide = true,
                    AnimateShow = true,
                    DefaultButtonFocus = MessageDialogResult.Affirmative,
                    ColorScheme = MetroDialogColorScheme.Theme,
                };
                var result = await this.DialogCoordinator.ShowMessageAsync(
                    Application.Current.MainWindow.DataContext,
                    product.Name,
                    string.Format(this.CurrentCulture, this.GetCultureString(nameof(Properties.Resources.DoYouWantToLaunch)), product.Name),
                    MessageDialogStyle.AffirmativeAndNegative,
                    dialogSettings).ConfigureAwait(false);

                if (result == MessageDialogResult.Affirmative)
                {
                    try
                    {
                        await this.fileService.ExecuteAsync(product.Path).ConfigureAwait(false);
                    }
                    catch (ProcessErrorException processErrorException)
                    {
                        foreach (var error in processErrorException.ErrorOutput)
                        {
                            this.logger.ZLogError(error);
                        }
                    }
                }

                return;
            }

            this.itemHistory.Push(item);
            this.UpdateCollection(item);
        }

        /// <inheritdoc />
        public async ValueTask AddItemAsync(string name, string? iconFilePath, string filePath)
        {
            var parent = this.itemHistory.CurrentValue;

            if (parent is RootItem parentRootItem)
            {
                var brand = await this.gameSettingService.CreateBrandItemAsync(name, iconFilePath)
                    .ConfigureAwait(false);

                if (parentRootItem.Brands.Any(item => item.Name == brand.Name))
                {
                    var dialogSettings = new MetroDialogSettings
                    {
                        AffirmativeButtonText = "OK",
                        AnimateHide = true,
                        AnimateShow = true,
                        ColorScheme = MetroDialogColorScheme.Theme,
                    };
                    await this.DialogCoordinator.ShowMessageAsync(
                        Application.Current.MainWindow.DataContext,
                        brand.Name,
                        string.Format(this.CurrentCulture, this.GetCultureString(nameof(Properties.Resources.AlreadyExists)), brand.Name),
                        MessageDialogStyle.Affirmative,
                        dialogSettings).ConfigureAwait(false);

                    return;
                }

                parentRootItem.Brands.Add(brand);
            }
            else if (parent is Brand brand)
            {
                var product =
                    await this.gameSettingService.CreateProductItemAsync(name, iconFilePath, brand.Name, filePath)
                        .ConfigureAwait(false);

                if (brand.Products.Any(item => item.Name == product.Name))
                {
                    var dialogSettings = new MetroDialogSettings
                    {
                        AffirmativeButtonText = "OK",
                        AnimateHide = true,
                        AnimateShow = true,
                        ColorScheme = MetroDialogColorScheme.Theme,
                    };
                    await this.DialogCoordinator.ShowMessageAsync(
                        Application.Current.MainWindow.DataContext,
                        product.Name,
                        string.Format(this.CurrentCulture, this.GetCultureString(nameof(Properties.Resources.AlreadyExists)), product.Name),
                        MessageDialogStyle.Affirmative,
                        dialogSettings).ConfigureAwait(false);

                    return;
                }

                brand.Products.Add(product);
            }
            else
            {
                return;
            }

            await this.gameSettingService.SaveSettingAsync(this.rootItem).ConfigureAwait(false);
            this.UpdateCollection(parent);
        }

        /// <inheritdoc />
        public async ValueTask EditItemAsync(string name, string? iconFilePath, string filePath)
        {
            var item = this.SelectedItem;

            if (item == null)
            {
                return;
            }

            this.Items.Remove(item);

            var parent = this.itemHistory.CurrentValue;

            if (parent is RootItem parentRootItem)
            {
                parentRootItem.Brands.Remove((Brand)item);
            }
            else if (parent is Brand brand)
            {
                brand.Products.Remove((Product)item);
            }

            this.itemHistory.Remove(item);
            await this.AddItemAsync(name, iconFilePath, filePath).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async ValueTask<bool> RemoveItemAsync()
        {
            var item = this.SelectedItem;

            if (item == null)
            {
                return false;
            }

            var dialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "OK",
                NegativeButtonText = "Cancel",
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Theme,
            };
            var result = await this.DialogCoordinator.ShowMessageAsync(
                Application.Current.MainWindow.DataContext,
                item.Name,
                string.Format(this.CurrentCulture, this.GetCultureString(nameof(Properties.Resources.DoYouWantToRemove)), item.Name),
                MessageDialogStyle.AffirmativeAndNegative,
                dialogSettings).ConfigureAwait(false);

            if (result == MessageDialogResult.Affirmative)
            {
                this.Items.Remove(item);

                var parent = this.itemHistory.CurrentValue;

                if (parent is RootItem parentRootItem)
                {
                    parentRootItem.Brands.Remove((Brand)item);
                }
                else if (parent is Brand brand)
                {
                    brand.Products.Remove((Product)item);
                }

                this.itemHistory.Remove(item);
                this.CheckHistory();

                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public async ValueTask LoadAppSettingAsync()
        {
            var appSettings = await this.appSettingService.LoadAppSettingAsync().ConfigureAwait(false);

            if (appSettings == null)
            {
                return;
            }

            this.ChangeCulture(appSettings.Culture);
            await this.ChangeThemeAsync(appSettings.Theme).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async ValueTask SaveAppSettingAsync()
        {
            var settings = new AppSettings
            {
                Culture = this.CurrentCulture,
                Theme = this.CurrentTheme,
            };

            await this.appSettingService.SaveAppSettingAsync(settings).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async ValueTask LoadSettingAsync()
        {
            this.rootItem = await this.gameSettingService.LoadSettingAsync().ConfigureAwait(false);
            this.CurrentItem = this.rootItem;
            this.itemHistory = new HistoryCollection<Item>(this.rootItem);
            this.Items.AddRange(this.rootItem.Brands);
        }

        /// <inheritdoc />
        public async ValueTask SaveSettingAsync()
        {
            if (this.CurrentItem is RootItem currentRootItem)
            {
                currentRootItem.Brands.Clear();

                foreach (var item in this.Items)
                {
                    if (item is Brand brand)
                    {
                        currentRootItem.Brands.Add(brand);
                    }
                }

                await this.gameSettingService.SaveSettingAsync(currentRootItem).ConfigureAwait(false);
            }
            else if (this.CurrentItem is Brand brand)
            {
                brand.Products.Clear();

                foreach (var item in this.Items)
                {
                    if (item is Product product)
                    {
                        brand.Products.Add(product);
                    }
                }

                await this.gameSettingService.SaveSettingAsync(this.rootItem).ConfigureAwait(false);
            }
        }

        private void UpdateCollection(Item item)
        {
            this.CurrentBrand = string.Empty;
            this.Items.Clear();

            if (item is RootItem tempRootItem)
            {
                this.Items.AddRange(tempRootItem.Brands);
                this.CurrentItem = tempRootItem;
            }
            else if (item is Brand brand)
            {
                this.CurrentBrand = brand.Name;
                this.Items.AddRange(brand.Products);
                this.CurrentItem = brand;
            }

            this.CheckHistory();
        }

        private void CheckHistory()
        {
            this.IsEnabledBack = this.itemHistory.IsEnabledUndo;
            this.IsEnabledForward = this.itemHistory.IsEnabledRedo;
        }
    }
}
