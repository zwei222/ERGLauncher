using System.Text;
using ERGLauncher.Core.Models;
using ERGLauncher.Core.Serialization;

namespace ERGLauncher.Core.Services;

public sealed class AppSettingService : IAppSettingService
{
    private const string AppSettingFileName = "appSettings.json";
    private readonly IFileService fileService;
    private readonly string appSettingFilePath;

    public AppSettingService(IFileService fileService)
    {
        this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        this.appSettingFilePath = Path.Combine(fileService.GetSettingsDirectoryPath(), AppSettingFileName);
    }

    public async ValueTask<AppSettings?> LoadAppSettingAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(this.appSettingFilePath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(this.appSettingFilePath, cancellationToken).ConfigureAwait(false);
        return SettingJsonSerializer.DeserializeAppSettings(json);
    }

    public async ValueTask SaveAppSettingAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        Directory.CreateDirectory(this.fileService.GetSettingsDirectoryPath());
        var json = SettingJsonSerializer.Serialize(settings);
        await File.WriteAllTextAsync(this.appSettingFilePath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), cancellationToken).ConfigureAwait(false);
    }
}
