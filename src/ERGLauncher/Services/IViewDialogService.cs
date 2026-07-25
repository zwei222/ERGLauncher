using System.Threading.Tasks;

namespace ERGLauncher.Services;

/// <summary>
/// Result of a modal dialog interaction surfaced by <see cref="IViewDialogService"/>.
/// </summary>
public readonly struct DialogResult(bool accepted, object? value)
{
    public bool Accepted { get; } = accepted;

    public object? Value { get; } = value;

    public static DialogResult Cancelled { get; } = new(false, null);
}

/// <summary>
/// Displays application dialogs (Add/Edit brand or product, settings) as modal
/// windows owned by the main view, and returns the user's result.
/// </summary>
public interface IViewDialogService
{
    Task<DialogResult> ShowDialogAsync(string dialogName, object? parameter = null);
}
