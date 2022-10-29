using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Cysharp.Diagnostics;
using Cysharp.Text;
using ERGLauncher.Properties;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace ERGLauncher.Services.Implementations
{
    /// <summary>
    /// File service.
    /// </summary>
    public class FileService : IFileService
    {
        /// <summary>
        /// Assets directory name.
        /// </summary>
        private const string AssetsDirectoryName = "Assets";

        /// <summary>
        /// Default icon file name.
        /// </summary>
        private const string DefaultIconFileName = "icon.png";

        /// <summary>
        /// Settings directory name.
        /// </summary>
        private const string SettingsDirectoryName = "settings";

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<FileService> logger;

        /// <summary>
        /// Resource service.
        /// </summary>
        private readonly IResourceService resourceService;

        /// <summary>
        /// Base directory path.
        /// </summary>
        private readonly string baseDirectoryPath;

        /// <summary>
        /// Default icon file path.
        /// </summary>
        private readonly string defaultIconFilePath;

        /// <summary>
        /// Settings directory path.
        /// </summary>
        private readonly string settingsDirectoryPath;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="resourceService">Resource service</param>
        public FileService(ILogger<FileService> logger, IResourceService resourceService)
        {
            this.logger = logger;
            this.resourceService = resourceService;
#if DEBUG
            this.baseDirectoryPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;
#else
            using (var module = System.Diagnostics.Process.GetCurrentProcess().MainModule)
            {
                this.baseDirectoryPath = Path.GetDirectoryName(module?.FileName) ?? string.Empty;
            }
#endif
            this.defaultIconFilePath = Path.Combine(this.baseDirectoryPath, AssetsDirectoryName, DefaultIconFileName);
            this.settingsDirectoryPath = Path.Combine(this.baseDirectoryPath, SettingsDirectoryName);
        }

        /// <inheritdoc />
        public async ValueTask<BitmapImage> CreateBitmapImageAsync(string filePath)
        {
            var image = new BitmapImage();
            await using var fileStream = File.OpenRead(filePath);

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = fileStream;
            image.EndInit();

            return image;
        }

        /// <inheritdoc />
        public async ValueTask<string?> CopyIconFileAsync(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            if (Directory.GetParent(filePath).FullName == Path.Combine(this.baseDirectoryPath, AssetsDirectoryName))
            {
                return filePath;
            }

            var newFilePath = this.GetNewFilePath(Path.GetExtension(filePath));
            await using var sourceStream = File.OpenRead(filePath);
            await using var destinationStream = File.Create(newFilePath);

            await sourceStream.CopyToAsync(destinationStream).ConfigureAwait(false);

            return newFilePath;
        }

        /// <inheritdoc />
        public async ValueTask ExecuteAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(this.resourceService.GetCultureString(nameof(Resources.ExecutableFileNotFound)), filePath);
            }

            using var zstringBuilder = ZString.CreateStringBuilder();

            zstringBuilder.Append("/c \"");
            zstringBuilder.Append(filePath);
            zstringBuilder.Append("\"");

            var (_, stdOut, stdError) = ProcessX.GetDualAsyncEnumerable(
                fileName: "cmd.exe",
                zstringBuilder.ToString(),
                Directory.GetParent(filePath).FullName);
            var stdOutTask = Task.Run(async () =>
            {
                await foreach (var item in stdOut)
                {
                    this.logger.ZLogInformation(item);
                }
            });
            var stdErrorTask = Task.Run(async () =>
            {
                await foreach (var item in stdError)
                {
                    this.logger.ZLogError(item);
                }
            });

            try
            {
                await Task.WhenAll(stdOutTask, stdErrorTask).ConfigureAwait(false);
            }
            catch (ProcessErrorException processErrorException)
            {
                this.logger.ZLogError(
                    processErrorException,
                    "[{0}] {1}",
                    DateTimeOffset.Now.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture),
                    this.resourceService.GetCultureString(nameof(Resources.FailedStartExecutableFile)));
                throw;
            }
        }

        /// <inheritdoc />
        public async ValueTask<string?> SaveBitmapAsync(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                return null;
            }

            var filePath = this.GetNewFilePath(".png");
            await using var fileStream = File.Create(filePath);

            bitmap.Save(fileStream, ImageFormat.Png);

            return filePath;
        }

        /// <inheritdoc />
        public string GetBaseDirectoryPath()
        {
            return this.baseDirectoryPath;
        }

        /// <inheritdoc />
        public string GetDefaultIconFilePath()
        {
            return this.defaultIconFilePath;
        }

        /// <inheritdoc />
        public string GetSettingsDirectoryPath()
        {
            return this.settingsDirectoryPath;
        }

        /// <summary>
        /// Get new file path.
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <returns>New file path</returns>
        private string GetNewFilePath(string extension)
        {
            var newFilePath = Path.Combine(this.baseDirectoryPath, AssetsDirectoryName, Guid.NewGuid().ToString("N") + extension);

            if (File.Exists(newFilePath))
            {
                return this.GetNewFilePath(extension);
            }

            return newFilePath;
        }
    }
}
