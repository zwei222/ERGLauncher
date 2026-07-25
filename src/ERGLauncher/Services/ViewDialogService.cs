extern alias MigratedCore;

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ERGLauncher.ViewModels;
using ERGLauncher.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ERGLauncher.Services;

/// <summary>
/// Hosts the application dialog views (<c>AddBrand</c>, <c>AddProduct</c>, <c>Setting</c>)
/// inside a modal <see cref="Window"/> owned by the main window, wiring each view to a
/// freshly resolved view model and returning the user's result.
/// </summary>
public sealed class ViewDialogService(IServiceProvider serviceProvider) : IViewDialogService
{
    private readonly IServiceProvider serviceProvider =
        serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public async Task<DialogResult> ShowDialogAsync(string dialogName, object? parameter = null)
    {
        var (content, viewModel) = Create(dialogName);
        using var scopedViewModel = viewModel;
        viewModel.OnDialogOpened(parameter);

        var window = new Window
        {
            Content = content,
            DataContext = viewModel,
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Title = viewModel.Title,
        };

        var completion = new TaskCompletionSource<DialogResult>();

        void OnRequestClose(object? sender, DialogCloseRequestedEventArgs e)
        {
            completion.TrySetResult(new DialogResult(e.Accepted, e.Value));
            window.Close();
        }

        viewModel.RequestClose += OnRequestClose;
        window.Closed += (_, _) =>
        {
            viewModel.RequestClose -= OnRequestClose;
            completion.TrySetResult(DialogResult.Cancelled);
        };

        var owner = ResolveOwner();
        if (owner is not null)
        {
            await window.ShowDialog(owner).ConfigureAwait(true);
        }
        else
        {
            window.Show();
        }

        return await completion.Task.ConfigureAwait(true);
    }

    private (Control Content, DialogViewModelBase ViewModel) Create(string dialogName) => dialogName switch
    {
        "AddBrand" => (new AddBrandView(), serviceProvider.GetRequiredService<AddBrandViewModel>()),
        "AddProduct" => (new AddProductView(), serviceProvider.GetRequiredService<AddProductViewModel>()),
        "Setting" => (new SettingView(), serviceProvider.GetRequiredService<SettingViewModel>()),
        "Message" => (new MessageView(), serviceProvider.GetRequiredService<MessageViewModel>()),
        _ => throw new ArgumentOutOfRangeException(nameof(dialogName), dialogName, "Unknown dialog name."),
    };

    private static Window? ResolveOwner()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }

        return null;
    }
}
