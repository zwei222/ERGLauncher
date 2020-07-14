using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using ERGLauncher.Core;
using ERGLauncher.Services;
using Prism.Mvvm;

namespace ERGLauncher.Models.Implementations
{
    /// <summary>
    /// Base model.
    /// </summary>
    public abstract class ModelBase : BindableBase, IModelBase
    {
        /// <summary>
        /// Resource service.
        /// </summary>
        private readonly IResourceService resourceService;

        /// <summary>
        /// Theme service.
        /// </summary>
        private readonly IThemeService themeService;

        /// <summary>
        /// Title.
        /// </summary>
        private string? title;

        /// <summary>
        /// Window height.
        /// </summary>
        private double height;

        /// <summary>
        /// Window width.
        /// </summary>
        private double width;

        /// <summary>
        /// Window top position.
        /// </summary>
        private double top;

        /// <summary>
        /// Window left position.
        /// </summary>
        private double left;

        /// <summary>
        /// Window state.
        /// </summary>
        private WindowState windowState;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resourceService">Resource service</param>
        /// <param name="themeService">Theme service</param>
        protected ModelBase(IResourceService resourceService, IThemeService themeService)
        {
            this.resourceService = resourceService;
            this.themeService = themeService;
        }

        /// <inheritdoc />
        public string? Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        /// <inheritdoc />
        public double Height
        {
            get => this.height;
            set => this.SetProperty(ref this.height, value);
        }

        /// <inheritdoc />
        public double Width
        {
            get => this.width;
            set => this.SetProperty(ref this.width, value);
        }

        /// <inheritdoc />
        public double Top
        {
            get => this.top;
            set => this.SetProperty(ref this.top, value);
        }

        /// <inheritdoc />
        public double Left
        {
            get => this.left;
            set => this.SetProperty(ref this.left, value);
        }

        /// <inheritdoc />
        public WindowState WindowState
        {
            get => this.windowState;
            set => this.SetProperty(ref this.windowState, value);
        }

        /// <inheritdoc />
        public CultureInfo CurrentCulture => this.resourceService.CurrentCulture;

        /// <inheritdoc />
        public Theme CurrentTheme => this.themeService.CurrentTheme;

        /// <inheritdoc />
        public string GetCultureString(string key)
        {
            return this.resourceService.GetCultureString(key)!;
        }

        /// <summary>
        /// Change culture.
        /// </summary>
        /// <param name="culture">culture</param>
        protected void ChangeCulture(CultureInfo? culture)
        {
            this.resourceService.ChangeCulture(culture);
        }

        /// <summary>
        /// Change themes asynchronously.
        /// </summary>
        /// <param name="theme">theme</param>
        /// <returns>Task</returns>
        protected async ValueTask ChangeThemeAsync(Theme theme)
        {
            await this.themeService.ChangeThemeAsync(theme).ConfigureAwait(false);
        }
    }
}
