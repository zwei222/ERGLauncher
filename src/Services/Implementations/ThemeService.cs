using System.Threading.Tasks;
using System.Windows;
using ControlzEx.Theming;

namespace ERGLauncher.Services.Implementations
{
    /// <summary>
    /// Theme service.
    /// </summary>
    public class ThemeService : IThemeService
    {
        /// <summary>
        /// Dispatcher service.
        /// </summary>
        private readonly IDispatcherService dispatcherService;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dispatcherService">Dispatcher service</param>
        public ThemeService(IDispatcherService dispatcherService)
        {
            this.dispatcherService = dispatcherService;
            this.ChangeThemeAsync(Core.Theme.Sync).AsTask().Wait();
        }

        /// <inheritdoc />
        public Core.Theme CurrentTheme { get; private set; }

        /// <inheritdoc />
        public async ValueTask ChangeThemeAsync(Core.Theme theme)
        {
            await this.dispatcherService.SafeAction(() =>
            {
                switch (theme)
                {
                    case Core.Theme.Light:
                        ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");
                        break;
                    case Core.Theme.Dark:
                        ThemeManager.Current.ChangeTheme(Application.Current, "Dark.Blue");
                        break;
                    default:
                        ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
                        ThemeManager.Current.SyncTheme();
                        break;
                }

                this.CurrentTheme = theme;
            }).ConfigureAwait(false);
        }
    }
}
