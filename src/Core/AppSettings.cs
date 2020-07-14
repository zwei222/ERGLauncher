using System.Globalization;
using ERGLauncher.Core.JsonFormatters;
using Utf8Json;

namespace ERGLauncher.Core
{
    /// <summary>
    /// Application settings.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Application culture.
        /// </summary>
        [JsonFormatter(typeof(CultureInfoJsonFormatter))]
        public CultureInfo Culture { get; set; } = CultureInfo.CurrentUICulture;

        /// <summary>
        /// Application theme.
        /// </summary>
        public Theme Theme { get; set; }
    }
}
