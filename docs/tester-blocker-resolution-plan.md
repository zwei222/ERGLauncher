# Tester / Reviewer Blocker Resolution Replanning

[English](tester-blocker-resolution-plan.md) | [日本語](tester-blocker-resolution-plan.ja.md)

Creation Date: 2026-07-23
Target Board: erglauncher-avalonia-migration
Target Tasks: T-2007 (blocked) / T-2008 (blocked) / T-2009 (blocked), QA Rejection t_c0e66663

---

## 1. Summary of Blocker Causes

### 1.1 Location of Deliverables Identified from Current State Investigation

| Deliverable | Location | Status |
|:-------|:-----|:-----|
| Legacy WPF code (master) | `/home/hermes/repos/workspace/agent` (master currently checked out) | Original source for migration |
| Avalonia foundation (equivalent to T-2001) | worktree `T-1002`, no branch (commit `0130482`) | `src/ERGLauncher.Avalonia/` + `tests/ERGLauncher.Avalonia.Tests/` + global.json |
| Core layer + STJ compatibility (T-2002) | Branch `feat/AvaloniaMigration-T2002` (commit `f7fee5d`) | Already deployed to worktree `T-2007` as well |
| Services layer (T-2003) | worktree `T-1003` (`src/ERGLauncher.Core/` + `tests/ERGLauncher.Core.Tests/`) | Commit not created (remains on master) |
| Models layer (T-2004) | Integrated into worktree `t_c0e66663` (commit `4ef749c`) | Tests also included |
| ViewModels (T-2005) | Branch `feat/AvaloniaMigration-T2005` / `feat/AvaloniaMigration-T1004` (commit `0cacf15`) | worktree `T-1004` |
| Views (T-2006) | worktree `T-1005` (commit `72adffc`) | `tests/ERGLauncher.Views.*` included |
| **Final Integration Candidate** | **worktree `t_c0e66663` (branch `task/t_c0e66663`, commit `158e49e`)** | **Latest state containing T-2002 through T-2005 + Models + 3 test projects (ERGLauncher.Tests / ERGLauncher.Core.Tests / ERGLauncher.ViewModels.Tests) + Fixtures (appSettings.json / gameSettings.json)** |
| global.json (Microsoft.Testing.Platform enforcement) | Root of each of `t_c0e66663` / `T-2007` / `T-1002` / `T-1004` | Inconsistent with configuration on the test project side |
| `feat/AvaloniaMigration` branch in user workspace | `/home/hermes/repos/workspace/agent` | Commit `35d9806` (scaffold only). **T-2002 through T-2006 and tests not integrated** |

### 1.2 QA Findings (From t_c0e66663 body)

1. **Inconsistency in test execution configuration**: While the root global.json enforces `"test": { "runner": "Microsoft.Testing.Platform" }`, `tests/ERGLauncher.Tests/ERGLauncher.Tests.csproj` is treated as VSTest, causing `dotnet test --no-restore` to stop with exit 1. Not a single TUnit test is executed. `dotnet run` also exits with exit 1 due to OutputType=Library.
2. **Missing required test files**: Acceptance-specified files `HistoryCollectionTests.cs` / `AppSettingServiceTests.cs` / `GameSettingServiceTests.cs` / `MainModelTests.cs` do not exist under `tests/ERGLauncher.Tests/`. The existing `CoreModelTests.cs` only covers Push-after-undo for History, lacking coverage for Push/Back/Forward/Remove/Clear/Peek/At and undo/redo states. Services/Models tests are unimplemented.
   - *Note: Tests with the same names exist under `tests/ERGLauncher.Core.Tests/`, but the project configuration differs from the layout specified by the T-2007 acceptance criteria (Core/Services/Models under `tests/ERGLauncher.Tests/`), and they are not actually executed due to test execution configuration inconsistencies.*
3. **Insufficient provenance for JSON fixtures**: Although the fixtures have no BOM (leading bytes `7b22...`), there is no evidence (generation procedure, save logs from legacy application, etc.) showing they originate from actual Utf8Json output.
4. **Integration issues**: T-2002 Core / T-2003 Services / T-2004 Models / test projects are not assembled in a single integrable worktree. The user workspace `/home/hermes/repos/workspace/agent` contains only the legacy WPF code, and migration deliverables have not been integrated into the `feat/AvaloniaMigration` branch either.

### 1.3 Key Points of T-2008 QA Report (qa-evidence/verification-report.md)

- The T-2008 workspace was initially empty; verification was performed by copying T-1004 (ViewModels only, `0cacf15`) → That project is a WPF project with `net6-windows` + `UseWPF=true`, and `dotnet publish` failed with NETSDK1100 for both win-x64 and linux-x64.
- The path itself for the acceptance command `dotnet run --project src/ERGLauncher` does not exist (it is actually `src/ERGLauncher.csproj`).
- No OS guard on `Icon.ExtractAssociatedIcon` in `AddProductModel.cs` (Static Finding X1).
- Conclusion: The root cause is that a **single integrated revision** supporting cross-platform TFM, Avalonia, and AoT was not provided to the tester.

### 1.4 Summary of Root Causes

- **Each Coder task left deliverables in independent worktrees, and they were not aggregated into a single integrated branch/worktree** (the primary cause).
- Inconsistency between global.json enforcing Microsoft.Testing.Platform and test project configurations (treated as VSTest / TUnit unconfigured).
- Insufficient test implementation at paths specified by T-2007, insufficient provenance for actual Utf8Json fixtures.
- Discrepancy between acceptance procedure for T-2008 (`src/ERGLauncher/ERGLauncher.csproj`) and actual project layout, unconfigured AoT settings.

---

## 2. Work Breakdown for Resolution

### Process Overview

```
T-3001 (coder)    Aggregate deliverables into integrated branch + fix test execution configuration
T-3002 (coder)    Implement missing tests + generate provenance for actual Utf8Json fixtures
T-3003 (tester)   Unit test acceptance verification (all dotnet test pass)
T-3004 (tester)   Integration test, Native AoT, and cross-platform verification
T-3005 (reviewer) Code review (requirements compliance, prohibited libraries, design quality)
```

### T-3001 (coder / priority 1): Integrated Branch Aggregation and Test Execution Configuration Fix

**Objective**: Provide a "single integrated worktree + branch" that testers can verify as-is.

**Tasks**:
1. Check out the `feat/AvaloniaMigration` branch in user workspace `/home/hermes/repos/workspace/agent`.
2. Based on the contents of the final integration candidate worktree `~/.hermes/kanban/boards/erglauncher-avalonia-migration/workspaces/t_c0e66663` (commit `158e49e`), aggregate the following:
   - `src/ERGLauncher/` (Avalonia application main body. Integrated version including T-2005 Views, T-2004 Models, T-2003 Services, and T-2002 Core. However, reconcile whether View/XAML changes from T-2006 Views deliverable `T-1005` worktree are missing, and incorporate any missing parts from T-1005 commit `72adffc`)
   - `src/ERGLauncher.Core/` (T-2003 deliverable)
   - `tests/ERGLauncher.Tests/` / `tests/ERGLauncher.Core.Tests/` / `tests/ERGLauncher.ViewModels.Tests/` / `tests/ERGLauncher.Views.Tests` (if present)
   - root `global.json`, solution file (.sln / .slnx)
3. **Fix test execution configuration (resolving QA Finding 1)**:
   - Unify all test projects (`tests/**/*.csproj`) to Microsoft.Testing.Platform configuration: explicitly specify `<OutputType>Exe</OutputType>`, TUnit package (latest `TUnit`), and required extensions such as `Microsoft.Testing.Extensions.TrxReport`; organize VSTest packages (VSTest dependencies such as `Microsoft.NET.Test.Sdk` / `coverlet.collector`) into a form consistent with the TUnit + MTP configuration.
   - While retaining `"test": { "runner": "Microsoft.Testing.Platform" }` in root `global.json`, confirm that `dotnet test tests/ERGLauncher.Tests/ERGLauncher.Tests.csproj --no-restore` actually runs TUnit tests with exit 0.
   - Set the application main body `src/ERGLauncher/ERGLauncher.csproj` to `<OutputType>WinExe</OutputType>` (Avalonia desktop application), and confirm that `dotnet run --project src/ERGLauncher/ERGLauncher.csproj` launches on Linux (resolving the finding of exit 1 due to OutputType=Library). Unify the acceptance command path notation (`src/ERGLauncher` vs. `src/ERGLauncher/ERGLauncher.csproj`) to match the actual layout, and state it clearly in the handoff document.
4. **Unify to cross-platform TFM (resolving QA Finding and T-2008 Finding)**:
   - Unify the application main body to `net10.0` (not a Windows-exclusive TFM) + Avalonia references. Remove any remaining WPF dependencies (`UseWPF`, `net6-windows`).
   - Add AoT settings such as `PublishAot` / `IsAotCompatible` / `InvariantGlobalization` to csproj.
   - Guard Windows-exclusive APIs such as `Icon.ExtractAssociatedIcon` with `OperatingSystem.IsWindows()`, and implement a fallback using a default icon on Linux (resolving T-2008 Static Finding X1).
5. Confirm locally that `dotnet build -c Release --warnaserror` succeeds, and that `dotnet test` (standard execution) runs all tests and succeeds.
6. Commit to the `feat/AvaloniaMigration` branch (Conventional Commits). Record commit hash, full `dotnet test` log, and test count in the task body.

**Deliverables**: Integrated `feat/AvaloniaMigration` branch (user workspace) + verifiable worktree.

### T-3002 (coder / priority 2): Implementation of Missing Tests and Generation of Provenance for Actual Utf8Json Fixtures

**Dependencies**: T-3001.

**Tasks**:
1. **Implement tests at paths specified by T-2007 (resolving QA Finding 2)** — all in TUnit:
   - `tests/ERGLauncher.Tests/Core/HistoryCollectionTests.cs`: Operations Push / Back / Forward / Remove / Clear / Peek / At, state transitions for `IsEnabledUndo` / `IsEnabledRedo`, regression where redo side is cleared on Push after undo.
   - `tests/ERGLauncher.Tests/Core/JsonCompatibilityTests.cs`: Deserialization of legacy-format fixtures, key structure match in new serialization output (PascalCase / Culture is `{"Name":"..."}` / enum is numeric), `Item.Icon` not being included in JSON.
   - `tests/ERGLauncher.Tests/Services/AppSettingServiceTests.cs`: null when nonexistent, Save→Load round-trip.
   - `tests/ERGLauncher.Tests/Services/GameSettingServiceTests.cs`: Empty RootItem when empty/nonexistent, round-trip, CreateBrandItemAsync / CreateProductItemAsync.
   - `tests/ERGLauncher.Tests/Models/MainModelTests.cs`: LoadSettingAsync loading brands into Items, AddItemAsync (not added when duplicate), RemoveItemAsync, Back/Forward history transitions. Demonstration that `IFileService` / `IGameSettingService` / `IAppSettingService` can be substituted using Moq or NSubstitute.
   - If contents overlap with existing tests of the same name under `tests/ERGLauncher.Core.Tests/`, aggregate and migrate them into the T-2007 specified project, organizing them so they are not double-counted by duplicate execution.
2. **Generate provenance for actual Utf8Json fixtures (resolving QA Finding 3)**:
   - Generate `appSettings.json` / `gameSettings.json` using the legacy WPF application (Utf8Json serializer) or serialization settings identical to the legacy format (equivalent to Utf8Json: PascalCase keys, CultureInfo as `{"Name":...}`, numeric enums, `[IgnoreDataMember]` excluded), and place them in `tests/ERGLauncher.Tests/Fixtures/`.
   - Record the generation procedure (used code/tools, commands, method for verifying leading bytes `7b22...` = UTF-8 without BOM) in `tests/ERGLauncher.Tests/Fixtures/README.md` (new) to enable tracing of fixture provenance.
3. Cover happy path, error cases, edge cases, and regressions, and confirm that `dotnet test` (standard execution, keeping MTP configuration in global.json) executes all tests successfully.
4. Commit, and record full log, test count, and commit hash in the body.

### T-3003 (tester / priority 3): Unit Test Acceptance Verification

**Dependencies**: T-3002. Substantially takes over acceptance of T-2007.

**Verification Items**:
1. Confirm that `dotnet test tests/ERGLauncher.Tests/ERGLauncher.Tests.csproj --no-restore --verbosity normal` executes all TUnit tests with exit 0 in the integrated worktree (`feat/AvaloniaMigration`) (re-verification of QA Finding 1).
2. Confirm that the 5 specified test files (HistoryCollectionTests / JsonCompatibilityTests / AppSettingServiceTests / GameSettingServiceTests / MainModelTests) exist under `tests/ERGLauncher.Tests/` and cover happy path, error cases, edge cases, and regressions for Core / JSON compatibility / Services / Models.
3. Confirm that JSON compatibility tests are verified with actual Utf8Json-format fixtures (no BOM, with provenance documentation of generation procedure).
4. Confirm that service substitution using mocks (Moq or NSubstitute) is demonstrated.
5. Record full log, test count, and target git commit; if passed, set to done; if failed, record findings in the body and return to coder.

### T-3004 (tester / priority 4): Integration Test, Native AoT, and Cross-Platform Verification

**Dependencies**: T-3003. Substantially takes over acceptance of T-2008.

**Verification Items**:
1. Integration test: Launch → add brand → add product → change settings → exit → relaunch → verify persistence. Verification of reading under legacy-format `settings/appSettings.json` / `settings/gameSettings.json` placement.
2. Native AoT: Confirm that `dotnet publish src/ERGLauncher/ERGLauncher.csproj -c Release -r win-x64 --self-contained` and `-r linux-x64` both succeed, with 0 AoT warnings (IL2xxx / IL3xxx). Smoke test of publish binaries (launch, read/write settings).
   - In preparation for cases where win-x64 publish from a Linux host is needed, confirm necessary settings on the csproj side (handling of Windows targeting); if impossible, clearly state so along with alternative evidence.
3. Cross-platform: On Linux, confirm that the equivalent of `Icon.ExtractAssociatedIcon` is skipped and default icon is used, and path separators are handled properly.
4. Record results with logs/screenshots in `qa-evidence/`. If failed, return to coder.

### T-3005 (reviewer / priority 5): Code Review

**Dependencies**: T-3004. Substantially takes over acceptance of T-2009.

**Review Perspectives** (Following T-2009 checklist):
1. Requirements compliance: .NET 10 / CommunityToolkit.Mvvm (no ReactiveProperty or Prism) / System.Text.Json (no Utf8Json) / backward compatibility of configuration JSON schema / FluentTheme / TUnit.
2. 0 prohibited libraries: `Prism.*`, `ReactiveProperty`, `Utf8Json`, `MaterialDesignThemes.*`, `Gu.Wpf.Localization`, `ProcessX`, `Microsoft.Xaml.Behaviors.Wpf`.
3. Design quality: MVVM / DI / guarding platform-dependent code (`OperatingSystem.IsWindows()`, etc.) / AoT support (source generation, reflection elimination).
4. Code quality: `dotnet build --warnaserror` succeeds, naming and comment consistency.
5. Review `git diff master..feat/AvaloniaMigration`; if there are findings, return to coder; if all cleared, set to done.

---

## 3. Acceptance Criteria (Overall Replanning)

The following conditions covering all QA findings are defined as the comprehensive acceptance criteria for the new task group:

1. While maintaining the `Microsoft.Testing.Platform` configuration in root `global.json`, standard `dotnet test` **executes and passes all TUnit tests across all test projects** (exit 0). There must be no halting due to treatment as VSTest.
2. The 5 specified test files (`HistoryCollectionTests.cs` / `JsonCompatibilityTests.cs` / `AppSettingServiceTests.cs` / `GameSettingServiceTests.cs` / `MainModelTests.cs`) must be implemented under `tests/ERGLauncher.Tests/`.
3. JSON compatibility verification must be conducted using **fixtures originating from actual Utf8Json output** (no BOM, accompanied by provenance documentation of generation procedure).
4. T-2002 Core / T-2003 Services / T-2004 Models / T-2005 ViewModels / T-2006 Views / test projects must be provided as a **single worktree integrated into the `feat/AvaloniaMigration` branch** in user workspace `/home/hermes/repos/workspace/agent`.
5. `dotnet publish` for win-x64 / linux-x64 must succeed, with 0 AoT warnings, and smoke tests of published binaries must succeed.
6. In each phase, the **complete log, test count, and git commit hash** must be recorded in the task body.
7. `dotnet build --warnaserror` must succeed, with 0 references to prohibited libraries.

---

## 4. Handling of Existing Blocked Tasks (T-2007 / T-2008 / T-2009)

- **Policy: Replace with new tasks (T-3003 / T-3004 / T-3005).** Keep existing tasks with `blocked` status, and append a note at the end of the body stating: "This task has been replaced by T-3003 (T-3004 / T-3005) due to replanning. Acceptance criteria will be managed on the new task side."
- Reason: Although the acceptance criteria descriptions in existing tasks remain valid, tester/reviewer cannot start work while the prerequisite "provision of an integrated worktree" is missing; transferring dependencies to newly numbered tasks that re-establish steps starting from coder is more consistent with dispatcher priority control (in priority order).
- For QA rejection task `t_c0e66663` (treated as status=done), clearly state via body note that all findings have been incorporated into the work scope of T-3001 / T-3002 (retained as history).

## 5. Migration Requirements (Common across all tasks / Carried over)

.NET 10 / Windows and Linux cross-platform / Native AoT support / MVVM / CommunityToolkit.Mvvm (ReactiveProperty prohibited) / System.Text.Json (Utf8Json prohibited) / Prism prohibited / Backward compatibility of existing JSON configuration files / Fluent Design (FluentTheme) / TUnit / Recorded in `feat/AvaloniaMigration` branch / Workspace `/home/hermes/repos/workspace/agent`

## 6. References

- Original Plan: `docs/avalonia-migration-plan.md` (Sections 1, 6, 7)
- QA Rejection: Kanban task `t_c0e66663` body
- T-2008 QA Report: `~/.hermes/kanban/boards/erglauncher-avalonia-migration/workspaces/T-2008/qa-evidence/verification-report.md`
