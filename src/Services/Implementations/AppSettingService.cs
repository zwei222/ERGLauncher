using System;
using System.IO;
using System.Threading.Tasks;
using ERGLauncher.Core;
using Utf8Json;

namespace ERGLauncher.Services.Implementations
{
    /// <summary>
    /// Application setting service.
    /// </summary>
    public class AppSettingService : IAppSettingService
    {
        /// <summary>
        /// Application setting file name.
        /// </summary>
        private const string AppSettingFileName = "appSettings.json";

        /// <summary>
        /// File service.
        /// </summary>
        private readonly IFileService fileService;

        /// <summary>
        /// Application setting file path.
        /// </summary>
        private readonly string appSettingFilePath;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileService">File service</param>
        public AppSettingService(IFileService fileService)
        {
            this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            this.appSettingFilePath = Path.Combine(this.fileService.GetSettingsDirectoryPath(), AppSettingFileName);
        }

        /// <inheritdoc />
        public async ValueTask<AppSettings?> LoadAppSettingAsync()
        {
            if (!File.Exists(this.appSettingFilePath))
            {
                return null;
            }

            var jsonBytes = await File.ReadAllBytesAsync(this.appSettingFilePath).ConfigureAwait(false);

            return JsonSerializer.Deserialize<AppSettings>(jsonBytes);
        }

        /// <inheritdoc />
        public async ValueTask SaveAppSettingAsync(AppSettings settings)
        {
            var settingDirectoryPath = this.fileService.GetSettingsDirectoryPath();

            if (!Directory.Exists(settingDirectoryPath))
            {
                Directory.CreateDirectory(settingDirectoryPath);
            }

            var jsonBytes = JsonSerializer.Serialize(settings);

            await File.WriteAllBytesAsync(this.appSettingFilePath, jsonBytes).ConfigureAwait(false);
        }
    }
}
