namespace ERGLauncher.Core.Tests;

internal sealed class TemporaryDirectory : IDisposable
{
    public TemporaryDirectory()
    {
        this.Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"erglauncher-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(this.Path);
    }

    public string Path { get; }

    public void Dispose()
    {
        Directory.Delete(this.Path, recursive: true);
    }
}
