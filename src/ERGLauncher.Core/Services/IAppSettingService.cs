using ERGLauncher.Core.Models;

namespace ERGLauncher.Core.Services;

public interface IAppSettingService
{
    ValueTask<AppSettings?> LoadAppSettingAsync(CancellationToken cancellationToken = default);

    ValueTask SaveAppSettingAsync(AppSettings settings, CancellationToken cancellationToken = default);
}
