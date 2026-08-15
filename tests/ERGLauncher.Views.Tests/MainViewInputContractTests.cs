using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.ComponentModel;
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
    public async Task MainViewDetachesPreviousDataContextPropertyChangedSubscription()
    {
        var result = await AvaloniaTestSession.RunAsync(() =>
        {
            var oldContext = new CountingMainViewDataContext(new Brand([]) { Name = "Old" });
            var newContext = new CountingMainViewDataContext(new Brand([]) { Name = "New" });
            var view = new MainView();

            var subscriptionsBeforeAssignment = oldContext.PropertyChangedSubscriberCount;
            view.DataContext = oldContext;
            view.Show();
            view.ApplyTemplate();
            view.UpdateLayout();
            var subscriptionsAfterAttach = oldContext.PropertyChangedSubscriberCount;

            view.DataContext = newContext;
            var oldSubscriptionAfterReplacement = oldContext.PropertyChangedSubscriberCount;
            var newSubscriptionAfterReplacement = newContext.PropertyChangedSubscriberCount;
            oldContext.RaiseSelectedItemChanged();
            newContext.RaiseSelectedItemChanged();

            view.DataContext = null;
            return (oldSubscriptionAfterReplacement, newSubscriptionAfterReplacement,
                oldContext.PropertyChangedSubscriberCount,
                newContext.PropertyChangedSubscriberCount,
                subscriptionsBeforeAssignment,
                subscriptionsAfterAttach);
        });

        await Assert.That(result.Item1).IsEqualTo(0);
        await Assert.That(result.Item2).IsEqualTo(1);
        await Assert.That(result.Item3).IsEqualTo(0);
        await Assert.That(result.Item4).IsEqualTo(0);
        await Assert.That(result.Item5).IsEqualTo(0);
        await Assert.That(result.Item6).IsEqualTo(1);
    }

    [Test]
    public async Task MainViewScrollsRestoredSelectionIntoView()
    {
        var result = await AvaloniaTestSession.RunAsync(() =>
        {
            var items = Enumerable.Range(0, 40)
                .Select(index => (Item)new Brand([]) { Name = $"Item {index}" })
                .ToArray();
            var dataContext = new CountingMainViewDataContext(items);
            var view = new MainView { DataContext = dataContext };
            PrepareView(view);

            var listBox = view.FindControl<ListBox>("MainListBox")!;
            var selectedItem = items[^1];
            dataContext.SelectedItem = selectedItem;
            Dispatcher.UIThread.RunJobs();
            view.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            view.UpdateLayout();

            var container = GetRealizedItem(listBox, selectedItem);
            var topLeft = container.TranslatePoint(new Point(0, 0), listBox)!.Value;
            var bottomRight = container.TranslatePoint(
                new Point(container.Bounds.Width, container.Bounds.Height), listBox)!.Value;
            var visible = topLeft.Y >= 0 && bottomRight.Y <= listBox.Bounds.Height;
            view.Close();
            return (visible, listBox.Bounds.Height, topLeft.Y, bottomRight.Y);
        });

        await Assert.That(result.visible).IsTrue();
        await Assert.That(result.Height).IsGreaterThan(0);
    }

    [Test]
    public async Task MainViewResubscribesAndResumesScrollingAfterDetachAndReattach()
    {
        var result = await AvaloniaTestSession.RunAsync(() =>
        {
            var items = Enumerable.Range(0, 40)
                .Select(index => (Item)new Brand([]) { Name = $"Item {index}" })
                .ToArray();
            var dataContext = new CountingMainViewDataContext(items);
            var view = new MainView { DataContext = dataContext };
            PrepareView(view);
            view.Close();
            var subscriptionsWhileDetached = dataContext.PropertyChangedSubscriberCount;
            var reattachedView = new MainView { DataContext = dataContext };
            PrepareView(reattachedView);
            var reattachedListBox = reattachedView.FindControl<ListBox>("MainListBox")!;
            var subscriptionsAfterReattach = dataContext.PropertyChangedSubscriberCount;

            dataContext.SelectedItem = items[^1];
            Dispatcher.UIThread.RunJobs();
            view.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            var container = GetRealizedItem(reattachedListBox, items[^1]);
            var topLeft = container.TranslatePoint(new Point(0, 0), reattachedListBox)!.Value;
            var bottomRight = container.TranslatePoint(
                new Point(container.Bounds.Width, container.Bounds.Height), reattachedListBox)!.Value;
            var visible = topLeft.Y >= 0 && bottomRight.Y <= reattachedListBox.Bounds.Height;
            reattachedView.Close();
            return (subscriptionsWhileDetached, subscriptionsAfterReattach, visible);
        });

        await Assert.That(result.subscriptionsWhileDetached).IsEqualTo(0);
        await Assert.That(result.subscriptionsAfterReattach).IsEqualTo(1);
        await Assert.That(result.visible).IsTrue();
    }

    [Test]
    public async Task MainViewSuppressesQueuedScrollsFromReplacedContextAndDetachedView()
    {
        var result = await AvaloniaTestSession.RunAsync(() =>
        {
            static (double offset, bool oldItemVisible, bool currentItemVisible) Inspect(
                MainView view, ListBox listBox, Item oldItem, Item currentItem)
            {
                view.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                view.UpdateLayout();
                var scrollViewer = listBox.GetVisualDescendants().OfType<ScrollViewer>().Single();
                var oldContainer = listBox.ContainerFromIndex(listBox.Items.IndexOf(oldItem)) as ListBoxItem;
                var currentContainer = listBox.ContainerFromIndex(listBox.Items.IndexOf(currentItem)) as ListBoxItem;
                return (scrollViewer.Offset.Y,
                    oldContainer is not null && oldContainer.Bounds.Top >= 0 &&
                    oldContainer.Bounds.Bottom <= listBox.Bounds.Height,
                    currentContainer is not null && currentContainer.Bounds.Top >= 0 &&
                    currentContainer.Bounds.Bottom <= listBox.Bounds.Height);
            }

            var oldItems = Enumerable.Range(0, 40)
                .Select(index => (Item)new Brand([]) { Name = $"Old {index}" }).ToArray();
            var currentItems = Enumerable.Range(0, 40)
                .Select(index => (Item)new Brand([]) { Name = $"Current {index}" }).ToArray();
            var oldContext = new CountingMainViewDataContext(oldItems);
            var currentContext = new CountingMainViewDataContext(currentItems);
            var view = new MainView { DataContext = oldContext };
            PrepareView(view);
            var listBox = view.FindControl<ListBox>("MainListBox")!;

            oldContext.SelectedItem = oldItems[^1];
            oldContext.RaiseSelectedItemChanged();
            view.DataContext = currentContext;
            listBox.ItemsSource = currentContext.Items;
            Dispatcher.UIThread.RunJobs();
            view.UpdateLayout();
            var afterReplacement = Inspect(view, listBox, oldItems[^1], currentItems[^1]);
            view.Close();

            var currentView = new MainView { DataContext = currentContext };
            PrepareView(currentView);
            var currentListBox = currentView.FindControl<ListBox>("MainListBox")!;
            currentContext.SelectedItem = currentItems[^1];
            for (var pass = 0; pass < 3; pass++)
            {
                currentView.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
            }
            var afterCurrentSelection = Inspect(currentView, currentListBox, oldItems[^1], currentItems[^1]);
            var currentContextSubscriberCount = currentContext.PropertyChangedSubscriberCount;
            currentView.Close();

            var detachedContext = new CountingMainViewDataContext(oldItems);
            var detachedView = new MainView { DataContext = detachedContext };
            PrepareView(detachedView);
            detachedContext.SelectedItem = oldItems[^1];
            detachedView.Close();
            Dispatcher.UIThread.RunJobs();
            var afterClose = detachedContext.PropertyChangedSubscriberCount;

            return (afterReplacement, afterCurrentSelection, afterClose, currentContextSubscriberCount);
        });

        await Assert.That(result.afterReplacement.oldItemVisible).IsFalse();
        await Assert.That(result.afterReplacement.currentItemVisible).IsFalse();
        await Assert.That(result.afterCurrentSelection.offset).IsGreaterThan(0);
        await Assert.That(result.afterClose).IsEqualTo(0);
        await Assert.That(result.currentContextSubscriberCount).IsEqualTo(1);
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
            var settings = view.GetVisualDescendants().OfType<Button>().First(button =>
                button.Command == dataContext.OpenSettingCommand);
            var modified = MainViewKeyboardActivation.TryGetActivationItem(
                Key.Enter, KeyModifiers.Control, itemContainer, itemContainer, listBox, out _);
            var toolbar = MainViewKeyboardActivation.TryGetActivationItem(
                Key.Space, KeyModifiers.None, settings, settings, listBox, out _);
            listBox.SelectedItem = new Brand([]) { Name = "Stale" };
            var staleSelection = MainViewKeyboardActivation.TryGetActivationItem(
                Key.Enter, KeyModifiers.None, listBox, listBox, listBox, out _);
            var unrelatedSource = MainViewKeyboardActivation.TryGetActivationItem(
                Key.Enter, KeyModifiers.None, settings, settings, listBox, out _);
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

    [Test]
    public async Task ListContextMenuResolvesAndExecutesAddCommandFromCurrentDataContext()
    {
        var result = await AvaloniaTestSession.RunAsync(() =>
        {
            var dataContext = new CountingMainViewDataContext(new Brand([]) { Name = "Only" });
            var view = new MainView { DataContext = dataContext };
            PrepareView(view);

            var listBox = view.FindControl<ListBox>("MainListBox")!;
            var contextMenu = listBox.ContextMenu!;
            contextMenu.Open(listBox);
            Dispatcher.UIThread.RunJobs();

            var add = contextMenu.Items.OfType<MenuItem>().Single();
            var commandResolved = ReferenceEquals(add.Command, dataContext.AddItemAsyncCommand);
            add.Command!.Execute(add.CommandParameter);
            var result = (commandResolved, dataContext.AddCount, contextMenu.IsOpen);
            contextMenu.Close();
            view.Close();
            return result;
        });

        await Assert.That(result.commandResolved).IsTrue();
        await Assert.That(result.AddCount).IsEqualTo(1);
        await Assert.That(result.IsOpen).IsTrue();
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

    private sealed class CountingMainViewDataContext : IMainViewDataContext, INotifyPropertyChanged
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
            AddItemAsyncCommand = new CountingCommand(_ => AddCount++);
        }

        public string? Title => "Test";
        public bool IsEnabledBack => false;
        public bool IsEnabledForward => false;
        public string? CurrentBrand => null;
        public bool IsLoading => false;
        private Item? selectedItem;
        public Item? SelectedItem
        {
            get => selectedItem;
            set
            {
                if (ReferenceEquals(selectedItem, value))
                {
                    return;
                }

                selectedItem = value;
                propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add
            {
                propertyChanged += value;
                if (value?.Method.Name == "OnViewModelPropertyChanged")
                {
                    propertyChangedSubscriberCount++;
                }
            }
            remove
            {
                propertyChanged -= value;
                if (value?.Method.Name == "OnViewModelPropertyChanged")
                {
                    propertyChangedSubscriberCount--;
                }
            }
        }

        private PropertyChangedEventHandler? propertyChanged;
        private int propertyChangedSubscriberCount;
        public int PropertyChangedSubscriberCount => propertyChangedSubscriberCount;
        public void RaiseSelectedItemChanged() => propertyChanged?.Invoke(
            this, new PropertyChangedEventArgs(nameof(SelectedItem)));
        public ObservableCollection<Item> Items { get; }
        public ICommand BackCommand { get; } = new CountingCommand();
        public ICommand ForwardCommand { get; } = new CountingCommand();
        public ICommand SelectItemAsyncCommand { get; }
        public ICommand AddItemAsyncCommand { get; }
        public ICommand EditItemAsyncCommand { get; }
        public ICommand RemoveItemAsyncCommand { get; }
        public ICommand OpenSettingCommand { get; } = new CountingCommand();
        public ICommand LoadSettingAsyncCommand { get; } = new CountingCommand();
        public ICommand SaveAppSettingAsyncCommand { get; } = new CountingCommand();
        public int SelectCount { get; private set; }
        public Item? LastSelectedItem { get; private set; }
        public Item? LastEditedItem { get; private set; }
        public Item? LastRemovedItem { get; private set; }
        public int AddCount { get; private set; }

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
