using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ERGLauncher.Core;
using Utf8Json;

namespace ERGLauncher.Services.Implementations
{
    /// <summary>
    /// Game setting service.
    /// </summary>
    public class GameSettingService : IGameSettingService
    {
        /// <summary>
        /// Game settings file name.
        /// </summary>
        private const string GameSettingsFileName = "gameSettings.json";

        /// <summary>
        /// Dispatcher service.
        /// </summary>
        private readonly IDispatcherService dispatcherService;

        /// <summary>
        /// File service.
        /// </summary>
        private readonly IFileService fileService;

        /// <summary>
        /// Game settings file path.
        /// </summary>
        private readonly string settingFilePath;

        /// <summary>
        /// Default icon file path.
        /// </summary>
        private readonly string defaultIconFilePath;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dispatcherService">Dispatcher service</param>
        /// <param name="fileService">File service</param>
        public GameSettingService(IDispatcherService dispatcherService, IFileService fileService)
        {
            this.dispatcherService = dispatcherService;
            this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            this.settingFilePath = Path.Combine(this.fileService.GetSettingsDirectoryPath(), GameSettingsFileName);
            this.defaultIconFilePath = this.fileService.GetDefaultIconFilePath();
        }

        /// <inheritdoc />
        public async ValueTask<RootItem> LoadSettingAsync(CancellationToken cancellationToken = default)
        {
            if (!File.Exists(this.settingFilePath))
            {
                return new RootItem(new List<Brand>());
            }

            var jsonBytes = await File.ReadAllBytesAsync(this.settingFilePath, cancellationToken).ConfigureAwait(true);
            var rootItem = JsonSerializer.Deserialize<RootItem>(jsonBytes);

            foreach (var brand in rootItem.Brands)
            {
                if (string.IsNullOrEmpty(brand.IconPath))
                {
                    brand.Icon = await this.CreateBitmapImage(this.defaultIconFilePath).ConfigureAwait(false);
                }
                else
                {
                    brand.Icon = await this.CreateBitmapImage(brand.IconPath).ConfigureAwait(false);
                }

                foreach (var product in brand.Products)
                {
                    if (string.IsNullOrEmpty(product.IconPath))
                    {
                        product.Icon = await this.CreateBitmapImage(this.defaultIconFilePath).ConfigureAwait(false);
                    }
                    else
                    {
                        product.Icon = await this.CreateBitmapImage(product.IconPath).ConfigureAwait(false);
                    }

                    if (product.BrandName != brand.Name)
                    {
                        product.BrandName = brand.Name;
                    }
                }
            }

            return rootItem;
        }

        /// <inheritdoc />
        public async ValueTask SaveSettingAsync(RootItem? rootItem, CancellationToken cancellationToken = default)
        {
            await this.dispatcherService.SafeAction(async () =>
            {
                var settingDirectoryPath = this.fileService.GetSettingsDirectoryPath();

                if (!Directory.Exists(settingDirectoryPath))
                {
                    Directory.CreateDirectory(settingDirectoryPath);
                }

                var jsonBytes = JsonSerializer.Serialize(rootItem);

                await File.WriteAllBytesAsync(this.settingFilePath, jsonBytes, cancellationToken).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async ValueTask<Brand> CreateBrandItemAsync(string name, string? iconFilePath)
        {
            var brand = new Brand(new List<Product>())
            {
                Name = name,
                IconPath = iconFilePath,
            };

            if (string.IsNullOrEmpty(iconFilePath))
            {
                brand.Icon = await this.CreateBitmapImage(this.defaultIconFilePath).ConfigureAwait(false);
            }
            else
            {
                brand.Icon = await this.CreateBitmapImage(iconFilePath).ConfigureAwait(false);
            }

            return brand;
        }

        /// <inheritdoc />
        public async ValueTask<Product> CreateProductItemAsync(string name, string? iconFilePath, string brandName, string gameFilePath)
        {
            var product = new Product
            {
                Name = name,
                IconPath = iconFilePath,
                BrandName = brandName,
                Path = gameFilePath,
            };

            if (string.IsNullOrEmpty(iconFilePath))
            {
                product.Icon = await this.CreateBitmapImage(this.defaultIconFilePath).ConfigureAwait(false);
            }
            else
            {
                product.Icon = await this.CreateBitmapImage(iconFilePath).ConfigureAwait(false);
            }

            return product;
        }

        private async ValueTask<BitmapImage> CreateBitmapImage(string filePath)
        {
            return await this.fileService.CreateBitmapImageAsync(filePath).ConfigureAwait(false);
        }
    }
}
