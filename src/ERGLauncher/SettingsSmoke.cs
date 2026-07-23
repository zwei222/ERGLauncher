extern alias MigratedCore;

using System.Globalization;
using AppSettingService = MigratedCore::ERGLauncher.Core.Services.AppSettingService;
using Brand = MigratedCore::ERGLauncher.Core.Models.Brand;
using FileService = MigratedCore::ERGLauncher.Core.Services.FileService;
using GameSettingService = MigratedCore::ERGLauncher.Core.Services.GameSettingService;
using Product = MigratedCore::ERGLauncher.Core.Models.Product;
using Theme = MigratedCore::ERGLauncher.Core.Models.Theme;

namespace ERGLauncher;

/// <summary>
/// Headless verification path for settings compatibility in published binaries.
/// </summary>
public static class SettingsSmoke
{
    public const string BrandName = "Native AoT Smoke Brand";
    public const string UpdatedProductName = "Native AoT Smoke Product Updated";
    public const string DeletedProductName = "Native AoT Smoke Product Deleted";

    public static async Task<int> RunAsync(string baseDirectoryPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var firstFileService = new FileService(baseDirectoryPath);
            var appSettingService = new AppSettingService(firstFileService);
            var gameSettingService = new GameSettingService(firstFileService);

            var appSettings = await appSettingService.LoadAppSettingAsync(cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidDataException("settings/appSettings.json could not be loaded.");
            var root = await gameSettingService.LoadSettingAsync(cancellationToken).ConfigureAwait(false);
            if (root.Brands.Count == 0)
            {
                throw new InvalidDataException("settings/gameSettings.json did not contain a brand.");
            }

            Console.WriteLine($"SETTINGS_LOAD=PASS culture={appSettings.Culture.Name} theme={appSettings.Theme} brands={root.Brands.Count}");

            var smokeBrand = root.Brands.SingleOrDefault(brand => brand.Name == BrandName);
            if (smokeBrand is null)
            {
                smokeBrand = new Brand([]) { Name = BrandName };
                root.Brands.Add(smokeBrand);
                Console.WriteLine("CRUD_CREATE=PASS");
            }
            else
            {
                Console.WriteLine("CRUD_READ_AFTER_RESTART=PASS");
            }

            smokeBrand.Products.Clear();
            var retainedProduct = new Product
            {
                Name = "Native AoT Smoke Product",
                BrandName = BrandName,
                Path = "games/native-aot-smoke",
            };
            var deletedProduct = new Product
            {
                Name = DeletedProductName,
                BrandName = BrandName,
                Path = "games/delete-me",
            };
            smokeBrand.Products.Add(retainedProduct);
            smokeBrand.Products.Add(deletedProduct);
            retainedProduct.Name = UpdatedProductName;
            _ = smokeBrand.Products.Remove(deletedProduct);
            Console.WriteLine("CRUD_UPDATE_DELETE=PASS");

            appSettings.Culture = CultureInfo.GetCultureInfo("en-US");
            appSettings.Theme = Theme.Light;
            await appSettingService.SaveAppSettingAsync(appSettings, cancellationToken).ConfigureAwait(false);
            await gameSettingService.SaveSettingAsync(root, cancellationToken).ConfigureAwait(false);
            Console.WriteLine("SETTINGS_SAVE=PASS");

            var restartedFileService = new FileService(baseDirectoryPath);
            var restartedAppSettings = await new AppSettingService(restartedFileService)
                .LoadAppSettingAsync(cancellationToken)
                .ConfigureAwait(false);
            var restartedRoot = await new GameSettingService(restartedFileService)
                .LoadSettingAsync(cancellationToken)
                .ConfigureAwait(false);
            var restartedBrand = restartedRoot.Brands.Single(brand => brand.Name == BrandName);
            if (restartedAppSettings?.Culture.Name != "en-US"
                || restartedAppSettings.Theme != Theme.Light
                || restartedBrand.Products.Single().Name != UpdatedProductName
                || restartedBrand.Products.Any(product => product.Name == DeletedProductName))
            {
                throw new InvalidDataException("Settings did not persist after services were restarted.");
            }

            Console.WriteLine("RESTART_PERSISTENCE=PASS");
            Console.WriteLine("SETTINGS_SMOKE=PASS");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"SETTINGS_SMOKE=FAIL {exception.GetType().Name}: {exception.Message}");
            return 1;
        }
    }
}
