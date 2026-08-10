using System.Buffers.Binary;
using System.Xml.Linq;

namespace ERGLauncher.Views.Tests;

public class ViewContractTests
{
    private static readonly string ViewsDirectory = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "../../../../../src/ERGLauncher/Views"));
    private static readonly string ProjectDirectory = Path.GetFullPath(Path.Combine(
        ViewsDirectory,
        ".."));

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
    public async Task MainView_UsesOneContentTitleHeadingAndOptionalBrandContext()
    {
        var document = XDocument.Load(Path.Combine(ViewsDirectory, "MainView.axaml"));
        XNamespace avalonia = "https://github.com/avaloniaui";
        var contentHeader = document.Descendants(avalonia + "Grid")
            .Single(grid => grid.Attribute("Grid.Row")?.Value == "1");
        var headerTextBlocks = contentHeader.Descendants(avalonia + "TextBlock").ToArray();

        await Assert.That(document.Root!.Attribute("Title")?.Value).IsEqualTo("ERG Launcher");
        await Assert.That(headerTextBlocks.Count(textBlock =>
                textBlock.Attribute("Text")?.Value == "{Binding Title}"))
            .IsEqualTo(1);
        await Assert.That(headerTextBlocks.Count(textBlock =>
                textBlock.Attribute("Text")?.Value == "{Binding CurrentBrand}"))
            .IsEqualTo(1);
        await Assert.That(document.Descendants(avalonia + "TextBlock")
                .Count(textBlock => textBlock.Attribute("Text")?.Value == "ERG Launcher"))
            .IsEqualTo(0);
        await Assert.That(document.Descendants(avalonia + "TextBlock")
                .Count(textBlock => textBlock.Attribute("Text")?.Value == "{Binding Title}"))
            .IsEqualTo(1);
    }

    [Test]
    public async Task MainView_UsesIcoAndNeverPngAsWindowIcon()
    {
        var source = await File.ReadAllTextAsync(Path.Combine(ViewsDirectory, "MainView.axaml"));

        await Assert.That(source).Contains("Icon=\"avares://ERGLauncher/Assets/icon.ico\"");
        await Assert.That(source).DoesNotContain("Icon=\"avares://ERGLauncher/Assets/icon.png\"");
    }

    [Test]
    public async Task Project_DeclaresIconResourcesAndVersionContract()
    {
        var project = await File.ReadAllTextAsync(Path.Combine(ProjectDirectory, "ERGLauncher.csproj"));

        await Assert.That(project).Contains("<ApplicationIcon>Assets/icon.ico</ApplicationIcon>");
        await Assert.That(project).Contains("<AvaloniaResource Include=\"Assets/**\" />");
        await Assert.That(project).Contains("<Content Include=\"Assets/icon.png\"");
        await Assert.That(project).Contains("CopyToOutputDirectory=\"PreserveNewest\"");
        await Assert.That(project).Contains("CopyToPublishDirectory=\"PreserveNewest\"");
        await Assert.That(project).Contains("<Version>2.0.0</Version>");
        await Assert.That(project).Contains("<AssemblyVersion>2.0.0.0</AssemblyVersion>");
        await Assert.That(project).Contains("<FileVersion>2.0.0.0</FileVersion>");
        await Assert.That(project).Contains("<InformationalVersion>2.0.0</InformationalVersion>");
        var pngPath = Path.Combine(ProjectDirectory, "Assets", "icon.png");
        var icoPath = Path.Combine(ProjectDirectory, "Assets", "icon.ico");
        await Assert.That(File.Exists(pngPath)).IsTrue();
        await Assert.That(File.Exists(icoPath)).IsTrue();
        await Assert.That(new FileInfo(pngPath).Length).IsGreaterThan(0L);

        var ico = await File.ReadAllBytesAsync(icoPath);
        await Assert.That(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ico)).ToLowerInvariant())
            .IsEqualTo("21cb42e92a6b9aead1cb0057e4428ffc722f650408c3db120a18ac50503c1672");
        await Assert.That(ico.Length).IsGreaterThanOrEqualTo(6);
        await Assert.That(BinaryPrimitives.ReadUInt16LittleEndian(ico.AsSpan(0, 2))).IsEqualTo((ushort)0);
        await Assert.That(BinaryPrimitives.ReadUInt16LittleEndian(ico.AsSpan(2, 2))).IsEqualTo((ushort)1);
        await Assert.That(BinaryPrimitives.ReadUInt16LittleEndian(ico.AsSpan(4, 2))).IsEqualTo((ushort)6);
        await Assert.That(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(await File.ReadAllBytesAsync(pngPath))).ToLowerInvariant())
            .IsEqualTo("50a705ed9e94924bdd95908123e35e8d2ec887927bc3dce04d1bdf8bf9551a84");
    }

    [Test]
    public async Task AppDialogControlCentersAllButtonContent()
    {
        var source = await File.ReadAllTextAsync(Path.Combine(ProjectDirectory, "App.axaml"));

        await Assert.That(source).Contains("<Style Selector=\"Button.dialog-control\">");
        await Assert.That(source).Contains("<Setter Property=\"HorizontalContentAlignment\" Value=\"Center\" />");
        await Assert.That(source).Contains("<Setter Property=\"VerticalContentAlignment\" Value=\"Center\" />");
    }

    [Test]
    public async Task SettingsButtonUsesNewAccessibleGearPath()
    {
        var source = await File.ReadAllTextAsync(Path.Combine(ViewsDirectory, "MainView.axaml"));
        const string oldRadialPath = "M12,8 A4,4 0 1 0 12,16 A4,4 0 1 0 12,8 M4.93,4.93";
        const string gearPath = "M19.43,12.98 C19.47,12.66";

        await Assert.That(source).Contains(gearPath);
        await Assert.That(source).DoesNotContain(oldRadialPath);
        await Assert.That(source).Contains("Command=\"{Binding OpenSettingCommand}\"");
        await Assert.That(source).Contains("ToolTip.Tip=\"{x:Static properties:Resources.Setting}\"");
        await Assert.That(source).Contains("AutomationProperties.Name=\"{x:Static properties:Resources.Setting}\"");
        await Assert.That(source).Contains("Width=\"18\" Height=\"18\"");
    }

    [Test]
    public async Task WindowsManifest_UsesMajorVersionTwoIdentity()
    {
        var manifest = await File.ReadAllTextAsync(Path.Combine(ProjectDirectory, "app.manifest"));

        await Assert.That(manifest).Contains("version=\"2.0.0.0\"");
    }

    [Test]
    public async Task MainView_UsesCompiledBindingsForNativeAot()
    {
        var source = await File.ReadAllTextAsync(Path.Combine(ViewsDirectory, "MainView.axaml"));

        await Assert.That(source).Contains("x:CompileBindings=\"True\"");
        await Assert.That(source).Contains("x:DataType=\"views:IMainViewDataContext\"");
        await Assert.That(source).DoesNotContain("ReflectionBinding");
    }

    [Test]
    public async Task MainView_UsesListSelectionAsTheSingleItemActivationSurface()
    {
        var document = XDocument.Load(Path.Combine(ViewsDirectory, "MainView.axaml"));
        XNamespace avalonia = "https://github.com/avaloniaui";
        var listBox = document.Descendants(avalonia + "ListBox").Single();
        var itemTemplate = listBox.Descendants(avalonia + "DataTemplate").Single();

        await Assert.That(listBox.Attribute("SelectedItem")?.Value)
            .IsEqualTo("{Binding SelectedItem, Mode=TwoWay}");
        await Assert.That(itemTemplate.Descendants(avalonia + "Button")).IsEmpty();
    }

    [Test]
    public async Task MainView_SeparatesPrimaryAndSecondaryActionsAndKeepsScrollBarAtListEdge()
    {
        var source = await File.ReadAllTextAsync(Path.Combine(ViewsDirectory, "MainView.axaml"));

        await Assert.That(source).Contains("<MenuItem Header=\"{x:Static properties:Resources.Edit}\"");
        await Assert.That(source).Contains("<MenuItem Header=\"{x:Static properties:Resources.Remove}\"");
        await Assert.That(source).Contains("AutomationProperties.Name=");
        await Assert.That(source).Contains("ToolTip.Tip=");
        await Assert.That(source).Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"");
        await Assert.That(source).Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Disabled\"");
        await Assert.That(source).DoesNotContain("<Border Grid.Row=\"2\"");
    }

    [Test]
    public async Task MainView_UsesAccessibleLocalizedIconsForToolbarActions()
    {
        var source = await File.ReadAllTextAsync(Path.Combine(ViewsDirectory, "MainView.axaml"));

        await Assert.That(source).Contains("<PathIcon");
        foreach (var command in new[]
                 {
                     "BackCommand",
                     "ForwardCommand",
                     "OpenSettingCommand",
                     "AddItemAsyncCommand",
                 })
        {
            await Assert.That(source).Contains($"Command=\"{{Binding {command}}}\"");
        }

        await Assert.That(source).DoesNotContain("Content=\"←\"");
        await Assert.That(source).DoesNotContain("Content=\"→\"");
        await Assert.That(source).DoesNotContain("Content=\"⋯\"");
    }

    [Test]
    public async Task MainView_UsesPerItemContextMenuForSecondaryItemActions()
    {
        var source = await File.ReadAllTextAsync(Path.Combine(ViewsDirectory, "MainView.axaml"));

        await Assert.That(source).DoesNotContain("SelectionChanged=\"OnListSelectionChanged\"");
        await Assert.That(source).DoesNotContain("MoreActions");
        await Assert.That(source).Contains("<ContextMenu>");
        await Assert.That(source).Contains("Click=\"OnEditItemClick\"");
        await Assert.That(source).Contains("Click=\"OnRemoveItemClick\"");
        await Assert.That(source).Contains("HorizontalScrollBarVisibility=\"Disabled\"");
        await Assert.That(source).Contains("VerticalScrollBarVisibility=\"Auto\"");
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
