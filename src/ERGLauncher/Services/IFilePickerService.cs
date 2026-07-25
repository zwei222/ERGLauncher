using System.Threading.Tasks;

namespace ERGLauncher.Services;

/// <summary>
/// Abstracts native file-open pickers so dialog view models remain testable and
/// decoupled from Avalonia's storage provider.
/// </summary>
public interface IFilePickerService
{
    /// <summary>
    /// Prompts the user to pick a single file. Returns the selected path or
    /// <see langword="null"/> when the user cancels.
    /// </summary>
    /// <param name="title">Localized dialog title.</param>
    /// <param name="patterns">Glob patterns to filter (e.g. "*.png"). Null means all files.</param>
    Task<string?> PickFileAsync(string? title, string[]? patterns = null);
}
