using ERGLauncher.Core.Models;

namespace ERGLauncher.Core.Services;

public interface IThemeService
{
    Theme CurrentTheme { get; }

    ValueTask ChangeThemeAsync(Theme theme, CancellationToken cancellationToken = default);
}
