extern alias MigratedCore;

using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using ICoreDialogService = MigratedCore::ERGLauncher.Core.Services.IDialogService;

namespace ERGLauncher.Services;

/// <summary>
/// Avalonia implementation of the core <c>IDialogService</c>. Presents confirmation
/// and message dialogs as modal windows owned by the main window so the model layer
/// stays UI-framework agnostic.
/// </summary>
public sealed class AvaloniaDialogService : ICoreDialogService
{
    public async ValueTask<bool> ShowConfirmationAsync(
        string title,
        string message,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await ShowAsync(title, message, showCancel: true).ConfigureAwait(true);
    }

    public async ValueTask ShowMessageAsync(
        string title,
        string message,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await ShowAsync(title, message, showCancel: false).ConfigureAwait(true);
    }

    private static async Task<bool> ShowAsync(string title, string message, bool showCancel)
    {
        var result = false;

        var okButton = new Button
        {
            Content = Properties.Resources.Ok,
            MinWidth = 90,
            IsDefault = true,
        };
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
        };

        var window = new Window
        {
            Title = title,
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            MinWidth = 320,
        };

        okButton.Click += (_, _) =>
        {
            result = true;
            window.Close();
        };

        if (showCancel)
        {
            var cancelButton = new Button
            {
                Content = Properties.Resources.Cancel,
                MinWidth = 90,
                IsCancel = true,
            };
            cancelButton.Click += (_, _) =>
            {
                result = false;
                window.Close();
            };
            buttons.Children.Add(cancelButton);
        }

        buttons.Children.Add(okButton);

        var panel = new StackPanel
        {
            Margin = new Avalonia.Thickness(20),
            Spacing = 16,
            Children =
            {
                new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 15,
                },
                buttons,
            },
        };
        window.Content = panel;

        var owner = ResolveOwner();
        if (owner is not null)
        {
            await window.ShowDialog(owner).ConfigureAwait(true);
        }
        else
        {
            window.Show();
        }

        return result;
    }

    private static Window? ResolveOwner()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }

        return null;
    }
}
