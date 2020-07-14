using System;
using System.Globalization;
using System.Threading.Tasks;
using ERGLauncher.Core;
using ERGLauncher.Services;
using Microsoft.Extensions.Logging;

namespace ERGLauncher.Models.Implementations
{
    /// <summary>
    /// Setting model.
    /// </summary>
    public class SettingModel : ModelBase, ISettingModel
    {
        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<SettingModel> logger;

        /// <summary>
        /// Selected culture.
        /// </summary>
        private CultureInfo? selectedLanguage;

        /// <summary>
        /// Selected theme.
        /// </summary>
        private Theme selectedTheme;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="resourceService">Resource service</param>
        /// <param name="themeService">Theme service</param>
        public SettingModel(ILogger<SettingModel> logger, IResourceService resourceService, IThemeService themeService)
            : base(resourceService, themeService)
        {
            if (resourceService == null)
            {
                throw new ArgumentNullException(nameof(resourceService));
            }

            if (themeService == null)
            {
                throw new ArgumentNullException(nameof(themeService));
            }

            this.logger = logger;
            this.Height = 300;
            this.Width = 500;
            this.SelectedLanguage = resourceService.CurrentCulture;
            this.SelectedTheme = themeService.CurrentTheme;
        }

        /// <inheritdoc />
        public CultureInfo? SelectedLanguage
        {
            get => this.selectedLanguage;
            set => this.SetProperty(ref this.selectedLanguage, value);
        }

        /// <inheritdoc />
        public Theme SelectedTheme
        {
            get => this.selectedTheme;
            set => this.SetProperty(ref this.selectedTheme, value);
        }

        /// <inheritdoc />
        public async ValueTask ApplyAsync()
        {
            this.ChangeCulture(this.SelectedLanguage);
            await this.ChangeThemeAsync(this.SelectedTheme).ConfigureAwait(false);
        }
    }
}
