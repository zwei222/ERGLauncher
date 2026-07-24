# ERGLauncher Avalonia UI 移行計画

## 1. 背景と目的

ERGLauncher（WPF / .NET 6 / Prism.DryIoc + ReactiveProperty + MaterialDesignThemes.MahApps）を、Avalonia UI / .NET 10 / CommunityToolkit.Mvvm ベースのクロスプラットフォームアプリケーションへ移行する。

### 移行要件（必須）

| 要件 | 内容 |
|:-----|:-----|
| ターゲットフレームワーク | .NET 10 |
| プラットフォーム | Windows / Linux（主） |
| Native AoT | 対応可能な構成にする |
| MVVM | CommunityToolkit.Mvvm（ReactiveProperty / Prism は使用しない） |
| JSON シリアライザ | System.Text.Json（Utf8Json は使用しない） |
| 設定ファイル互換 | 既存の `settings/appSettings.json`・`settings/gameSettings.json` をそのまま読み書き可能にする（後方互換必須） |
| UI デザイン | Fluent Design（Avalonia FluentTheme） |
| 画面構成 | 既存アプリの厳密な踏襲は不要（同等機能を提供） |
| UnitTest | TUnit |
| ブランチ | `feat/AvaloniaMigration` で作業（Coder が作成/チェックアウト） |

---

## 2. 既存コード調査結果

### 2.1 設定ファイル JSON スキーマ

#### appSettings.json

保存先: `{アプリベースディレクトリ}/settings/appSettings.json`

```json
{
  "Culture": { "Name": "ja-JP" },
  "Theme": 2
}
```

- シリアライザ: **Utf8Json**（キーは C# プロパティ名 PascalCase のまま、camelCase ではない）
- `Culture`: `CultureInfoJsonFormatter` により `{"Name":"カルチャ名"}` オブジェクト形式で保存
- `Theme`: `Theme` enum の数値（0=None, 1=Light, 2=Dark, 3=Sync）

#### gameSettings.json

保存先: `{アプリベースディレクトリ}/settings/gameSettings.json`

```json
{
  "Brands": [
    {
      "Name": "ブランド名",
      "IconPath": "Assets\\xxxx.png または null",
      "Products": [
        {
          "Name": "ゲーム名",
          "IconPath": "Assets\\yyyy.png または null",
          "Path": "C:\\Games\\xxx\\game.exe",
          "BrandName": "ブランド名"
        }
      ]
    }
  ]
}
```

- `Item.Icon`（`BitmapImage`）は `[IgnoreDataMember]` で JSON 保存対象外。ロード時に `IconPath` から画像を復元する。
- `Brand` / `RootItem` は `[SerializationConstructor]` で `ICollection<T>` をコンストラクタ経由で受け取る。
- ベースディレクトリは DEBUG 時は `Assembly.GetExecutingAssembly().Location`、RELEASE 時は `Process.GetCurrentProcess().MainModule.FileName` の親ディレクトリ。

### 2.2 責務と相互依存

| レイヤ | 主要クラス | 責務 |
|:-------|:-----------|:-----|
| Core | `Item`(abstract), `Brand`, `Product`, `RootItem`, `AppSettings`, `HistoryCollection<T>`, `Theme` | ドメインエンティティ。`Item` は Prism `BindableBase` を継承し `Name`/`IconPath`/`Icon` を保持 |
| Services | `IFileService`/`FileService`, `IAppSettingService`, `IGameSettingService`, `IResourceService`, `IThemeService`, `IDispatcherService`, `ICommonDialogService` | ファイル I/O・JSON 永続化・プロセス起動・ローカライズ・テーマ切替・UI スレッド制御・ファイルダイアログ |
| Models | `IMainModel`/`MainModel`, `IAddBrandModel`, `IAddProductModel`, `ISettingModel`, `IMessageModel` | ビジネスロジック。Services を組み合わせて CRUD・履歴管理（HistoryCollection）・設定適用を行う |
| ViewModels | `MainViewModel`, `AddBrandViewModel`, `AddProductViewModel`, `SettingViewModel`, `MessageViewModel` | ReactiveProperty で Model をラップし、コマンド（`ReactiveCommand`/`AsyncReactiveCommand`）を公開。Prism `IDialogService` / `IDialogAware` でダイアログ制御 |
| Views | `MainView`(MetroWindow), `AddBrandView`, `AddProductView`, `SettingView`, `MessageView`(UserControl + prism:Dialog.WindowStyle) | MahApps.Metro + MaterialDesign の XAML。`Gu.Wpf.Localization` の `{gu:Static}` でローカライズ |

依存の流れ: `App`(PrismApplication, DryIoc) → Views ⇄ ViewModels → Models → Services → Core

### 2.3 置換ポイント一覧

| 既存依存 | 使用箇所 | 置換先 |
|:---------|:---------|:-------|
| **Prism.DryIoc** | `App.xaml.cs`（PrismApplication, RegisterTypes, ConfigureViewModelLocator）, `DialogViewModelBase`(IDialogAware), `IMainModel.DialogCoordinator`, Views の `prism:ViewModelLocator.AutoWireViewModel` | Microsoft.Extensions.DependencyInjection + 自前ダイアログサービス（または CommunityToolkit.Mvvm の Messenger） |
| **ReactiveProperty** | 全 ViewModel（`ReactiveProperty<T>`, `ReadOnlyReactivePropertySlim<T>`, `ReactiveCommand`, `AsyncReactiveCommand`, `BusyNotifier`, `ToReactivePropertyAsSynchronized`） | CommunityToolkit.Mvvm `ObservableObject` + `[ObservableProperty]` + `[RelayCommand]` + `ObservableValidator`。Model→VM の同期は手動プロパティ変更通知または `ObservableObject` 直接継承に簡素化 |
| **Utf8Json** | `AppSettings.cs`, `Brand.cs`, `RootItem.cs`（属性）, `AppSettingService`, `GameSettingService`, `CultureInfoJsonFormatter` | System.Text.Json + カスタム `JsonConverter<CultureInfo>`。`[JsonConstructor]` 対応 |
| **MaterialDesignThemes.MahApps** | `App.xaml`（マージド辞書）, `ViewBase`(MetroWindow), MainView/各ダイアログのスタイル, `ThemeService`(ControlzEx ThemeManager) | Avalonia FluentTheme + 標準スタイル。テーマ切替は `Application.RequestedThemeVariant` |
| **Gu.Wpf.Localization / Gu.Localization** | 全 View の `{gu:Static properties:Resources.Xxx}`, `ResourceService`(Translator), SettingView の `Translator.Cultures` バインド | resx は .NET 標準 `ResourceManager` で引き続き利用可能（Avalonia でも `Properties.Resources` は機能する）。XAML バインドは `{Binding Source={x:Static properties:Resources.Xxx}}` ではなく、VM 経由または `IValueConverter` で提供する方針 |
| **Microsoft.Xaml.Behaviors.Wpf** | MainView の `Interaction.Triggers`（Loaded/Closing/MouseLeftButtonUp → InvokeCommandAction） | Avalonia の `Behavior`（Avalonia.Xaml.Behaviors パッケージ）またはイベントハンドラで代替 |
| **ProcessX** (Cysharp.Diagnostics) | `FileService.ExecuteAsync`（cmd.exe /c "..." で起動し stdout/stderr を非同期列挙） | `System.Diagnostics.Process` で置換。クロスプラットフォームのため `cmd.exe` 直指定をやめ、`ProcessStartInfo { FileName = filePath, WorkingDirectory = ..., UseShellExecute = true }` で起動 |
| **System.Drawing.Common** | `AddProductModel.SelectFileAsync` の `Icon.ExtractAssociatedIcon`, `FileService.SaveBitmapAsync`(Bitmap→PNG) | Windows: 引き続き `System.Drawing.Common`（Windows のみサポート）か `Shell32` 経由。Linux: 代替としてアイコン抽出をスキップするか、`Avalonia.Media.Imaging.Bitmap` への変換レイヤを用意。**要設計判断**：実行ファイルアイコン自動抽出は Windows 限定機能とし、Linux ではデフォルトアイコンを使用する方針が現実的 |
| **BitmapImage (System.Windows.Media.Imaging)** | `Item.Icon`, `IFileService.CreateBitmapImageAsync`, `GameSettingService` ロード処理 | `Avalonia.Media.Imaging.Bitmap` |
| **ZLogger** | `App.xaml.cs`（ILoggerFactory + AddZLoggerRollingFile）, 各 Model/Service の `logger.ZLogXxx` | 継続利用可能（ZLogger は .NET 標準）。AoT 対応のためソースジェネレータ版を使用 |

### 2.4 ローカライズ

- `Properties/Resources.resx`（デフォルト=en）, `Resources.en-US.resx`, `Resources.ja-JP.resx` の 3 ファイル。29 キー。
- WPF では `Gu.Wpf.Localization` の `{gu:Static}` で XAML 直接参照 + `Translator.Culture` で動的切替。
- Avalonia 移行後: resx はそのまま `Properties.Resources` として利用可能（`PublicResXFileCodeGenerator`）。XAML からの直接参照はできないため、各 ViewModel が `IResourceService` 経由で文字列を公開する方式に変更。言語切替時は `INotifyPropertyChanged` で全文字列プロパティを再通知。

### 2.5 プラットフォーム依存リスク

| 箇所 | リスク | 対応方針 |
|:-----|:-------|:---------|
| `FileService.ExecuteAsync` の `cmd.exe /c` | Windows 専用 | `Process.Start` with `UseShellExecute=true` に変更（OS 任せの関連付け起動） |
| `Icon.ExtractAssociatedIcon` | Windows 専用 API | Windows のみ有効。Linux では null を返しデフォルトアイコンを使用 |
| パス区切り `\\` | gameSettings.json 内の `Path`/`IconPath` が Windows パス | そのまま保存・読込。Linux で同一設定を使うケースは想定外（ユーザーごとのローカル設定のため許容） |
| `System.Drawing.Common` | .NET 6+ では Windows のみサポート（Linux では `PlatformNotSupportedException`） | Windows 専用コードパスに `#if WINDOWS` または `OperatingSystem.IsWindows()` でガード |

---

## 3. プロジェクト構成案

### 3.1 ソリューション/プロジェクト構成

```
ERGLauncher.sln
src/
  ERGLauncher/                      # Avalonia UI アプリ本体（net10.0）
    ERGLauncher.csproj
    App.axaml / App.axaml.cs        # Application + FluentTheme + DI 初期化
    Program.cs                      # [STAThread] Main → BuildAvaloniaApp
    ViewModels/
      ViewModelBase.cs              # ObservableObject 継承
      MainViewModel.cs
      AddBrandViewModel.cs
      AddProductViewModel.cs
      SettingViewModel.cs
      MessageViewModel.cs
    Views/
      MainWindow.axaml(.cs)         # Window（旧 MainView）
      AddBrandWindow.axaml(.cs)     # ダイアログは Window.ShowDialog に変更
      AddProductWindow.axaml(.cs)
      SettingWindow.axaml(.cs)
      MessageWindow.axaml(.cs)
    Models/                          # 既存 IModel/実装を CommunityToolkit.Mvvm 化して移植
    Services/                        # 既存インターフェースをほぼ踏襲して移植
    Core/                            # Item, Brand, Product, RootItem, AppSettings, HistoryCollection, Theme
    Converters/                      # EnumToBooleanConverter 等（IValueConverter 移植）
    Properties/
      Resources.resx                 # 既存を流用
      Resources.en-US.resx
      Resources.ja-JP.resx
    Assets/
      icon.png                       # 既存を流用（デフォルトアイコン）
tests/
  ERGLauncher.Tests/                 # TUnit テストプロジェクト（net10.0）
    ERGLauncher.Tests.csproj
    Core/HistoryCollectionTests.cs
    Core/JsonCompatibilityTests.cs   # 既存 JSON フォーマット互換性テスト
    Services/AppSettingServiceTests.cs
    Services/GameSettingServiceTests.cs
    Models/MainModelTests.cs
```

**設計判断**: 既存の ViewModels は ReactiveProperty への依存が深い（`ToReactivePropertyAsSynchronized` による双方向同期）ため、Avalonia 版では **Model 層を廃止または薄くし、ViewModel が直接 Services を呼ぶ構成** を推奨する。ただし既存ロジック（MainModel の履歴管理・CRUD・重複チェック）は複雑なため、Model 層を CommunityToolkit.Mvvm の `ObservableObject` ベースで維持し、ViewModel から手動でプロパティをバインドするハイブリッド方式も許容する。本計画では **Model 層を維持し、INotifyPropertyChanged 実装を CommunityToolkit.Mvvm に置き換える** 方針とする（差分が最小で、テスト容易性が保たれる）。

### 3.2 csproj 骨格（ERGLauncher.csproj）

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

### 3.3 依存ライブラリ選定とバージョン方針

| ライブラリ | バージョン方針 | 選定理由 |
|:-----------|:---------------|:---------|
| Avalonia / Avalonia.Desktop / Avalonia.Themes.Fluent / Avalonia.Fonts.Inter | **11.3.x（浮動パッチ）** | 安定版最新系。FluentTheme が要件。Desktop で Windows/Linux 両対応 |
| Avalonia.Xaml.Behaviors | 11.3.x（Avalonia と同一系） | WPF Behaviors の代替（Loaded/Closing トリガ等） |
| CommunityToolkit.Mvvm | 8.4.x | 要件指定。ソースジェネレータで AoT フレンドリ |
| System.Text.Json | .NET 10 同梱（パッケージ参照不要） | 要件指定。`JsonSerializerContext` ソースジェネレーションで AoT 対応 |
| Microsoft.Extensions.DependencyInjection | 10.0.x | Prism/DryIoc の代替。軽量で AoT 互換 |
| Microsoft.Extensions.Logging + ZLogger 2.x | 10.0.x / 2.5.x | 既存ロギング構造を維持。ZLogger 2 はソースジェネレータベースで AoT 対応 |
| TUnit | 最新安定版（0.x 系、浮動） | 要件指定 |
| Moq または NSubstitute | 最新安定版 | Model/Service のモックテスト用（TUnit と併用） |
| System.Drawing.Common | 6.0.x（Windows ターゲット時のみ条件付き参照） | 実行ファイルアイコン抽出（Windows 限定機能として隔離） |

**禁止**: Prism.*, ReactiveProperty, Utf8Json, MaterialDesignThemes.*, Gu.Wpf.Localization, ProcessX, Microsoft.Xaml.Behaviors.Wpf

---

## 4. 設定ファイル互換方針（後方互換必須）

### 4.1 System.Text.Json での互換実装

1. **キー名**: `JsonSerializerOptions.PropertyNamingPolicy = null`（デフォルト: プロパティ名そのまま = PascalCase）。Utf8Json の出力と一致。
2. **Culture**: `JsonConverter<CultureInfo>` を新規作成し、`{"Name":"ja-JP"}` オブジェクト形式を読み書き。旧 `CultureInfoJsonFormatter` と完全互換。
3. **enum**: `Theme` は数値としてシリアライズ（System.Text.Json デフォルトと同じ。Utf8Json も数値）。
4. **コンストラクタ**: `RootItem`/`Brand` の `ICollection<T>` 受け取りコンストラクタには `[JsonConstructor]` を付与。System.Text.Json は `ICollection<T>` に対し `List<T>` を割り当て可能。
5. **無視プロパティ**: `Item.Icon` は `[JsonIgnore]`（旧 `[IgnoreDataMember]` 相当）。
6. **読み書き**: バイト配列ベース（`File.ReadAllBytesAsync`/`WriteAllBytesAsync`）を維持し、UTF-8 BOM なしで書き込む（Utf8Json と同じ）。
7. **ソースジェネレーション**: AoT 対応のため `JsonSerializerContext`（`AppSettingsContext`, `GameSettingsContext`）を定義し、`TypeInfoResolver` 経由でシリアライズ。リフレクションベースのフォールバックは無効化（`JsonSerializerOptions.TypeInfoResolver = context`）。

### 4.2 保存パス互換

- 設定ディレクトリ: `{ベースディレクトリ}/settings/` を維持。
- ベースディレクトリ決定ロジックは `AppContext.BaseDirectory` に統一（DEBUG/RELEASE 分岐を廃止）。既存の RELEASE 時挙動（exe 隣接 `settings/`）と一致。
- 既存ユーザーの `settings/appSettings.json`・`settings/gameSettings.json`・`Assets/*.png` をそのまま読み込めることを受入条件とする。

---

## 5. 画面設計方針

- **テーマ**: `FluentTheme`（`Application.Styles` に追加）。Light/Dark/Sync は `Application.RequestedThemeVariant` を切替（`ThemeVariant.Light`/`Dark`/`Default`）。
- **画面構成**: 既存 5 画面の同等機能を提供。厳密なレイアウト踏襲は不要。
  - `MainWindow`: グリッド/ラップパネルのアイコン一覧（ListBox + WrapPanel 相当は `ItemsControl` + `WrapPanel` で再現可）、戻る/進むボタン、右クリックコンテキストメニュー（追加/編集/削除/設定）、ブランド名表示。
  - `AddBrandWindow` / `AddProductWindow`: モーダルダイアログ（`Window.ShowDialog<TResult>()`）。名前入力、アイコン選択・プレビュー、（製品は）実行ファイルパス選択、OK/Cancel。
  - `SettingWindow`: 言語 ComboBox（NativeName 表示）、テーマ選択（Sync/Light/Dark）、OK/Cancel/Apply。
  - `MessageWindow`: メッセージ + 詳細（折りたたみ可）+ Close。起動確認・削除確認・重複警告・致命的例外表示に使用。
- **ローカライズ**: ViewModel が文字列プロパティを公開し、言語変更時に一括通知。XAML は `x:CompileBindings` でバインド。
- **ダイアログサービス**: Prism `IDialogService` の代替として、独自 `IDialogService`（`ShowDialogAsync<TResult>(Window dialog)`）を実装。`DialogViewModelBase` は `RequestClose` イベントを独自定義し、`TaskCompletionSource` で結果を返す。
- **ウィンドウ位置保存**: 既存の `SaveWindowPosition` 相当機能は appSettings.json スキーマに存在しないため **対象外**（現行も Top/Left/Height/Width は永続化されていない）。

---

## 6. Native AoT 対応方針

1. `PublishAot=true` / `IsAotCompatible=true` を設定し、ビルド時の AoT 警告（IL2xxx/IL3xxx）をゼロにする。
2. **System.Text.Json**: ソースジェネレーションコンテキスト必須（リフレクションシリアライズ禁止）。
3. **resx**: `ResourceManager` は AoT 互換。サテライトアセンブリ（ja-JP/en-US）のトリミングに注意し、`SatelliteResourceLanguages` を明示。
4. **Avalonia**: 11.x は AoT 対応を公式サポート。`AvaloniaUseCompiledBindingsByDefault=true` でリフレクションバインドを排除。
5. **リフレクション使用箇所の排除**: DryIoc → Microsoft.Extensions.DI（AoT 互換）、`Assembly.GetExecutingAssembly().Location` → `AppContext.BaseDirectory`。
6. **System.Drawing.Common**: Windows のみ。AoT  publish は win-x64 / linux-x64 を分け、linux-x64 ではアイコン抽出機能を無効化（`OperatingSystem.IsWindows()` ガード + リンカーによる除去を許容）。
7. CI 相当の検証: `dotnet publish -c Release -r win-x64` / `-r linux-x64` が成功し、生成バイナリが起動すること（Smoke test）。

---

## 7. 工程と受入条件

### Phase 1: 基盤（Coder）

| # | 内容 | 受入条件 |
|:--|:-----|:---------|
| 1 | `feat/AvaloniaMigration` ブランチ作成、Avalonia プロジェクト新設（csproj/Program.cs/App.axaml/Program 起動） | `dotnet build` 成功。空の MainWindow が表示される |
| 2 | Core 層移植（Item/Brand/Product/RootItem/AppSettings/HistoryCollection/Theme + STJ コンバータ + JsonSerializerContext） | 旧 JSON ファイルを読み込める互換性テスト用データでデシリアライズ成功 |
| 3 | Services 層移植（File/AppSetting/GameSetting/Resource/Theme/Dispatcher/CommonDialog） | 単体テスト可能な形で DI 登録完了。`ExecuteAsync` が Process.Start ベースで動作 |

### Phase 2: 機能実装（Coder）

| # | 内容 | 受入条件 |
|:--|:-----|:---------|
| 4 | Models 層移植（MainModel の履歴管理・CRUD・重複チェック含む全 Model） | MainModel の主要フロー（Load→Select→Add→Edit→Remove→Save）が動作 |
| 5 | ViewModels 層実装（CommunityToolkit.Mvvm 化 + ダイアログ基盤） | 全コマンドが CanExecute 連動。Prism/ReactiveProperty 不在 |
| 6 | Views 実装（5 画面 + FluentTheme + ローカライズ + テーマ切替） | 日英切替・テーマ切替が即時反映。全画面が表示・操作可能 |

### Phase 3: 品質（Tester）

| # | 内容 | 受入条件 |
|:--|:-----|:---------|
| 7 | TUnit テスト実装（Core 互換性・Services・Models） | `dotnet test` 全パス。JSON 互換性テストが旧フォーマット fixture で検証済み |
| 8 | 結合・AoT 検証 | win-x64/linux-x64 で `dotnet publish` 成功、AoT 警告 0、起動 Smoke test 通過 |

### Phase 4: レビュー（Reviewer）

| # | 内容 | 受入条件 |
|:--|:-----|:---------|
| 9 | コードレビュー（要件遵守・禁止ライブラリ・設計品質） | 禁止ライブラリ参照 0、要件チェックリスト全項目クリア、指摘対応完了 |

---

## 8. リスクと留意点

1. **Utf8Json の厳密な出力再現**: インデントなし・プロパティ順は宣言順。System.Text.Json もデフォルトで宣言順のため互換。旧ファイルの読み込みが主目的であり、新規保存は STJ 出力で問題ない。
2. **Avalonia の `Bitmap`**: `Image.Source` には `Avalonia.Media.Imaging.Bitmap`。非同期ロードは `Task.Run` でファイル読み込み後に UI スレッドで生成するか、`Bitmap.DecodeToWidth` 等を使用。
3. **ItemsControl + WrapPanel**: Avalonia では `ItemsControl.ItemsPanel` に `WrapPanel` を指定可能。WPF の ListBox スタイル再現は `ListBox` + `ItemsPanel` 差替で対応。
4. **MainModel の ObservableCollection→Avalonia**: `ObservableCollection<T>` はそのまま使用可能。`AddRange` 拡張は自前実装が必要（`System.Collections.ObjectModel` には存在しない）。
5. **言語切替の即時反映**: resx は静的クラスのため、言語変更時は `ResourceService` が `PropertyChanged` を発火し、各 VM が文字列を再取得する仕組みが必要。
