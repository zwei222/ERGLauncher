namespace ERGLauncher.Views.Tests;

public class MainViewInputContractTests
{
    private static readonly string MainViewCodePath = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "../../../../../src/ERGLauncher/Views/MainView.axaml.cs"));

    private static async Task<string> ReadMainViewCodeAsync() =>
        await File.ReadAllTextAsync(MainViewCodePath);

    [Test]
    public async Task SelectionChangedHandlerDoesNotExecuteItemCommand()
    {
        var source = await ReadMainViewCodeAsync();
        var handlerStart = source.IndexOf("private void OnListSelectionChanged", StringComparison.Ordinal);
        var nextMethod = source.IndexOf("private void OnPointerPressed", handlerStart, StringComparison.Ordinal);
        var handler = source[handlerStart..nextMethod];

        await Assert.That(handler).Contains("private void OnListSelectionChanged");
        await Assert.That(handler).DoesNotContain("SelectItemAsyncCommand.Execute");
    }

    [Test]
    public async Task PointerActivationUsesOnlyLeftButtonForListItems()
    {
        var source = await ReadMainViewCodeAsync();

        await Assert.That(source).Contains("AddHandler(InputElement.PointerPressedEvent");
        await Assert.That(source).Contains("RoutingStrategies.Bubble");
        await Assert.That(source).Contains("handledEventsToo: true");
        await Assert.That(source).Contains("PointerUpdateKind.LeftButtonPressed");
        await Assert.That(source).Contains("FindAncestorOfType<ListBoxItem>(includeSelf: true)");
        await Assert.That(source).Contains("Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(this)");
        await Assert.That(source).Contains("SelectItemAsyncCommand.CanExecute(item)");
    }

    [Test]
    public async Task PointerSideButtonsNavigateWithoutActivatingItems()
    {
        var source = await ReadMainViewCodeAsync();

        await Assert.That(source).Contains("PointerUpdateKind.XButton1Pressed");
        await Assert.That(source).Contains("PointerUpdateKind.XButton2Pressed");
        await Assert.That(source).Contains("BackCommand.CanExecute(null)");
        await Assert.That(source).Contains("ForwardCommand.CanExecute(null)");
    }

    [Test]
    public async Task KeyboardActivationAndNavigationUseCommandsAndHandleOnlyProcessedKeys()
    {
        var source = await ReadMainViewCodeAsync();

        await Assert.That(source).Contains("AddHandler(InputElement.KeyDownEvent, OnNavigationKeyDown");
        await Assert.That(source).Contains("AddHandler(InputElement.KeyDownEvent, OnActivationKeyDown");
        await Assert.That(source).Contains("handledEventsToo: true");
        await Assert.That(source).Contains("Key.Enter");
        await Assert.That(source).Contains("Key.Space");
        await Assert.That(source).Contains("Key.BrowserBack");
        await Assert.That(source).Contains("Key.BrowserForward");
        await Assert.That(source).Contains("Key.Left");
        await Assert.That(source).Contains("Key.Right");
        await Assert.That(source).Contains("e.KeyModifiers == KeyModifiers.Alt");
        await Assert.That(source).DoesNotContain("protected override void OnKeyDown");
        await Assert.That(source).Contains("e.Handled = true");
    }

    [Test]
    public async Task PointerAndKeyboardHandlersAreRestrictedToTheirIntendedInputKinds()
    {
        var source = await ReadMainViewCodeAsync();

        await Assert.That(source).Contains("PointerUpdateKind.LeftButtonPressed");
        await Assert.That(source).Contains("PointerUpdateKind.XButton1Pressed");
        await Assert.That(source).Contains("PointerUpdateKind.XButton2Pressed");
        await Assert.That(source).Contains("KeyModifiers.None");
        await Assert.That(source).Contains("source is not Visual");
        await Assert.That(source).Contains("IsDescendantOfMainList(listBoxItem)");
    }
}
