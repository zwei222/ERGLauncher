using System.Threading.Tasks;
using ERGLauncher.Core;

namespace ERGLauncher.Services
{
    /// <summary>
    /// Theme service interface.
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Current theme.
        /// </summary>
        public Theme CurrentTheme { get; }

        /// <summary>
        /// Change the theme asynchronously.
        /// </summary>
        /// <param name="theme">Theme</param>
        /// <returns></returns>
        public ValueTask ChangeThemeAsync(Theme theme);
    }
}
