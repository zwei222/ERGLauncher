namespace ERGLauncher.Core.Services;

public interface IFileService
{
    string GetBaseDirectoryPath();

    string GetDefaultIconFilePath();

    string GetSettingsDirectoryPath();

    ValueTask<byte[]> ReadAllBytesAsync(string filePath, CancellationToken cancellationToken = default);

    ValueTask<Avalonia.Media.Imaging.Bitmap?> CreateBitmapAsync(string? filePath, CancellationToken cancellationToken = default);

    ValueTask<string?> CopyIconFileAsync(string? filePath, CancellationToken cancellationToken = default);

    ValueTask<string> SaveIconAsync(ReadOnlyMemory<byte> content, string extension = ".png", CancellationToken cancellationToken = default);

    ValueTask ExecuteAsync(string filePath, CancellationToken cancellationToken = default);
}
