using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace ERGLauncher.Services;

/// <summary>
/// Avalonia <see cref="IStorageProvider"/> backed file picker used by the dialog view models.
/// </summary>
public sealed class FilePickerService : IFilePickerService
{
    public async Task<string?> PickFileAsync(string? title, string[]? patterns = null)
    {
        var owner = ResolveOwner();
        var storageProvider = owner?.StorageProvider;
        if (storageProvider is null)
        {
            return null;
        }

        var options = new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
        };

        if (patterns is { Length: > 0 } && !(patterns.Length == 1 && patterns[0] == "*"))
        {
            options.FileTypeFilter =
            [
                new FilePickerFileType(title ?? "Files")
                {
                    Patterns = patterns,
                },
            ];
        }

        var files = await storageProvider.OpenFilePickerAsync(options).ConfigureAwait(true);
        var file = files.FirstOrDefault();
        return file?.TryGetLocalPath();
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
