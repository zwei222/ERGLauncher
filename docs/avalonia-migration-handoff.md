# Avalonia migration integration handoff

The integrated verification worktree is `/home/hermes/repos/workspace/agent` on branch `feat/AvaloniaMigration`.

Run from the repository root:

```bash
dotnet build -c Release --warnaserror
dotnet test
dotnet test tests/ERGLauncher.Tests/ERGLauncher.Tests.csproj --no-restore
dotnet run --project src/ERGLauncher/ERGLauncher.csproj
```

`global.json` keeps `Microsoft.Testing.Platform` as the test runner. The solution contains the four TUnit executable test projects `ERGLauncher.Tests`, `ERGLauncher.Core.Tests`, `ERGLauncher.ViewModels.Tests`, and `ERGLauncher.Views.Tests`.

The application project is `src/ERGLauncher/ERGLauncher.csproj` (not the legacy root-level `src/ERGLauncher.csproj`). It targets `net10.0`, uses Avalonia FluentTheme, and has `PublishAot` and `IsAotCompatible` enabled. A running X11/Wayland display is required for the desktop launch command; headless shells fail at `XOpenDisplay` before a window can be created.

Integration verification on 2026-07-23:

- Release build: succeeded with 0 warnings and 0 errors.
- Full Microsoft.Testing.Platform run: 60 passed, 0 failed, 0 skipped across all four projects.
- Targeted `ERGLauncher.Tests` no-restore run: 6 passed, 0 failed, 0 skipped.
- Desktop command: executable reached Avalonia platform initialization, but the worker shell had no authorized display (`XOpenDisplay failed`); GUI smoke testing remains for the desktop-enabled tester.
