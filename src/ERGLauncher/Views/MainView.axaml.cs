using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using ERGLauncher.Core;

namespace ERGLauncher.Views;

/// <summary>
/// Main launcher window.
/// </summary>
public partial class MainView : Window
{
    private readonly ListBox _mainListBox;

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
        _mainListBox = Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(this)
            .OfType<ListBox>().Single();

        AddHandler(InputElement.PointerPressedEvent, OnPointerPressed,
            RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(InputElement.KeyDownEvent, OnNavigationKeyDown, RoutingStrategies.Tunnel);
        AddHandler(InputElement.KeyDownEvent, OnActivationKeyDown,
            RoutingStrategies.Bubble, handledEventsToo: true);
    }

    private void OnListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
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

        if (updateKind != PointerUpdateKind.LeftButtonPressed ||
            !TryGetMainListItem(e.Source, out var item) ||
            DataContext is not IMainViewDataContext viewModel ||
            !viewModel.SelectItemAsyncCommand.CanExecute(item))
        {
            return;
        }

        viewModel.SelectItemAsyncCommand.Execute(item);
        e.Handled = true;
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
        if (e.Key is not (Key.Enter or Key.Space) ||
            !TryGetMainListItem(e.Source, out var item) ||
            DataContext is not IMainViewDataContext viewModel ||
            !viewModel.SelectItemAsyncCommand.CanExecute(item))
        {
            return;
        }

        viewModel.SelectItemAsyncCommand.Execute(item);
        e.Handled = true;
    }

    private bool TryGetMainListItem(object? source, out Item item)
    {
        item = null!;
        if (source is not Visual visual)
        {
            return false;
        }

        var listBoxItem = visual.FindAncestorOfType<ListBoxItem>(includeSelf: true);
        if (listBoxItem?.DataContext is not Item listItem ||
            !IsDescendantOfMainList(listBoxItem))
        {
            return false;
        }

        item = listItem;
        return true;
    }

    private bool IsDescendantOfMainList(Visual visual)
    {
        for (Visual? current = visual;
             current is not null;
             current = Avalonia.VisualTree.VisualExtensions.GetVisualParent(current))
        {
            if (ReferenceEquals(current, _mainListBox))
            {
                return true;
            }
        }

        return false;
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
}
