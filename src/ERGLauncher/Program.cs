using Avalonia;
using System;

namespace ERGLauncher;

internal static class Program
{
    [STAThread]
    public static async Task<int> Main(string[] args)
    {
        if (args is ["--settings-smoke", var baseDirectoryPath])
        {
            return await SettingsSmoke.RunAsync(baseDirectoryPath).ConfigureAwait(false);
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        return 0;
    }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace();
}
