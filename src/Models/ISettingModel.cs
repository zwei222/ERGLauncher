using System.Globalization;
using System.Threading.Tasks;
using ERGLauncher.Core;

namespace ERGLauncher.Models
{
    /// <summary>
    /// Setting model interface.
    /// </summary>
    public interface ISettingModel : IModelBase
    {
        /// <summary>
        /// Selected culture.
        /// </summary>
        public CultureInfo? SelectedLanguage { get; set; }

        /// <summary>
        /// Selected theme.
        /// </summary>
        public Theme SelectedTheme { get; set; }

        /// <summary>
        /// Apply the settings asynchronously.
        /// </summary>
        /// <returns>Task</returns>
        public ValueTask ApplyAsync();
    }
}
