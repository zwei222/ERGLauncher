using System.ComponentModel;
using System.Globalization;
using System.Windows;
using ERGLauncher.Core;

namespace ERGLauncher.Models
{
    /// <summary>
    /// Base model interface.
    /// </summary>
    public interface IModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Title
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Window height.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Window width.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Window top position.
        /// </summary>
        public double Top { get; set; }

        /// <summary>
        /// Window left position.
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// Window state.
        /// </summary>
        public WindowState WindowState { get; set; }

        /// <summary>
        /// Current culture.
        /// </summary>
        public CultureInfo CurrentCulture { get; }

        /// <summary>
        /// Current theme.
        /// </summary>
        public Theme CurrentTheme { get; }

        /// <summary>
        /// Gets the translated string for the current culture.
        /// </summary>
        /// <param name="key">Resources key</param>
        /// <returns>The translated string</returns>
        public string GetCultureString(string key);
    }
}
