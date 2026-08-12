using System;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ERGLauncher.Core;

namespace ERGLauncher.Views;

/// <summary>
/// Main launcher window.
/// </summary>
public partial class MainView : Window
{
    private readonly ListBox _mainListBox;
    private INotifyPropertyChanged? _subscribedDataContext;
    private long _scrollGeneration;
    private bool _isAttached;

    public static readonly StyledProperty<ICommand?> OpenedCommandProperty =
        AvaloniaProperty.Register<MainView, ICommand?>(nameof(OpenedCommand));

    public static readonly StyledProperty<ICommand?> ClosingCommandProperty =
        AvaloniaProperty.Register<MainView, ICommand?>(nameof(ClosingCommand));

    public ICommand? OpenedCommand
    {
        get => GetValue(OpenedCommandProperty);
        set => SetValue(OpenedCommandProperty, value);
    }

    public ICommand? ClosingCommand
    {
        get => GetValue(ClosingCommandProperty);
        set => SetValue(ClosingCommandProperty, value);
    }

    public MainView()
    {
        AvaloniaXamlLoader.Load(this);
        _mainListBox = MainListBox;
        DataContextChanged += OnDataContextChanged;

        AddHandler(InputElement.PointerPressedEvent, OnPointerPressed,
            RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(InputElement.KeyDownEvent, OnNavigationKeyDown, RoutingStrategies.Tunnel);
        AddHandler(InputElement.KeyDownEvent, OnActivationKeyDown,
            RoutingStrategies.Bubble, handledEventsToo: true);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        InvalidatePendingScrolls();
        if (_isAttached)
        {
            SubscribeToDataContext();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IMainViewDataContext.SelectedItem) ||
            DataContext is not IMainViewDataContext viewModel ||
            viewModel.SelectedItem is not Item selectedItem)
        {
            return;
        }

        var generation = _scrollGeneration;
        var context = viewModel;
        var selected = selectedItem;
        Dispatcher.UIThread.Post(() =>
        {
            if (generation != _scrollGeneration ||
                !_isAttached ||
                !ReferenceEquals(DataContext, context) ||
                !ReferenceEquals(context.SelectedItem, selected))
            {
                return;
            }

            var index = context.Items.IndexOf(selected);
            if (index >= 0 && VisualRoot is not null && TopLevel.GetTopLevel(_mainListBox) is not null)
            {
                _mainListBox.ScrollIntoView(index);
            }
        });
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type != PointerType.Mouse)
        {
            return;
        }

        var updateKind = e.GetCurrentPoint(this).Properties.PointerUpdateKind;
        if (updateKind == PointerUpdateKind.XButton1Pressed)
        {
            e.Handled = ExecuteNavigationCommand(viewModel => viewModel.BackCommand);
            return;
        }

        if (updateKind == PointerUpdateKind.XButton2Pressed)
        {
            e.Handled = ExecuteNavigationCommand(viewModel => viewModel.ForwardCommand);
            return;
        }

    }

    private void OnItemTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Border { DataContext: Item item } ||
            DataContext is not IMainViewDataContext viewModel ||
            !viewModel.SelectItemAsyncCommand.CanExecute(item))
        {
            return;
        }

        viewModel.SelectItemAsyncCommand.Execute(item);
        e.Handled = true;
    }

    private void OnEditItemClick(object? sender, RoutedEventArgs e) =>
        ExecuteItemCommand(sender, static viewModel => viewModel.EditItemAsyncCommand);

    private void OnRemoveItemClick(object? sender, RoutedEventArgs e) =>
        ExecuteItemCommand(sender, static viewModel => viewModel.RemoveItemAsyncCommand);

    private void ExecuteItemCommand(object? sender, Func<IMainViewDataContext, ICommand> commandSelector)
    {
        if (sender is not MenuItem menuItem ||
            menuItem.DataContext is not Item item ||
            DataContext is not IMainViewDataContext viewModel)
        {
            return;
        }

        var command = commandSelector(viewModel);
        if (command.CanExecute(item))
        {
            command.Execute(item);
        }
    }

    private void OnNavigationKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not IMainViewDataContext viewModel)
        {
            return;
        }

        if ((e.Key == Key.BrowserBack && e.KeyModifiers == KeyModifiers.None) ||
            (e.Key == Key.Left && e.KeyModifiers == KeyModifiers.Alt))
        {
            if (viewModel.BackCommand.CanExecute(null))
            {
                viewModel.BackCommand.Execute(null);
                e.Handled = true;
            }

            return;
        }

        if ((e.Key == Key.BrowserForward && e.KeyModifiers == KeyModifiers.None) ||
            (e.Key == Key.Right && e.KeyModifiers == KeyModifiers.Alt))
        {
            if (viewModel.ForwardCommand.CanExecute(null))
            {
                viewModel.ForwardCommand.Execute(null);
                e.Handled = true;
            }
        }
    }

    private void OnActivationKeyDown(object? sender, KeyEventArgs e)
    {
        if (!MainViewKeyboardActivation.TryGetActivationItem(
                e.Key,
                e.KeyModifiers,
                e.Source,
                TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement(),
                _mainListBox,
                out var item) ||
            DataContext is not IMainViewDataContext viewModel ||
            !viewModel.SelectItemAsyncCommand.CanExecute(item))
        {
            return;
        }

        viewModel.SelectItemAsyncCommand.Execute(item);
        e.Handled = true;
    }


    private bool ExecuteNavigationCommand(Func<IMainViewDataContext, ICommand> commandSelector)
    {
        if (DataContext is IMainViewDataContext viewModel)
        {
            var command = commandSelector(viewModel);
            if (command.CanExecute(null))
            {
                command.Execute(null);
                return true;
            }
        }

        return false;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (OpenedCommand?.CanExecute(null) == true)
        {
            OpenedCommand.Execute(null);
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (ClosingCommand?.CanExecute(null) == true)
        {
            ClosingCommand.Execute(null);
        }

        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        _isAttached = false;
        InvalidatePendingScrolls();
        DetachDataContext();
        DataContextChanged -= OnDataContextChanged;
        base.OnClosed(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        InvalidatePendingScrolls();
        DetachDataContext();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        SubscribeToDataContext();
    }

    private void SubscribeToDataContext()
    {
        DetachDataContext();
        if (_isAttached && DataContext is INotifyPropertyChanged notify)
        {
            notify.PropertyChanged += OnViewModelPropertyChanged;
            _subscribedDataContext = notify;
        }
    }

    private void InvalidatePendingScrolls() => _scrollGeneration++;

    private void DetachDataContext()
    {
        if (_subscribedDataContext is not null)
        {
            _subscribedDataContext.PropertyChanged -= OnViewModelPropertyChanged;
            _subscribedDataContext = null;
        }
    }
}
