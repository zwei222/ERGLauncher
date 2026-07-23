using System.Threading.Tasks;

namespace ERGLauncher.Services;

public interface IViewDialogService
{
    ValueTask<ViewDialogResult> ShowDialogAsync(string dialogName, object? parameter = null);
}

public readonly record struct ViewDialogResult(bool Accepted, object? Value = null);
