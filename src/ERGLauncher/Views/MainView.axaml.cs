using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Markup.Xaml;
using ERGLauncher.Core;

namespace ERGLauncher.Views;

/// <summary>
/// Main launcher window.
/// </summary>
public partial class MainView : Window
{
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
    }

    private void OnListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var item = e.AddedItems.OfType<Item>().FirstOrDefault();
        if (item is not null && DataContext is IMainViewDataContext viewModel &&
            viewModel.SelectItemAsyncCommand.CanExecute(item))
        {
            viewModel.SelectItemAsyncCommand.Execute(item);
        }
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
