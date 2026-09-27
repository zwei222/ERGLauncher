# T-3004 Native AoT Rework and Re-verification

[English](T-3004-verification-report.md) | [日本語](T-3004-verification-report.ja.md)

- Verdict: **Linux requirements passed / Windows x64 verification not applicable due to Linux build environment**
- Target branch: `feat/AvaloniaMigration`
- Base commit: `a131cdc3c9e94097ccf1dcf39b6487f68870779e`
- Change commit: Recorded in Kanban completion record
- Execution date: 2026-07-24
- Execution environment: Linux 7.0.9+parrot7-amd64, x64, .NET SDK 10.0.302

> **Evidence Retention Status:** This document is a historical report recording verification results from that time. The log names in the table below are the names referenced at the time of execution, but the logs themselves are not retained in the current repository. Therefore, the historical PASS cannot be independently verified from a checkout alone. When evaluating the current state, run the re-verification procedure described below anew, and preserve the generated logs and exit codes within the same execution unit.

## Changes Made

1. Configured `x:CompileBindings="True"` and `x:DataType="views:IMainViewDataContext"` on `MainView.axaml`, and applied the `core:Item` type to the item template. As a result, all reachable MainView bindings were converted from ReflectionBinding to compiled bindings.
2. Added `IMainViewDataContext`, defining the MainView binding surface with strong typing.
3. Added `--settings-smoke <base-directory>`, invokable from published executables. This verifies loading legacy-format JSON, Create/Read/Update/Delete operations for brands/products, saving app/game settings, persistence after service recreation, and loading after restarting in a separate process.
4. Added compiled binding contract tests and settings smoke integration tests.

## Verification Results

| ID | Command / Procedure | Historical Result | Historical Log Name (Currently Not Retained) |
|---|---|---|---|
| B-01 | `dotnet build ERGLauncher.sln -c Release --no-restore --warnaserror --verbosity minimal` | PASS. 0 warnings, 0 errors, exit 0 | `build-warnaserror.log` |
| T-01 | `dotnet test ERGLauncher.sln -c Release --no-restore --verbosity minimal` | PASS. 62 passed / 0 failed / 0 skipped, exit 0 | `test-integration.log` |
| A-02 | `dotnet publish src/ERGLauncher/ERGLauncher.csproj -c Release -r linux-x64 --self-contained` | PASS. exit 0, 0 IL2xxx/IL3xxx occurrences | `publish-linux-x64.log` |
| S-01 | Launch published ELF against legacy fixture with `--settings-smoke` | PASS. Loaded legacy app/game settings, CRUD, save, persistence after service recreation, exit 0 | `smoke-linux-x64.log` |
| S-02 | Relaunch the same published ELF against the same destination in a separate process | PASS. Reloaded saved culture/theme/brand/product, exit 0 | `smoke-linux-x64.log` |
| A-01 | Check viability of win-x64 Native AoT publish on Linux host | SDK reported `Cross-OS native compilation is not supported.`, exit 1. Excluded from evaluation as this condition applies only to a Windows build environment | `publish-win-x64.log` |

## Re-verification Procedure

`--no-restore` should only be used after an explicit restore has succeeded within the same verification run. To verify the current checkout, execute in at least the following order:

```sh
./tools/verify-tests.sh

dotnet restore ERGLauncher.sln
mkdir -p qa-artifacts/verification
RUN_ROOT="$(mktemp -d qa-artifacts/verification/aot-run-XXXXXX)"
mkdir -p "$RUN_ROOT/settings"
cp -- tests/ERGLauncher.Tests/Fixtures/appSettings.json "$RUN_ROOT/settings/appSettings.json"
cp -- tests/ERGLauncher.Tests/Fixtures/gameSettings.json "$RUN_ROOT/settings/gameSettings.json"

dotnet publish src/ERGLauncher/ERGLauncher.csproj \
  -c Release -r linux-x64 --self-contained true \
  -p:PublishAot=true --no-restore \
  -o "$RUN_ROOT/publish"

"$RUN_ROOT/publish/ERGLauncher" --settings-smoke "$RUN_ROOT"
```

During evaluation, save the complete stdout, stderr, exit code, `$RUN_ROOT/publish`, and the path of the executed binary under the same `$RUN_ROOT`. Because `qa-artifacts/verification/` is not tracked by Git, upload to a separate persistent location, such as CI artifacts, if retention is required.

## Specific Checked Values for Linux Publish Smoke

- Artifact: ELF 64-bit x86-64 Native AoT executable
- Initial load: Loaded `culture=ja-JP theme=Dark brands=1` from legacy JSON
- CRUD: brand/product create, product rename update, target product delete
- After saving: `appSettings.json` has `Culture.Name=en-US`, `Theme=1`
- In-process service recreation: `RESTART_PERSISTENCE=PASS`
- Separate process restart: `CRUD_READ_AFTER_RESTART=PASS`
- Final SHA-256:
  - appSettings.json: `8b5a626e0016ce13f4ab0737e6da1398e1b5d6d60c394bfbf95bd7c45c1520a9`
  - gameSettings.json: `7976ebc542ada19b246e3f4604a8ac87a6c572c7f0543ad277539232a16ccb00`

## Handling of Windows Conditions

Acceptance Criterion 2 applies only when the build environment is Windows. The execution environment was Linux x64, and because win-x64 Native AoT cross-compilation from Linux is unsupported by the .NET SDK, Windows publish and `.exe` smoke were not executed. The unexecuted results are not treated as success, but recorded as not applicable to the conditions.

## Residual Risks

Native AoT publish on Windows x64 and settings smoke for the published `.exe` have not been tested. When verifying these changes in a Windows build environment, obtain the complete logs for `publish-win-x64.log` and `.exe --settings-smoke`.
