using System.Diagnostics;

namespace ERGLauncher.Core.Services;

public sealed class FileService : IFileService
{
    private const string AssetsDirectoryName = "Assets";
    private const string DefaultIconFileName = "icon.png";
    private const string SettingsDirectoryName = "settings";
    private readonly string baseDirectoryPath;

    public FileService(string? baseDirectoryPath = null)
    {
        this.baseDirectoryPath = ResolveBaseDirectoryPath(baseDirectoryPath);
    }

    /// <summary>
    /// Resolves the base directory used to locate the <c>settings/</c> and <c>Assets/</c>
    /// folders. An explicit argument always wins; otherwise the directory of the running
    /// executable (<see cref="Environment.ProcessPath"/>) is used so that files placed
    /// beside the launcher — as they were with the legacy WPF build — are found even when
    /// published as single-file or Native AoT, where <see cref="AppContext.BaseDirectory"/>
    /// can point elsewhere. <see cref="AppContext.BaseDirectory"/> remains the final fallback.
    /// </summary>
    private static string ResolveBaseDirectoryPath(string? baseDirectoryPath)
    {
        if (!string.IsNullOrWhiteSpace(baseDirectoryPath))
        {
            return Path.GetFullPath(baseDirectoryPath);
        }

        var processPath = Environment.ProcessPath;
        if (!string.IsNullOrWhiteSpace(processPath))
        {
            var processDirectory = Path.GetDirectoryName(processPath);
            if (!string.IsNullOrWhiteSpace(processDirectory))
            {
                return Path.GetFullPath(processDirectory);
            }
        }

        return Path.GetFullPath(AppContext.BaseDirectory);
    }

    public string GetBaseDirectoryPath() => this.baseDirectoryPath;

    public string GetDefaultIconFilePath() => Path.Combine(this.baseDirectoryPath, AssetsDirectoryName, DefaultIconFileName);

    public string GetSettingsDirectoryPath() => Path.Combine(this.baseDirectoryPath, SettingsDirectoryName);

    public async ValueTask<byte[]> ReadAllBytesAsync(string filePath, CancellationToken cancellationToken = default) =>
        await File.ReadAllBytesAsync(filePath, cancellationToken).ConfigureAwait(false);

    public ValueTask<Avalonia.Media.Imaging.Bitmap?> CreateBitmapAsync(
        string? filePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(
            string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)
                ? null
                : new Avalonia.Media.Imaging.Bitmap(filePath));
    }

    public async ValueTask<string?> CopyIconFileAsync(string? filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        var sourcePath = Path.GetFullPath(filePath);
        var assetsPath = Path.Combine(this.baseDirectoryPath, AssetsDirectoryName);
        Directory.CreateDirectory(assetsPath);
        if (string.Equals(Path.GetDirectoryName(sourcePath), assetsPath, StringComparison.Ordinal))
        {
            return sourcePath;
        }

        var destinationPath = this.GetNewAssetFilePath(Path.GetExtension(sourcePath));
        await using var source = File.OpenRead(sourcePath);
        await using var destination = File.Create(destinationPath);
        await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
        return destinationPath;
    }

    public async ValueTask<string> SaveIconAsync(ReadOnlyMemory<byte> content, string extension = ".png", CancellationToken cancellationToken = default)
    {
        var assetsPath = Path.Combine(this.baseDirectoryPath, AssetsDirectoryName);
        Directory.CreateDirectory(assetsPath);
        var destinationPath = this.GetNewAssetFilePath(extension);
        await File.WriteAllBytesAsync(destinationPath, content.ToArray(), cancellationToken).ConfigureAwait(false);
        return destinationPath;
    }

    public async ValueTask ExecuteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var executablePath = Path.GetFullPath(filePath);
        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException("Executable file was not found.", executablePath);
        }

        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = executablePath,
            WorkingDirectory = Path.GetDirectoryName(executablePath)!,
            UseShellExecute = true,
        }) ?? throw new InvalidOperationException($"Failed to start '{executablePath}'.");
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
    }

    private string GetNewAssetFilePath(string extension)
    {
        var normalizedExtension = extension.StartsWith('.') ? extension : $".{extension}";
        return Path.Combine(this.baseDirectoryPath, AssetsDirectoryName, $"{Guid.NewGuid():N}{normalizedExtension}");
    }
}
