using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ERGLauncher.Core;
using ERGLauncher.Services;
using Microsoft.Extensions.Logging;

namespace ERGLauncher.Models.Implementations
{
    /// <summary>
    /// Add brand model.
    /// </summary>
    public class AddBrandModel : ModelBase, IAddBrandModel
    {
        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<AddBrandModel> logger;

        /// <summary>
        /// File service.
        /// </summary>
        private readonly IFileService fileService;

        /// <summary>
        /// Brand name.
        /// </summary>
        private string name;

        /// <summary>
        /// Icon file path.
        /// </summary>
        private string? iconPath;

        /// <summary>
        /// Icon.
        /// </summary>
        private BitmapImage? icon;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="resourceService">Resource service</param>
        /// <param name="themeService">Theme service</param>
        /// <param name="fileService">File service</param>
        public AddBrandModel(
            ILogger<AddBrandModel> logger,
            IResourceService resourceService,
            IThemeService themeService,
            IFileService fileService)
            : base(resourceService, themeService)
        {
            this.logger = logger;
            this.fileService = fileService;
            this.name = string.Empty;
            this.Height = 300;
            this.Width = 300;
            this.Name = string.Empty;
            this.IconPath = string.Empty;
        }

        /// <inheritdoc />
        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        /// <inheritdoc />
        public string? IconPath
        {
            get => this.iconPath;
            set => this.SetProperty(ref this.iconPath, value);
        }

        /// <inheritdoc />
        public BitmapImage? Icon
        {
            get => this.icon;
            private set => this.SetProperty(ref this.icon, value);
        }

        /// <inheritdoc />
        public async ValueTask SelectIconAsync(string filePath)
        {
            this.IconPath = filePath;
            this.Icon = await this.fileService.CreateBitmapImageAsync(filePath).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async ValueTask<Brand> AddBrandAsync()
        {
            return new Brand(new List<Product>())
            {
                Icon = this.Icon,
                IconPath = await this.fileService.CopyIconFileAsync(this.IconPath).ConfigureAwait(false),
                Name = this.Name,
            };
        }

        /// <inheritdoc />
        public void LoadBrand(Brand brand)
        {
            if (brand == null)
            {
                return;
            }

            this.Name = brand.Name;
            this.Icon = brand.Icon;
            this.IconPath = brand.IconPath;
        }
    }
}
