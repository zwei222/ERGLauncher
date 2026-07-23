using System.Xml.Linq;

namespace ERGLauncher.Views.Tests;

public class ViewContractTests
{
    private static readonly string ViewsDirectory = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "../../../../../src/ERGLauncher/Views"));

    [Arguments("MainView")]
    [Arguments("AddBrandView")]
    [Arguments("AddProductView")]
    [Arguments("SettingView")]
    [Arguments("MessageView")]
    [Test]
    public async Task View_IsValidAvaloniaAxaml(string viewName)
    {
        var path = Path.Combine(ViewsDirectory, $"{viewName}.axaml");

        await Assert.That(File.Exists(path)).IsTrue();
        var document = XDocument.Load(path);
        await Assert.That(document.Root).IsNotNull();
        await Assert.That(document.Root!.Name.NamespaceName)
            .IsEqualTo("https://github.com/avaloniaui");
    }

    [Test]
    public async Task Views_DoNotUseWpfPrismOrReactivePropertyBindings()
    {
        foreach (var path in Directory.GetFiles(ViewsDirectory, "*.axaml"))
        {
            var source = await File.ReadAllTextAsync(path);
            await Assert.That(source).DoesNotContain("xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
            await Assert.That(source).DoesNotContain("prism:");
            await Assert.That(source).DoesNotContain("materialDesign:");
            await Assert.That(source).DoesNotContain(".Value");
        }
    }

    [Arguments("MainView")]
    [Arguments("AddBrandView")]
    [Arguments("AddProductView")]
    [Arguments("SettingView")]
    [Arguments("MessageView")]
    [Test]
    public async Task View_UsesExistingLocalizedResources(string viewName)
    {
        var source = await File.ReadAllTextAsync(Path.Combine(ViewsDirectory, $"{viewName}.axaml"));

        await Assert.That(source).Contains("{x:Static properties:Resources.");
    }

    [Test]
    public async Task SettingView_ProvidesLanguageAndThemeChoices()
    {
        var source = await File.ReadAllTextAsync(Path.Combine(ViewsDirectory, "SettingView.axaml"));

        await Assert.That(source).Contains("{x:Static views:SettingView.AvailableLanguages}");
        await Assert.That(source).Contains("{x:Static views:SettingView.AvailableThemeChoices}");
    }

    [Test]
    public async Task App_StartsWithMigratedMainView()
    {
        var appSource = await File.ReadAllTextAsync(Path.Combine(
            ViewsDirectory,
            "..",
            "App.axaml.cs"));

        await Assert.That(appSource).Contains("GetRequiredService<MainView>()");
        await Assert.That(appSource).DoesNotContain("GetRequiredService<MainWindow>()");
    }

    [Test]
    public async Task MainView_LoadsAndSavesSettingsAtWindowLifecycleBoundaries()
    {
        var source = await File.ReadAllTextAsync(Path.Combine(ViewsDirectory, "MainView.axaml"));

        await Assert.That(source).Contains("OpenedCommand=\"{Binding LoadSettingAsyncCommand}\"");
        await Assert.That(source).Contains("ClosingCommand=\"{Binding SaveAppSettingAsyncCommand}\"");
    }

    [Test]
    public async Task MainView_UsesCompiledBindingsForNativeAot()
    {
        var source = await File.ReadAllTextAsync(Path.Combine(ViewsDirectory, "MainView.axaml"));

        await Assert.That(source).Contains("x:CompileBindings=\"True\"");
        await Assert.That(source).Contains("x:DataType=\"views:IMainViewDataContext\"");
        await Assert.That(source).DoesNotContain("ReflectionBinding");
    }

    [Arguments("MainView", "Items", "SelectedItem", "AddItemAsyncCommand", "OpenSettingCommand")]
    [Arguments("AddBrandView", "Name", "Icon", "SelectIconAsyncCommand", "AddBrandAsyncCommand")]
    [Arguments("AddProductView", "Name", "Path", "SelectFileAsyncCommand", "AddProductAsyncCommand")]
    [Arguments("SettingView", "SelectedLanguage", "SelectedTheme", "ApplyAsyncCommand", "OkAsyncCommand")]
    [Arguments("MessageView", "Message", "Details", "IsShowDetails", "CloseCommand")]
    [Test]
    public async Task View_ExposesRequiredFunctionalBindings(
        string viewName,
        string first,
        string second,
        string third,
        string fourth)
    {
        var source = await File.ReadAllTextAsync(Path.Combine(ViewsDirectory, $"{viewName}.axaml"));

        foreach (var binding in new[] { first, second, third, fourth })
        {
            await Assert.That(source).Contains($"{{Binding {binding}");
        }
    }
}
