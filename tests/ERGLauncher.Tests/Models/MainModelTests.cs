using System.Globalization;
using ERGLauncher.Core.Models;
using ERGLauncher.Core.Services;
using NSubstitute;

namespace ERGLauncher.Tests.Models;

public sealed class MainModelTests
{
    [Test]
    public async Task LoadSettingPopulatesItemsFromInjectedGameSettingService()
    {
        var brand = new Brand([]) { Name = "Studio" };
        var services = CreateServices(new RootItem([brand]));

        await services.Model.LoadSettingAsync();

        await Assert.That(services.Model.Items).HasSingleItem();
        await Assert.That(services.Model.Items[0]).IsSameReferenceAs(brand);
        await services.GameSettings.Received(1).LoadSettingAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task AddItemDoesNotAddOrSaveDuplicate()
    {
        var services = CreateServices(new RootItem([]));
        services.GameSettings
            .CreateBrandItemAsync("Studio", null, Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(new Brand([]) { Name = "Studio" }));
        await services.Model.LoadSettingAsync();

        await services.Model.AddItemAsync("Studio", null, string.Empty);
        await services.Model.AddItemAsync("Studio", null, string.Empty);

        await Assert.That(services.Model.Items).HasSingleItem();
        await services.GameSettings.Received(1)
            .SaveSettingAsync(Arg.Any<RootItem>(), Arg.Any<CancellationToken>());
        await services.Dialogs.Received(1)
            .ShowMessageAsync("Studio", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RemoveItemRemovesSelectionAndPersistsRoot()
    {
        var brand = new Brand([]) { Name = "Studio" };
        var root = new RootItem([brand]);
        var services = CreateServices(root);
        services.Dialogs
            .ShowConfirmationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(true));
        await services.Model.LoadSettingAsync();
        services.Model.SelectedItem = brand;

        var removed = await services.Model.RemoveItemAsync();

        await Assert.That(removed).IsTrue();
        await Assert.That(services.Model.Items).IsEmpty();
        await Assert.That(root.Brands).IsEmpty();
        await services.GameSettings.Received(1)
            .SaveSettingAsync(root, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task BackAndForwardRestoreEachNavigationLevel()
    {
        var product = new Product { Name = "Title", BrandName = "Studio", Path = "/games/title" };
        var brand = new Brand([product]) { Name = "Studio" };
        var services = CreateServices(new RootItem([brand]));
        await services.Model.LoadSettingAsync();
        services.Model.SelectedItem = brand;
        await services.Model.SelectItemAsync();

        services.Model.Back();

        await Assert.That(services.Model.Items.Single()).IsSameReferenceAs(brand);
        await Assert.That(services.Model.IsEnabledForward).IsTrue();

        services.Model.Forward();

        await Assert.That(services.Model.Items.Single()).IsSameReferenceAs(product);
        await Assert.That(services.Model.IsEnabledBack).IsTrue();
    }

    [Test]
    public async Task InjectedFileAndAppSettingServicesAreUsed()
    {
        var product = new Product { Name = "Title", BrandName = "Studio", Path = "/games/title" };
        var brand = new Brand([product]) { Name = "Studio" };
        var services = CreateServices(new RootItem([brand]));
        services.AppSettings
            .LoadAppSettingAsync(Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult<AppSettings?>(null));
        services.Dialogs
            .ShowConfirmationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(true));

        await services.Model.LoadAppSettingAsync();
        await services.Model.LoadSettingAsync();
        services.Model.SelectedItem = brand;
        await services.Model.SelectItemAsync();
        services.Model.SelectedItem = product;
        await services.Model.SelectItemAsync();

        await services.AppSettings.Received(1)
            .LoadAppSettingAsync(Arg.Any<CancellationToken>());
        await services.FileService.Received(1)
            .ExecuteAsync(product.Path, Arg.Any<CancellationToken>());
    }

    private static TestServices CreateServices(RootItem root)
    {
        var fileService = Substitute.For<IFileService>();
        var appSettings = Substitute.For<IAppSettingService>();
        var gameSettings = Substitute.For<IGameSettingService>();
        var resources = Substitute.For<IResourceService>();
        var themes = Substitute.For<IThemeService>();
        var dialogs = Substitute.For<IDialogService>();
        gameSettings
            .LoadSettingAsync(Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(root));
        resources.CurrentCulture.Returns(CultureInfo.InvariantCulture);
        resources.GetCultureString(Arg.Any<string>()).Returns(call => call.Arg<string>());
        themes.CurrentTheme.Returns(Theme.None);

        var model = new MainModel(fileService, appSettings, gameSettings, resources, themes, dialogs);
        return new TestServices(model, fileService, appSettings, gameSettings, dialogs);
    }

    private sealed record TestServices(
        MainModel Model,
        IFileService FileService,
        IAppSettingService AppSettings,
        IGameSettingService GameSettings,
        IDialogService Dialogs);
}
