namespace ERGLauncher.Core.Services;

/// <summary>
/// Presents model-level messages without coupling the model layer to a UI framework.
/// </summary>
public interface IDialogService
{
    ValueTask<bool> ShowConfirmationAsync(
        string title,
        string message,
        CancellationToken cancellationToken = default);

    ValueTask ShowMessageAsync(
        string title,
        string message,
        CancellationToken cancellationToken = default);
}
