# ERGLauncher Avalonia UI Migration Plan

[English](avalonia-migration-plan.md) | [Japanese](avalonia-migration-plan.ja.md)

## 1. Background and Purpose

Migrate ERGLauncher (WPF / .NET 6 / Prism.DryIoc + ReactiveProperty + MaterialDesignThemes.MahApps) to a cross-platform application based on Avalonia UI / .NET 10 / CommunityToolkit.Mvvm.

### Migration Requirements (Mandatory)

| Requirement | Details |
|:-----|:-----|
| Target Framework | .NET 10 |
| Platform | Windows / Linux (Primary) |
| Native AoT | Use a configuration capable of supporting it |
| MVVM | CommunityToolkit.Mvvm (Do not use ReactiveProperty / Prism) |
| JSON Serializer | System.Text.Json (Do not use Utf8Json) |
| Configuration File Compatibility | Enable reading and writing of existing `settings/appSettings.json` and `settings/gameSettings.json` as-is (Backward compatibility mandatory) |
| UI Design | Fluent Design (Avalonia FluentTheme) |
| Screen Composition | Strict adherence to the existing application is not required (Provide equivalent functionality) |
| UnitTest | TUnit |
| Branch | Work on `feat/AvaloniaMigration` (Created/checked out by Coder) |

---

## 2. Existing Code Investigation Results

### 2.1 Configuration File JSON Schema

#### appSettings.json

Save location: `{Application Base Directory}/settings/appSettings.json`

```json
{
  "Culture": { "Name": "ja-JP" },
  "Theme": 2
}
```

- Serializer: **Utf8Json** (Keys remain in C# property name PascalCase, not camelCase)
- `Culture`: Saved in `{"Name":"Culture Name"}` object format via `CultureInfoJsonFormatter`
- `Theme`: Numeric value of the `Theme` enum (0=None, 1=Light, 2=Dark, 3=Sync)

#### gameSettings.json

Save location: `{Application Base Directory}/settings/gameSettings.json`

```json
{
  "Brands": [
    {
      "Name": "Brand name",
      "IconPath": "Assets\\xxxx.png or null",
      "Products": [
        {
          "Name": "Game name",
          "IconPath": "Assets\\yyyy.png or null",
          "Path": "C:\\Games\\xxx\\game.exe",
          "BrandName": "Brand name"
        }
      ]
    }
  ]
}
```

- `Item.Icon` (`BitmapImage`) is excluded from JSON persistence via `[IgnoreDataMember]`. The image is restored from `IconPath` during load.
- `Brand` / `RootItem` receives `ICollection<T>` via constructor injection using `[SerializationConstructor]`.
- The base directory is `Assembly.GetExecutingAssembly().Location` in DEBUG, and the parent directory of `Process.GetCurrentProcess().MainModule.FileName` in RELEASE.

### 2.2 Responsibilities and Interdependencies

| Layer | Major Classes | Responsibilities |
|:-------|:-----------|:-----|
| Core | `Item` (abstract), `Brand`, `Product`, `RootItem`, `AppSettings`, `HistoryCollection<T>`, `Theme` | Domain entities. `Item` inherits from Prism's `BindableBase` and holds `Name`/`IconPath`/`Icon` |
| Services | `IFileService`/`FileService`, `IAppSettingService`, `IGameSettingService`, `IResourceService`, `IThemeService`, `IDispatcherService`, `ICommonDialogService` | File I/O, JSON persistence, process execution, localization, theme switching, UI thread control, file dialogs |
| Models | `IMainModel`/`MainModel`, `IAddBrandModel`, `IAddProductModel`, `ISettingModel`, `IMessageModel` | Business logic. Combines Services to perform CRUD, history management (`HistoryCollection`), and setting application |
| ViewModels | `MainViewModel`, `AddBrandViewModel`, `AddProductViewModel`, `SettingViewModel`, `MessageViewModel` | Wraps Models with ReactiveProperty and exposes commands (`ReactiveCommand`/`AsyncReactiveCommand`). Controls dialogs via Prism's `IDialogService` / `IDialogAware` |
| Views | `MainView` (MetroWindow), `AddBrandView`, `AddProductView`, `SettingView`, `MessageView` (UserControl + prism:Dialog.WindowStyle) | MahApps.Metro + MaterialDesign XAML. Localized with `Gu.Wpf.Localization`'s `{gu:Static}` |

Dependency flow: `App` (PrismApplication, DryIoc) → Views ⇄ ViewModels → Models → Services → Core

### 2.3 Replacement Points List

| Existing Dependency | Usage Location | Replacement Target |
|:---------|:---------|:-------|
| **Prism.DryIoc** | `App.xaml.cs` (PrismApplication, RegisterTypes, ConfigureViewModelLocator), `DialogViewModelBase` (IDialogAware), `IMainModel.DialogCoordinator`, `prism:ViewModelLocator.AutoWireViewModel` in Views | Microsoft.Extensions.DependencyInjection + custom dialog service (or CommunityToolkit.Mvvm Messenger) |
| **ReactiveProperty** | All ViewModels (`ReactiveProperty<T>`, `ReadOnlyReactivePropertySlim<T>`, `ReactiveCommand`, `AsyncReactiveCommand`, `BusyNotifier`, `ToReactivePropertyAsSynchronized`) | CommunityToolkit.Mvvm `ObservableObject` + `[ObservableProperty]` + `[RelayCommand]` + `ObservableValidator`. Simplify Model→VM synchronization to manual property change notifications or direct inheritance of `ObservableObject` |
| **Utf8Json** | `AppSettings.cs`, `Brand.cs`, `RootItem.cs` (attributes), `AppSettingService`, `GameSettingService`, `CultureInfoJsonFormatter` | System.Text.Json + custom `JsonConverter<CultureInfo>`. `[JsonConstructor]` support |
| **MaterialDesignThemes.MahApps** | `App.xaml` (merged dictionaries), `ViewBase` (MetroWindow), MainView/each dialog style, `ThemeService` (ControlzEx ThemeManager) | Avalonia FluentTheme + standard styles. Theme switching via `Application.RequestedThemeVariant` |
| **Gu.Wpf.Localization / Gu.Localization** | `{gu:Static properties:Resources.Xxx}` in all Views, `ResourceService` (Translator), `Translator.Cultures` binding in SettingView | resx remains usable via .NET standard `ResourceManager` (`Properties.Resources` also functions in Avalonia). XAML binding policy is to provide values via VM or `IValueConverter`, rather than `{Binding Source={x:Static properties:Resources.Xxx}}` |
| **Microsoft.Xaml.Behaviors.Wpf** | MainView `Interaction.Triggers` (Loaded/Closing/MouseLeftButtonUp → InvokeCommandAction) | Replace with Avalonia `Behavior` (Avalonia.Xaml.Behaviors package) or event handlers |
| **ProcessX** (Cysharp.Diagnostics) | `FileService.ExecuteAsync` (executes with cmd.exe /c "..." and enumerates stdout/stderr asynchronously) | Replace with `System.Diagnostics.Process`. For cross-platform support, discontinue directly specifying `cmd.exe`, and launch with `ProcessStartInfo { FileName = filePath, WorkingDirectory = ..., UseShellExecute = true }` |
| **System.Drawing.Common** | `Icon.ExtractAssociatedIcon` in `AddProductModel.SelectFileAsync`, `FileService.SaveBitmapAsync` (Bitmap→PNG) | Windows: Continue using `System.Drawing.Common` (Windows-only support) or via `Shell32`. Linux: As an alternative, skip icon extraction or prepare a conversion layer to `Avalonia.Media.Imaging.Bitmap`. **Design Decision Required**: A realistic approach is to make automatic executable icon extraction a Windows-only feature and use a default icon on Linux |
| **BitmapImage (System.Windows.Media.Imaging)** | `Item.Icon`, `IFileService.CreateBitmapImageAsync`, `GameSettingService` load processing | `Avalonia.Media.Imaging.Bitmap` |
| **ZLogger** | `App.xaml.cs` (ILoggerFactory + AddZLoggerRollingFile), `logger.ZLogXxx` in each Model/Service | Can continue to be used (ZLogger is .NET standard). Use the source generator version for AoT support |

### 2.4 Localization

- 3 files: `Properties/Resources.resx` (default=en), `Resources.en-US.resx`, and `Resources.ja-JP.resx`. 29 keys.
- In WPF, direct XAML referencing via `Gu.Wpf.Localization` `{gu:Static}` + dynamic switching via `Translator.Culture`.
- After Avalonia migration: resx can be used as-is as `Properties.Resources` (`PublicResXFileCodeGenerator`). Since direct referencing from XAML is not possible, change to an approach where each ViewModel exposes string values via `IResourceService`. When language is switched, re-notify all string properties via `INotifyPropertyChanged`.

### 2.5 Platform Dependency Risks

| Location | Risk | Mitigation Strategy |
|:-----|:-------|:---------|
| `cmd.exe /c` in `FileService.ExecuteAsync` | Windows-only | Change to `Process.Start` with `UseShellExecute=true` (delegate association launch to the OS) |
| `Icon.ExtractAssociatedIcon` | Windows-only API | Valid on Windows only. Return null on Linux and use default icon |
| Path separator `\\` | `Path`/`IconPath` in gameSettings.json are Windows paths | Save and read as-is. Cases of using the identical settings on Linux are not assumed (acceptable because it is a per-user local configuration) |
| `System.Drawing.Common` | Supported only on Windows in .NET 6+ (`PlatformNotSupportedException` on Linux) | Guard Windows-only code paths with `#if WINDOWS` or `OperatingSystem.IsWindows()` |

---

## 3. Proposed Project Structure

### 3.1 Solution / Project Structure

```
ERGLauncher.sln
src/
  ERGLauncher/                      # Avalonia UI main application (net10.0)
    ERGLauncher.csproj
    App.axaml / App.axaml.cs        # Application + FluentTheme + DI initialization
    Program.cs                      # [STAThread] Main → BuildAvaloniaApp
    ViewModels/
      ViewModelBase.cs              # Inherits ObservableObject
      MainViewModel.cs
      AddBrandViewModel.cs
      AddProductViewModel.cs
      SettingViewModel.cs
      MessageViewModel.cs
    Views/
      MainWindow.axaml(.cs)         # Window (formerly MainView)
      AddBrandWindow.axaml(.cs)     # Change dialog to Window.ShowDialog
      AddProductWindow.axaml(.cs)
      SettingWindow.axaml(.cs)
      MessageWindow.axaml(.cs)
    Models/                          # Port existing IModel/implementations to CommunityToolkit.Mvvm
    Services/                        # Port while largely retaining existing interfaces
    Core/                            # Item, Brand, Product, RootItem, AppSettings, HistoryCollection, Theme
    Converters/                      # EnumToBooleanConverter, etc. (port IValueConverter)
    Properties/
      Resources.resx                 # Reuse existing
      Resources.en-US.resx
      Resources.ja-JP.resx
    Assets/
      icon.png                       # Reuse existing (default icon)
tests/
  ERGLauncher.Tests/                 # TUnit test project (net10.0)
    ERGLauncher.Tests.csproj
    Core/HistoryCollectionTests.cs
    Core/JsonCompatibilityTests.cs   # Existing JSON format compatibility test
    Services/AppSettingServiceTests.cs
    Services/GameSettingServiceTests.cs
    Models/MainModelTests.cs
```

**Design Decision**: Because existing ViewModels depend heavily on ReactiveProperty (bidirectional synchronization via `ToReactivePropertyAsSynchronized`), the recommended structure for the Avalonia version is to **eliminate or thin the Model layer, having ViewModels call Services directly**. However, because existing logic (MainModel history management, CRUD, duplicate check) is complex, a hybrid approach of preserving the Model layer based on CommunityToolkit.Mvvm `ObservableObject` and manually binding properties from the ViewModel is also acceptable. In this plan, the policy is to **preserve the Model layer and replace the INotifyPropertyChanged implementation with CommunityToolkit.Mvvm** (minimizes diffs and maintains testability).

### 3.2 csproj Skeleton (ERGLauncher.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <AssemblyName>ERGLauncher</AssemblyName>
    <RootNamespace>ERGLauncher</RootNamespace>
    <NeutralLanguage>en-US</NeutralLanguage>
    <SatelliteResourceLanguages>en-US;ja-JP</SatelliteResourceLanguages>
    <!-- Native AoT -->
    <PublishAot>true</PublishAot>
    <IsAotCompatible>true</IsAotCompatible>
    <InvariantGlobalization>false</InvariantGlobalization>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Avalonia" Version="11.3.*" />
    <PackageReference Include="Avalonia.Desktop" Version="11.3.*" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.3.*" />
    <PackageReference Include="Avalonia.Fonts.Inter" Version="11.3.*" />
    <PackageReference Include="Avalonia.Xaml.Behaviors" Version="11.3.*" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.*" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.*" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="10.0.*" />
    <PackageReference Include="ZLogger" Version="2.5.*" />
    <PackageReference Include="ZLogger.Providers" Version="2.5.*" />
  </ItemGroup>
</Project>
```

### 3.3 Dependency Library Selection and Versioning Policy

| Library | Versioning Policy | Selection Reason |
|:-----------|:---------------|:---------|
| Avalonia / Avalonia.Desktop / Avalonia.Themes.Fluent / Avalonia.Fonts.Inter | **11.3.x (floating patch)** | Latest stable line. FluentTheme is required. Desktop supports both Windows and Linux |
| Avalonia.Xaml.Behaviors | 11.3.x (same line as Avalonia) | Replacement for WPF Behaviors (Loaded/Closing triggers, etc.) |
| CommunityToolkit.Mvvm | 8.4.x | Specified requirement. AoT-friendly via source generators |
| System.Text.Json | Bundled with .NET 10 (no package reference required) | Specified requirement. Supports AoT via `JsonSerializerContext` source generation |
| Microsoft.Extensions.DependencyInjection | 10.0.x | Replacement for Prism/DryIoc. Lightweight and AoT-compatible |
| Microsoft.Extensions.Logging + ZLogger 2.x | 10.0.x / 2.5.x | Preserves existing logging structure. ZLogger 2 is source generator-based and AoT-compatible |
| TUnit | Latest stable version (0.x line, floating) | Specified requirement |
| Moq or NSubstitute | Latest stable version | For mock testing of Models/Services (used alongside TUnit) |
| System.Drawing.Common | 6.0.x (conditional reference only when targeting Windows) | Executable icon extraction (isolated as a Windows-only feature) |

**Prohibited**: Prism.*, ReactiveProperty, Utf8Json, MaterialDesignThemes.*, Gu.Wpf.Localization, ProcessX, Microsoft.Xaml.Behaviors.Wpf

---

## 4. Configuration File Compatibility Policy (Backward Compatibility Mandatory)

### 4.1 Compatibility Implementation with System.Text.Json

1. **Key names**: `JsonSerializerOptions.PropertyNamingPolicy = null` (default: property names as-is = PascalCase). Matches Utf8Json output.
2. **Culture**: Create a new `JsonConverter<CultureInfo>` to read and write the `{"Name":"ja-JP"}` object format. Fully compatible with the legacy `CultureInfoJsonFormatter`.
3. **enum**: Serialize `Theme` as a numeric value (same as System.Text.Json default; Utf8Json is also numeric).
4. **Constructors**: Add `[JsonConstructor]` to the `RootItem`/`Brand` constructors accepting `ICollection<T>`. System.Text.Json can assign `List<T>` to `ICollection<T>`.
5. **Ignored properties**: `Item.Icon` is marked with `[JsonIgnore]` (equivalent to former `[IgnoreDataMember]`).
6. **Read/Write**: Maintain byte array-based operations (`File.ReadAllBytesAsync`/`WriteAllBytesAsync`) and write without UTF-8 BOM (same as Utf8Json).
7. **Source generation**: Define `JsonSerializerContext` (`AppSettingsContext`, `GameSettingsContext`) for AoT support, and serialize via `TypeInfoResolver`. Disable reflection-based fallback (`JsonSerializerOptions.TypeInfoResolver = context`).

### 4.2 Save Path Compatibility

- Configuration directory: Maintain `{Base Directory}/settings/`.
- Base directory resolution logic is unified to `AppContext.BaseDirectory` (DEBUG/RELEASE branching removed). Matches existing RELEASE behavior (`settings/` adjacent to the executable).
- Acceptance condition is that existing users' `settings/appSettings.json`, `settings/gameSettings.json`, and `Assets/*.png` can be read as-is.

---

## 5. Screen Design Policy

- **Theme**: `FluentTheme` (added to `Application.Styles`). Switch Light/Dark/Sync via `Application.RequestedThemeVariant` (`ThemeVariant.Light`/`Dark`/`Default`).
- **Screen composition**: Provide equivalent functionality for the 5 existing screens. Strict layout adherence is not required.
  - `MainWindow`: Grid/wrap panel icon list (equivalent of ListBox + WrapPanel can be reproduced with `ItemsControl` + `WrapPanel`), back/forward buttons, right-click context menu (Add/Edit/Delete/Settings), brand name display.
  - `AddBrandWindow` / `AddProductWindow`: Modal dialogs (`Window.ShowDialog<TResult>()`). Name input, icon selection/preview, executable file path selection (for products), OK/Cancel.
  - `SettingWindow`: Language ComboBox (displaying NativeName), theme selection (Sync/Light/Dark), OK/Cancel/Apply.
  - `MessageWindow`: Message + details (collapsible) + Close. Used for launch confirmation, deletion confirmation, duplicate warning, and critical exception display.
- **Localization**: ViewModels expose string properties and notify in batch upon language change. XAML binds via `x:CompileBindings`.
- **Dialog service**: Implement custom `IDialogService` (`ShowDialogAsync<TResult>(Window dialog)`) as a replacement for Prism's `IDialogService`. `DialogViewModelBase` defines a custom `RequestClose` event and returns results via `TaskCompletionSource`.
- **Window position saving**: Functionality equivalent to existing `SaveWindowPosition` is **out of scope** because it does not exist in the appSettings.json schema (Top/Left/Height/Width are not persisted in the current implementation either).

---

## 6. Native AoT Support Policy

1. Set `PublishAot=true` / `IsAotCompatible=true`, achieving zero build-time AoT warnings (IL2xxx/IL3xxx).
2. **System.Text.Json**: Source generation context is mandatory (reflection serialization prohibited).
3. **resx**: `ResourceManager` is AoT-compatible. Pay attention to trimming of satellite assemblies (ja-JP/en-US), and explicitly specify `SatelliteResourceLanguages`.
4. **Avalonia**: 11.x officially supports AoT. Eliminate reflection bindings with `AvaloniaUseCompiledBindingsByDefault=true`.
5. **Eliminating reflection usage locations**: DryIoc → Microsoft.Extensions.DI (AoT-compatible), `Assembly.GetExecutingAssembly().Location` → `AppContext.BaseDirectory`.
6. **System.Drawing.Common**: Windows only. Separate AoT publish targets into win-x64 / linux-x64; disable icon extraction functionality on linux-x64 (`OperatingSystem.IsWindows()` guard + allow linker stripping).
7. CI-equivalent verification: `dotnet publish -c Release -r win-x64` / `-r linux-x64` succeeds, and generated binaries launch (smoke test).

---

## 7. Stages and Acceptance Criteria

### Phase 1: Foundation (Coder)

| # | Details | Acceptance Criteria |
|:--|:-----|:---------|
| 1 | Create `feat/AvaloniaMigration` branch, create new Avalonia project (csproj/Program.cs/App.axaml/Program startup) | `dotnet build` succeeds. Empty MainWindow is displayed |
| 2 | Port Core layer (Item/Brand/Product/RootItem/AppSettings/HistoryCollection/Theme + STJ converter + JsonSerializerContext) | Deserialization succeeds with compatibility test data capable of reading legacy JSON files |
| 3 | Port Services layer (File/AppSetting/GameSetting/Resource/Theme/Dispatcher/CommonDialog) | DI registration completed in a unit-testable manner. `ExecuteAsync` works based on Process.Start |

### Phase 2: Feature Implementation (Coder)

| # | Details | Acceptance Criteria |
|:--|:-----|:---------|
| 4 | Port Models layer (all Models including MainModel history management, CRUD, duplicate check) | MainModel primary flow (Load→Select→Add→Edit→Remove→Save) operates |
| 5 | Implement ViewModels layer (CommunityToolkit.Mvvm conversion + dialog infrastructure) | All commands interlock with CanExecute. Absence of Prism/ReactiveProperty |
| 6 | Implement Views (5 screens + FluentTheme + localization + theme switching) | Japanese/English switching and theme switching reflected immediately. All screens displayable and operable |

### Phase 3: Quality (Tester)

| # | Details | Acceptance Criteria |
|:--|:-----|:---------|
| 7 | Implement TUnit tests (Core compatibility, Services, Models) | All `dotnet test` pass. JSON compatibility tests verified with legacy format fixtures |
| 8 | Integration and AoT verification | `dotnet publish` succeeds on win-x64/linux-x64, 0 AoT warnings, smoke test passes on startup |

### Phase 4: Review (Reviewer)

| # | Details | Acceptance Criteria |
|:--|:-----|:---------|
| 9 | Code review (compliance with requirements, prohibited libraries, design quality) | 0 prohibited library references, all items in requirements checklist cleared, feedback addressed |

---

## 8. Risks and Considerations

1. **Exact reproduction of Utf8Json output**: Unindented; property order follows declaration order. System.Text.Json also defaults to declaration order, ensuring compatibility. Reading legacy files is the primary objective, and new saves using STJ output present no issue.
2. **Avalonia `Bitmap`**: `Image.Source` uses `Avalonia.Media.Imaging.Bitmap`. For asynchronous loading, generate on the UI thread after reading the file with `Task.Run`, or use `Bitmap.DecodeToWidth`, etc.
3. **ItemsControl + WrapPanel**: In Avalonia, `WrapPanel` can be specified for `ItemsControl.ItemsPanel`. Reproducing the WPF ListBox style is addressed by replacing `ListBox` + `ItemsPanel`.
4. **ObservableCollection in MainModel → Avalonia**: `ObservableCollection<T>` can be used as-is. An `AddRange` extension must be implemented independently (does not exist in `System.Collections.ObjectModel`).
5. **Immediate response to language changes**: Because resx is a static class, a mechanism is needed where `ResourceService` fires `PropertyChanged` upon language change, causing each VM to reacquire strings.
