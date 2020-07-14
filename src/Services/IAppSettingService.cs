using System.Threading.Tasks;
using ERGLauncher.Core;

namespace ERGLauncher.Services
{
    /// <summary>
    /// Application setting service interface.
    /// </summary>
    public interface IAppSettingService
    {
        /// <summary>
        /// Load application settings asynchronously.
        /// </summary>
        /// <returns>Application settings</returns>
        public ValueTask<AppSettings?> LoadAppSettingAsync();

        /// <summary>
        /// Save application settings asynchronously.
        /// </summary>
        /// <param name="settings">Application settings</param>
        /// <returns>Task</returns>
        public ValueTask SaveAppSettingAsync(AppSettings settings);
    }
}
