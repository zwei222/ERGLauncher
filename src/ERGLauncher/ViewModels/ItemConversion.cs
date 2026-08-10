extern alias MigratedCore;

using System;
using System.Threading;
using System.Threading.Tasks;
using CoreBrand = MigratedCore::ERGLauncher.Core.Models.Brand;
using CoreItem = MigratedCore::ERGLauncher.Core.Models.Item;
using CoreProduct = MigratedCore::ERGLauncher.Core.Models.Product;
using CoreRootItem = MigratedCore::ERGLauncher.Core.Models.RootItem;
using IFileService = MigratedCore::ERGLauncher.Core.Services.IFileService;
using ViewBrand = ERGLauncher.Core.Brand;
using ViewItem = ERGLauncher.Core.Item;
using ViewProduct = ERGLauncher.Core.Product;
using ViewRootItem = ERGLauncher.Core.RootItem;

namespace ERGLauncher.ViewModels;

/// <summary>
/// Bridges the migrated core model tree (<c>ERGLauncher.Core.Models.*</c>) and the
/// presentation model tree (<c>ERGLauncher.Core.*</c>) consumed by the compiled-binding
/// views. Icons are reloaded from <see cref="ViewItem.IconPath"/> via the file service so
/// no framework-specific bitmap plumbing leaks across the boundary.
/// </summary>
internal static class ItemConversion
{
    public static async Task<ViewItem?> ToViewItemAsync(
        CoreItem? item,
        IFileService fileService,
        CancellationToken cancellationToken = default)
    {
        switch (item)
        {
            case null:
                return null;
            case CoreBrand brand:
            {
                var viewBrand = new ViewBrand([])
                {
                    Name = brand.Name,
                    IconPath = brand.IconPath,
                };
                viewBrand.Icon = await fileService.CreateBitmapAsync(brand.IconPath, cancellationToken).ConfigureAwait(true);
                foreach (var product in brand.Products)
                {
                    if (await ToViewItemAsync(product, fileService, cancellationToken).ConfigureAwait(true) is ViewProduct viewProduct)
                    {
                        viewBrand.Products.Add(viewProduct);
                    }
                }

                return viewBrand;
            }

            case CoreProduct product:
            {
                var viewProduct = new ViewProduct
                {
                    Name = product.Name,
                    IconPath = product.IconPath,
                    Path = product.Path,
                    BrandName = product.BrandName,
                };
                var iconPath = string.IsNullOrWhiteSpace(product.IconPath)
                    ? fileService.GetDefaultIconFilePath()
                    : product.IconPath;
                viewProduct.Icon = await fileService.CreateBitmapAsync(iconPath, cancellationToken).ConfigureAwait(true);
                return viewProduct;
            }

            default:
                throw new NotSupportedException($"Unsupported core item type '{item.GetType().Name}'.");
        }
    }

    public static async Task<ViewRootItem> ToViewRootAsync(
        CoreRootItem root,
        IFileService fileService,
        CancellationToken cancellationToken = default)
    {
        var viewRoot = new ViewRootItem([]);
        foreach (var brand in root.Brands)
        {
            if (await ToViewItemAsync(brand, fileService, cancellationToken).ConfigureAwait(true) is ViewBrand viewBrand)
            {
                viewRoot.Brands.Add(viewBrand);
            }
        }

        return viewRoot;
    }
}
