using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using ERGLauncher.Views;

namespace ERGLauncher.Views.Tests;

public static class AvaloniaTestSession
{
    private static AvaloniaUiThreadHost? _host;

    [Before(TestSession)]
    public static void Initialize()
    {
        _host = new AvaloniaUiThreadHost();
        _host.Start();
    }

    [After(TestSession)]
    public static void Shutdown()
    {
        _host?.Dispose();
        _host = null;
    }

    public static Task<T> RunAsync<T>(Func<T> callback) =>
        (_host ?? throw new InvalidOperationException("Avalonia test session is not initialized."))
            .InvokeAsync(callback);

    private sealed class AvaloniaUiThreadHost : IDisposable
    {
        private readonly CancellationTokenSource _shutdown = new();
        private readonly TaskCompletionSource<object?> _started =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<object?> _completed =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Thread _thread;

        public AvaloniaUiThreadHost()
        {
            _thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "ERGLauncher Avalonia test UI thread"
            };
        }

        public void Start()
        {
            _thread.Start();
            _started.Task.GetAwaiter().GetResult();
        }

        public Task<T> InvokeAsync<T>(Func<T> callback)
        {
            var completion = new TaskCompletionSource<T>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    completion.SetResult(callback());
                }
                catch (Exception exception)
                {
                    completion.SetException(exception);
                }
            });
            return completion.Task;
        }

        public void Dispose()
        {
            _shutdown.Cancel();
            _thread.Join();
            _shutdown.Dispose();
            _completed.Task.GetAwaiter().GetResult();
        }

        private void Run()
        {
            try
            {
                AppBuilder.Configure<Application>()
                    .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                    .SetupWithoutStarting();
                _started.TrySetResult(null);
                Dispatcher.UIThread.MainLoop(_shutdown.Token);
            }
            catch (Exception exception)
            {
                if (!_started.TrySetException(exception))
                {
                    _completed.TrySetException(exception);
                }
            }
            finally
            {
                _completed.TrySetResult(null);
            }
        }
    }
}

[NotInParallel]
public class MainViewInputContractTests
{
    private static readonly string MainViewCodePath = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "../../../../../src/ERGLauncher/Views/MainView.axaml.cs"));

    private static async Task<string> ReadMainViewCodeAsync() =>
        await File.ReadAllTextAsync(MainViewCodePath);

    [Test]
    public async Task MainViewLoadsXamlAndResolvesMainListBox()
    {
        var listBox = await AvaloniaTestSession.RunAsync(() =>
        {
            var view = new MainView();
            return view.FindControl<ListBox>("MainListBox");
        });

        await Assert.That(listBox).IsNotNull();
    }

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
        await Assert.That(source).Contains("_mainListBox = MainListBox;");
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
