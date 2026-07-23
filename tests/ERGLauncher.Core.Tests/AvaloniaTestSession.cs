using Avalonia;
using Avalonia.Headless;

namespace ERGLauncher.Core.Tests;

public static class AvaloniaTestSession
{
    [Before(TestSession)]
    public static void Initialize() =>
        AppBuilder.Configure<Application>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .SetupWithoutStarting();
}