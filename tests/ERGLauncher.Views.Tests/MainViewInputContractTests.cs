using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ERGLauncher.Core;
using ERGLauncher;
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
                AppBuilder.Configure<App>()
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
    private static readonly string MainViewKeyboardActivationCodePath = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "../../../../../src/ERGLauncher/Views/MainViewKeyboardActivation.cs"));

    private static async Task<string> ReadMainViewCodeAsync() =>
        await File.ReadAllTextAsync(MainViewCodePath);

    private static async Task<string> ReadMainViewKeyboardActivationCodeAsync() =>
        await File.ReadAllTextAsync(MainViewKeyboardActivationCodePath);

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
    public async Task HeadlessLeftClickActivatesClickedItemOnceAndUpdatesSelection()
    {
        var result = await AvaloniaTestSession.RunAsync(() =>
        {
            var first = new Brand([]) { Name = "First" };
            var second = new Brand([]) { Name = "Second" };
            var dataContext = new CountingMainViewDataContext(first, second);
            var view = new MainView { DataContext = dataContext };
            PrepareView(view);

            var listBox = view.FindControl<ListBox>("MainListBox")!;
            var itemContainer = GetRealizedItem(listBox, first);
            var point = itemContainer.TranslatePoint(
                new Point(itemContainer.Bounds.Width / 2, itemContainer.Bounds.Height / 2), view)!.Value;
            view.MouseDown(point, MouseButton.Left);
            view.MouseUp(point, MouseButton.Left);

            var result = (dataContext.SelectCount, dataContext.LastSelectedItem,
                dataContext.SelectedItem, ReferenceEquals(dataContext.SelectedItem, first));
            view.Close();
            return result;
        });

        await Assert.That(result.SelectCount).IsEqualTo(1);
        await Assert.That(result.LastSelectedItem).IsSameReferenceAs(result.SelectedItem);
        await Assert.That(result.LastSelectedItem).IsTypeOf<Brand>();
        await Assert.That(result.Item4).IsTrue();
    }

    [Test]
    public async Task HeadlessWrongMouseButtonsDoNotActivateItem()
    {
        var count = await AvaloniaTestSession.RunAsync(() =>
        {
            var item = new Brand([]) { Name = "Only" };
            var dataContext = new CountingMainViewDataContext(item);
            var view = new MainView { DataContext = dataContext };
            PrepareView(view);
            var listBox = view.FindControl<ListBox>("MainListBox")!;
            var itemContainer = GetRealizedItem(listBox, item);
            var point = itemContainer.TranslatePoint(
                new Point(itemContainer.Bounds.Width / 2, itemContainer.Bounds.Height / 2), view)!.Value;
            view.MouseDown(point, MouseButton.Right);
            view.MouseUp(point, MouseButton.Right);
            view.MouseDown(point, MouseButton.Middle);
            view.MouseUp(point, MouseButton.Middle);
            var result = dataContext.SelectCount;
            view.Close();
            return result;
        });

        await Assert.That(count).IsEqualTo(0);
    }

    [Test]
    public async Task HeadlessInvalidMouseGesturesDoNotActivateItems()
    {
        var count = await AvaloniaTestSession.RunAsync(() =>
        {
            var first = new Brand([]) { Name = "First" };
            var second = new Brand([]) { Name = "Second" };
            var dataContext = new CountingMainViewDataContext(first, second);
            var view = new MainView { DataContext = dataContext };
            PrepareView(view);
            var listBox = view.FindControl<ListBox>("MainListBox")!;
            var firstPoint = GetCenter(GetRealizedItem(listBox, first), view);
            var secondPoint = GetCenter(GetRealizedItem(listBox, second), view);
            var outsidePoint = new Point(5, 5);

            view.MouseDown(outsidePoint, MouseButton.Left);
            view.MouseUp(firstPoint, MouseButton.Left);
            view.MouseDown(firstPoint, MouseButton.Left);
            view.MouseUp(secondPoint, MouseButton.Left);
            view.MouseDown(firstPoint, MouseButton.Left);
            view.MouseMove(outsidePoint);
            view.MouseUp(outsidePoint, MouseButton.Left);

            var result = dataContext.SelectCount;
            view.Close();
            return result;
        });

        await Assert.That(count).IsEqualTo(0);
    }

    [Test]
    public async Task MainViewHasNoObsoleteEmptySelectionHandler()
    {
        var source = await ReadMainViewCodeAsync();

        await Assert.That(source).DoesNotContain("OnListSelectionChanged");
    }

    [Test]
    public async Task ActivationHelperResolvesEnterAndSpaceToFocusedListBoxItems()
    {
        var result = await AvaloniaTestSession.RunAsync(() =>
        {
            var first = new Brand([]) { Name = "First" };
            var second = new Brand([]) { Name = "Second" };
            var dataContext = new CountingMainViewDataContext(first, second);
            var view = new MainView { DataContext = dataContext };
            PrepareView(view);
            var listBox = view.FindControl<ListBox>("MainListBox")!;
            var firstContainer = GetRealizedItem(listBox, first);
            var secondContainer = GetRealizedItem(listBox, second);
            firstContainer.Focus();
            var firstResolved = MainViewKeyboardActivation.TryGetActivationItem(
                Key.Enter, KeyModifiers.None, firstContainer, firstContainer, listBox, out var firstItem);
            secondContainer.Focus();
            var secondResolved = MainViewKeyboardActivation.TryGetActivationItem(
                Key.Space, KeyModifiers.None, secondContainer, secondContainer, listBox, out var secondItem);
            var result = (firstResolved, firstItemIsExact: ReferenceEquals(firstItem, first),
                secondResolved, secondItemIsExact: ReferenceEquals(secondItem, second));
            view.Close();
            return result;
        });

        await Assert.That(result.firstResolved).IsTrue();
        await Assert.That(result.firstItemIsExact).IsTrue();
        await Assert.That(result.secondResolved).IsTrue();
        await Assert.That(result.secondItemIsExact).IsTrue();
    }

    [Test]
    public async Task PointerActivationUsesOnlyLeftButtonForListItems()
    {
        var xaml = await File.ReadAllTextAsync(Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/ERGLauncher/Views/MainView.axaml")));
        var source = await ReadMainViewCodeAsync();
        await Assert.That(xaml).Contains("Tapped=\"OnItemTapped\"");
        await Assert.That(source).Contains("TappedEventArgs");
        await Assert.That(source).DoesNotContain("OnItemPointerReleased");
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
        var source = string.Concat(
            await ReadMainViewCodeAsync(),
            await ReadMainViewKeyboardActivationCodeAsync());
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
        var source = string.Concat(
            await ReadMainViewCodeAsync(),
            await ReadMainViewKeyboardActivationCodeAsync());

        await Assert.That(source).Contains("PointerUpdateKind.XButton1Pressed");
        await Assert.That(source).Contains("PointerUpdateKind.XButton2Pressed");
        await Assert.That(source).Contains("KeyModifiers.None");
        await Assert.That(source).Contains("source is not Visual");
    }

    [Test]
    public async Task ActivationHelperRejectsToolbarFocusStaleSelectionModifiedKeysAndUnrelatedSources()
    {
        var result = await AvaloniaTestSession.RunAsync(() =>
        {
            var item = new Brand([]) { Name = "Only" };
            var dataContext = new CountingMainViewDataContext(item) { SelectedItem = item };
            var view = new MainView { DataContext = dataContext };
            PrepareView(view);
            var listBox = view.FindControl<ListBox>("MainListBox")!;
            var itemContainer = GetRealizedItem(listBox, item);
            var add = view.GetVisualDescendants().OfType<Button>().First(button =>
                button.Command == dataContext.AddItemAsyncCommand);
            var modified = MainViewKeyboardActivation.TryGetActivationItem(
                Key.Enter, KeyModifiers.Control, itemContainer, itemContainer, listBox, out _);
            var toolbar = MainViewKeyboardActivation.TryGetActivationItem(
                Key.Space, KeyModifiers.None, add, add, listBox, out _);
            listBox.SelectedItem = new Brand([]) { Name = "Stale" };
            var staleSelection = MainViewKeyboardActivation.TryGetActivationItem(
                Key.Enter, KeyModifiers.None, listBox, listBox, listBox, out _);
            var unrelatedSource = MainViewKeyboardActivation.TryGetActivationItem(
                Key.Enter, KeyModifiers.None, add, add, listBox, out _);
            view.Close();
            return (modified, toolbar, staleSelection, unrelatedSource);
        });

        await Assert.That(result.modified).IsFalse();
        await Assert.That(result.toolbar).IsFalse();
        await Assert.That(result.staleSelection).IsFalse();
        await Assert.That(result.unrelatedSource).IsFalse();
    }

    [Test]
    public async Task ActivationHelperFallsBackToCurrentSelectionWhenListBoxHasFocus()
    {
        var result = await AvaloniaTestSession.RunAsync(() =>
        {
            var item = new Brand([]) { Name = "Only" };
            var dataContext = new CountingMainViewDataContext(item) { SelectedItem = item };
            var view = new MainView { DataContext = dataContext };
            PrepareView(view);
            var listBox = view.FindControl<ListBox>("MainListBox")!;
            listBox.SelectedItem = item;
            var resolved = MainViewKeyboardActivation.TryGetActivationItem(
                Key.Space, KeyModifiers.None, listBox, listBox, listBox, out var resolvedItem);
            var result = (resolved, resolvedItemIsExact: ReferenceEquals(resolvedItem, item));
            view.Close();
            return result;
        });

        await Assert.That(result.resolved).IsTrue();
        await Assert.That(result.resolvedItemIsExact).IsTrue();
    }

    [Test]
    public async Task ContextMenuActionsReceiveTheirOwningItemWhenSelectionDiffers()
    {
        var result = await AvaloniaTestSession.RunAsync(() =>
        {
            var first = new Brand([]) { Name = "First" };
            var second = new Brand([]) { Name = "Second" };
            var dataContext = new CountingMainViewDataContext(first, second);
            dataContext.SelectedItem = first;
            var view = new MainView { DataContext = dataContext };
            PrepareView(view);
            var listBox = view.FindControl<ListBox>("MainListBox")!;
            var secondContainer = GetRealizedItem(listBox, second);
            var card = secondContainer.GetVisualDescendants().OfType<Border>()
                .Single(border => border.Classes.Contains("card"));
            var contextMenu = card.ContextMenu!;
            contextMenu.Open(card);
            Dispatcher.UIThread.RunJobs();
            var menuItems = contextMenu.Items.OfType<MenuItem>().ToArray();
            var menuDataContexts = menuItems.Select(menuItem => menuItem.DataContext).ToArray();
            var selectionBeforeActions = dataContext.SelectedItem;
            menuItems[0].RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            menuItems[1].RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            contextMenu.Close();
            var result = (dataContext.LastEditedItem, dataContext.LastRemovedItem,
                menuDataContexts, selectionBeforeActions, dataContext.SelectedItem,
                contextMenu.IsOpen, expectedItem: second);
            view.Close();
            return result;
        });

        await Assert.That(result.LastEditedItem).IsSameReferenceAs(result.expectedItem);
        await Assert.That(result.LastRemovedItem).IsSameReferenceAs(result.expectedItem);
        await Assert.That(result.LastEditedItem).IsSameReferenceAs(result.menuDataContexts[0]);
        await Assert.That(result.LastEditedItem).IsSameReferenceAs(result.menuDataContexts[1]);
        await Assert.That(result.LastEditedItem).IsTypeOf<Brand>();
        await Assert.That(result.selectionBeforeActions).IsSameReferenceAs(result.SelectedItem);
        await Assert.That(result.IsOpen).IsFalse();
    }


    private static void PrepareView(MainView view)
    {
        if (view.DataContext is IMainViewDataContext viewModel)
        {
            var listBox = view.FindControl<ListBox>("MainListBox")!;
            listBox.ItemsSource = viewModel.Items;
            listBox.ApplyTemplate();
            listBox.ScrollIntoView(0);
        }

        view.Show();
        view.ApplyTemplate();
        for (var pass = 0; pass < 5; pass++)
        {
            view.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static ListBoxItem GetRealizedItem(ListBox listBox, Item item)
    {
        var byIndex = listBox.ContainerFromIndex(listBox.Items.IndexOf(item)) as ListBoxItem;
        if (byIndex is not null)
        {
            return byIndex;
        }

        return listBox.GetVisualDescendants().OfType<ListBoxItem>()
            .Single(container => ReferenceEquals(container.DataContext, item));
    }

    private sealed class CountingMainViewDataContext : IMainViewDataContext
    {
        public CountingMainViewDataContext(params Item[] items)
        {
            Items = new ObservableCollection<Item>(items);
            SelectItemAsyncCommand = new CountingCommand(item =>
            {
                SelectCount++;
                LastSelectedItem = item as Item;
            });
            EditItemAsyncCommand = new CountingCommand(item => LastEditedItem = item as Item);
            RemoveItemAsyncCommand = new CountingCommand(item => LastRemovedItem = item as Item);
        }

        public string? Title => "Test";
        public bool IsEnabledBack => false;
        public bool IsEnabledForward => false;
        public string? CurrentBrand => null;
        public Item? SelectedItem { get; set; }
        public ObservableCollection<Item> Items { get; }
        public ICommand BackCommand { get; } = new CountingCommand();
        public ICommand ForwardCommand { get; } = new CountingCommand();
        public ICommand SelectItemAsyncCommand { get; }
        public ICommand AddItemAsyncCommand { get; } = new CountingCommand();
        public ICommand EditItemAsyncCommand { get; }
        public ICommand RemoveItemAsyncCommand { get; }
        public ICommand OpenSettingCommand { get; } = new CountingCommand();
        public ICommand LoadSettingAsyncCommand { get; } = new CountingCommand();
        public ICommand SaveAppSettingAsyncCommand { get; } = new CountingCommand();
        public int SelectCount { get; private set; }
        public Item? LastSelectedItem { get; private set; }
        public Item? LastEditedItem { get; private set; }
        public Item? LastRemovedItem { get; private set; }

    }

    private static Point GetCenter(Control control, Visual relativeTo) =>
        control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), relativeTo)!.Value;

    private sealed class CountingCommand(Action<object?>? action = null) : ICommand
    {
        private readonly Action<object?> _action = action ?? (_ => { });
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _action(parameter);
    }
}
