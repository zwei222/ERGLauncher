using System.Globalization;

namespace ERGLauncher.Services
{
    /// <summary>
    /// Resource service interface.
    /// </summary>
    public interface IResourceService
    {
        /// <summary>
        /// Current culture.
        /// </summary>
        public CultureInfo CurrentCulture { get; }

        /// <summary>
        /// Change the culture.
        /// </summary>
        /// <param name="culture">culture</param>
        public void ChangeCulture(CultureInfo? culture);

        /// <summary>
        /// Gets the translated string for the current culture.
        /// </summary>
        /// <param name="key">Resources key</param>
        /// <returns>The translated string</returns>
        public string? GetCultureString(string key);
    }
}
