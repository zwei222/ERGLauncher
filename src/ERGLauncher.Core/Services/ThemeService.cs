using ERGLauncher.Core.Models;

namespace ERGLauncher.Core.Services;

public sealed class ThemeService : IThemeService
{
    private readonly Func<Theme, CancellationToken, ValueTask> applyThemeAsync;

    public ThemeService(Func<Theme, CancellationToken, ValueTask>? applyThemeAsync = null)
    {
        this.applyThemeAsync = applyThemeAsync ?? ((_, _) => ValueTask.CompletedTask);
        this.CurrentTheme = Theme.Sync;
    }

    public Theme CurrentTheme { get; private set; }

    public async ValueTask ChangeThemeAsync(Theme theme, CancellationToken cancellationToken = default)
    {
        await this.applyThemeAsync(theme, cancellationToken).ConfigureAwait(false);
        this.CurrentTheme = theme;
    }
}
