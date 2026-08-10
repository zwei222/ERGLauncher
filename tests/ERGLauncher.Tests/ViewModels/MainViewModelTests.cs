using System.Globalization;
using ERGLauncher.Core;
using ERGLauncher.Core.Models;
using ERGLauncher.Core.Services;
using ERGLauncher.Services;
using ERGLauncher.ViewModels;
using NSubstitute;
using CoreAppSettings = ERGLauncher.Core.Models.AppSettings;
using CoreBrand = ERGLauncher.Core.Models.Brand;
using CoreProduct = ERGLauncher.Core.Models.Product;
using CoreRootItem = ERGLauncher.Core.Models.RootItem;
using CoreTheme = ERGLauncher.Core.Models.Theme;
using ViewBrand = ERGLauncher.Core.Brand;
using ViewProduct = ERGLauncher.Core.Product;

namespace ERGLauncher.Tests.ViewModels;

public sealed class MainViewModelTests
{
    [Test]
    public async Task SelectItemCommandUsesTheItemParameterToNavigateToBrandProducts()
    {
        var coreProduct = new CoreProduct { Name = "Title", BrandName = "Studio", Path = "/games/title" };
        var coreBrand = new CoreBrand([coreProduct]) { Name = "Studio" };
        var services = CreateServices(new CoreRootItem([coreBrand]));
        await services.ViewModel.LoadSettingAsyncCommand.ExecuteAsync(null);
        var brand = services.ViewModel.Items.OfType<ViewBrand>().Single();

        await services.ViewModel.SelectItemAsyncCommand.ExecuteAsync(brand);

        var product = await Assert.That(services.ViewModel.Items).HasSingleItem();
        await Assert.That(product).IsTypeOf<ViewProduct>();
        await Assert.That(product.Name).IsEqualTo("Title");
        await Assert.That(services.ViewModel.CurrentBrand).IsEqualTo("Studio");
        await Assert.That(services.ViewModel.IsEnabledBack).IsTrue();
        await Assert.That(services.ViewModel.IsEnabledForward).IsFalse();
        await Assert.That(services.ViewModel.SelectedItem).IsNull();
    }

    [Test]
    public async Task RemoveItemCommandUsesTheSuppliedItemInsteadOfSelection()
    {
        var firstBrand = new CoreBrand([]) { Name = "First studio" };
        var secondBrand = new CoreBrand([]) { Name = "Second studio" };
        var services = CreateServices(new CoreRootItem([firstBrand, secondBrand]));
        services.Dialogs
            .ShowConfirmationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(true));
        await services.ViewModel.LoadSettingAsyncCommand.ExecuteAsync(null);
        var secondViewBrand = services.ViewModel.Items.OfType<ViewBrand>().Single(brand => brand.Name == "Second studio");

        await services.ViewModel.RemoveItemAsyncCommand.ExecuteAsync(secondViewBrand);

        await services.Dialogs.Received(1)
            .ShowConfirmationAsync("Second studio", Arg.Any<string>(), Arg.Any<CancellationToken>());
        await Assert.That(services.ViewModel.Items).HasSingleItem();
        await Assert.That(services.ViewModel.Items.Single().Name).IsEqualTo("First studio");
    }

    [Test]
    public async Task BrandNavigationClearsSelectionBeforeChangingItemsAndPreservesHistory()
    {
        var firstProduct = new CoreProduct { Name = "First title", BrandName = "First studio", Path = "/games/first" };
        var secondProduct = new CoreProduct { Name = "Second title", BrandName = "Second studio", Path = "/games/second" };
        var services = CreateServices(new CoreRootItem([
            new CoreBrand([firstProduct]) { Name = "First studio" },
            new CoreBrand([secondProduct]) { Name = "Second studio" },
        ]));
        await services.ViewModel.LoadSettingAsyncCommand.ExecuteAsync(null);
        var selectedBrand = services.ViewModel.Items.OfType<ViewBrand>().First();
        services.ViewModel.SelectedItem = selectedBrand;
        ERGLauncher.Core.Item? selectionAtFirstCollectionChange = selectedBrand;
        services.ViewModel.Items.CollectionChanged += (_, _) =>
            selectionAtFirstCollectionChange = services.ViewModel.SelectedItem;

        await services.ViewModel.SelectItemAsyncCommand.ExecuteAsync(selectedBrand);

        await Assert.That(selectionAtFirstCollectionChange).IsNull();
        await Assert.That(services.ViewModel.Items.Single().Name).IsEqualTo("First title");
        services.ViewModel.BackCommand.Execute(null);
        await Assert.That(services.ViewModel.Items).Count().IsEqualTo(2);
        services.ViewModel.ForwardCommand.Execute(null);
        await Assert.That(services.ViewModel.Items.Single().Name).IsEqualTo("First title");
        await Assert.That(services.ViewModel.CurrentBrand).IsEqualTo("First studio");
    }

    [Test]
    public async Task SelectItemCommandKeepsProductConfirmationAndLaunchBehavior()
    {
        var coreProduct = new CoreProduct { Name = "Title", BrandName = "Studio", Path = "/games/title" };
        var coreBrand = new CoreBrand([coreProduct]) { Name = "Studio" };
        var services = CreateServices(new CoreRootItem([coreBrand]));
        services.Dialogs
            .ShowConfirmationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(true));
        await services.ViewModel.LoadSettingAsyncCommand.ExecuteAsync(null);
        await services.ViewModel.SelectItemAsyncCommand.ExecuteAsync(services.ViewModel.Items.Single());
        var product = services.ViewModel.Items.Single();

        await services.ViewModel.SelectItemAsyncCommand.ExecuteAsync(product);

        await services.Dialogs.Received(1)
            .ShowConfirmationAsync("Title", Arg.Any<string>(), Arg.Any<CancellationToken>());
        await services.FileService.Received(1)
            .ExecuteAsync("/games/title", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task LoadingUsesDefaultIconOnlyForProductsWithoutConfiguredIcons()
    {
        var productWithoutIcon = new CoreProduct { Name = "Default", BrandName = "Studio", Path = "/games/default" };
        var productWithIcon = new CoreProduct
        {
            Name = "Explicit",
            BrandName = "Studio",
            Path = "/games/explicit",
            IconPath = "/icons/explicit.png",
        };
        var brand = new CoreBrand([productWithoutIcon, productWithIcon]) { Name = "Studio" };
        var services = CreateServices(new CoreRootItem([brand]));
        const string defaultIconPath = "/published/Assets/icon.png";
        services.FileService.GetDefaultIconFilePath().Returns(defaultIconPath);

        await services.ViewModel.LoadSettingAsyncCommand.ExecuteAsync(null);

        await services.FileService.Received(1)
            .CreateBitmapAsync(defaultIconPath, Arg.Any<CancellationToken>());
        await services.FileService.Received(1)
            .CreateBitmapAsync("/icons/explicit.png", Arg.Any<CancellationToken>());
        await services.FileService.Received(1)
            .CreateBitmapAsync(null, Arg.Any<CancellationToken>());
    }

    private static TestServices CreateServices(CoreRootItem root)
    {
        var fileService = Substitute.For<IFileService>();
        var appSettings = Substitute.For<IAppSettingService>();
        var gameSettings = Substitute.For<IGameSettingService>();
        var resources = Substitute.For<IResourceService>();
        var themes = Substitute.For<IThemeService>();
        var dialogs = Substitute.For<IDialogService>();
        var viewDialogs = Substitute.For<IViewDialogService>();

        appSettings.LoadAppSettingAsync(Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult<CoreAppSettings?>(null));
        gameSettings.LoadSettingAsync(Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(root));
        fileService.CreateBitmapAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult<Avalonia.Media.Imaging.Bitmap?>(null));
        resources.CurrentCulture.Returns(CultureInfo.InvariantCulture);
        resources.GetCultureString(Arg.Any<string>()).Returns(call => call.Arg<string>());
        themes.CurrentTheme.Returns(CoreTheme.None);

        var viewModel = new MainViewModel(
            fileService,
            appSettings,
            gameSettings,
            resources,
            themes,
            dialogs,
            viewDialogs);
        return new TestServices(viewModel, fileService, dialogs);
    }

    private sealed record TestServices(
        MainViewModel ViewModel,
        IFileService FileService,
        IDialogService Dialogs);
}
