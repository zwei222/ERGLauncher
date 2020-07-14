using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ERGLauncher.Core;
using ERGLauncher.Services;
using Microsoft.Extensions.Logging;

namespace ERGLauncher.Models.Implementations
{
    /// <summary>
    /// Add product model.
    /// </summary>
    public class AddProductModel : ModelBase, IAddProductModel
    {
        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<AddProductModel> logger;

        /// <summary>
        /// File service.
        /// </summary>
        private readonly IFileService fileService;

        /// <summary>
        /// Product name.
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
        /// Executable file path.
        /// </summary>
        private string path;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="resourceService">Resource service</param>
        /// <param name="themeService">Theme service</param>
        /// <param name="fileService">File service</param>
        public AddProductModel(
            ILogger<AddProductModel> logger,
            IResourceService resourceService,
            IThemeService themeService,
            IFileService fileService)
            : base(resourceService, themeService)
        {
            this.logger = logger;
            this.fileService = fileService;
            this.name = string.Empty;
            this.path = string.Empty;
            this.Height = 400;
            this.Width = 300;
            this.Name = string.Empty;
            this.IconPath = string.Empty;
            this.Path = string.Empty;
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
        public string Path
        {
            get => this.path;
            set => this.SetProperty(ref this.path, value);
        }

        /// <inheritdoc />
        public async ValueTask SelectIconAsync(string filePath)
        {
            this.IconPath = filePath;
            this.Icon = await this.fileService.CreateBitmapImageAsync(filePath).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async ValueTask SelectFileAsync(string filePath)
        {
            this.Path = filePath;

            if (string.IsNullOrEmpty(this.IconPath))
            {
                var fileIcon = System.Drawing.Icon.ExtractAssociatedIcon(filePath).ToBitmap();

                this.IconPath = await this.fileService.SaveBitmapAsync(fileIcon).ConfigureAwait(false);

                if (string.IsNullOrEmpty(this.IconPath))
                {
                    return;
                }

                this.Icon = await this.fileService.CreateBitmapImageAsync(this.IconPath).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async ValueTask<Product> AddProductAsync()
        {
            return new Product
            {
                Name = this.Name,
                IconPath = await this.fileService.CopyIconFileAsync(this.IconPath).ConfigureAwait(false),
                Path = this.Path,
            };
        }

        /// <inheritdoc />
        public void LoadProduct(Product product)
        {
            if (product == null)
            {
                return;
            }

            this.Name = product.Name;
            this.IconPath = product.IconPath;
            this.Icon = product.Icon;
            this.Path = product.Path;
        }
    }
}
