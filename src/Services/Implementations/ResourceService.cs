using System.Globalization;
using Gu.Localization;

namespace ERGLauncher.Services.Implementations
{
    /// <summary>
    /// Resource service.
    /// </summary>
    public class ResourceService : IResourceService
    {
        /// <inheritdoc />
        public CultureInfo CurrentCulture => Translator.CurrentCulture;

        /// <inheritdoc />
        public void ChangeCulture(CultureInfo? culture)
        {
            Translator.Culture = culture;
        }

        /// <inheritdoc />
        public string? GetCultureString(string key)
        {
            return Translator.Translate(Properties.Resources.ResourceManager, key, this.CurrentCulture);
        }
    }
}
