# Tester / Reviewer ブロック解消 再計画

作成日: 2026-07-23
対象ボード: erglauncher-avalonia-migration
対象タスク: T-2007（blocked）/ T-2008（blocked）/ T-2009（blocked）、QA 差戻し t_c0e66663

---

## 1. ブロック原因の整理

### 1.1 現状調査で判明した成果物の所在

| 成果物 | 所在 | 状態 |
|:-------|:-----|:-----|
| WPF 旧コード（master） | `/home/hermes/repos/workspace/agent`（master チェックアウト中） | 移行対象の原本 |
| Avalonia 土台（T-2001 相当） | worktree `T-1002`、ブランチなし（コミット `0130482`） | `src/ERGLauncher.Avalonia/` + `tests/ERGLauncher.Avalonia.Tests/` + global.json |
| Core 層 + STJ 互換（T-2002） | ブランチ `feat/AvaloniaMigration-T2002`（コミット `f7fee5d`） | worktree `T-2007` にも展開済み |
| Services 層（T-2003） | worktree `T-1003`（`src/ERGLauncher.Core/` + `tests/ERGLauncher.Core.Tests/`） | コミット未作成（master のまま） |
| Models 層（T-2004） | worktree `t_c0e66663` 内に統合済み（コミット `4ef749c`） | テストも同梱 |
| ViewModels（T-2005） | ブランチ `feat/AvaloniaMigration-T2005` / `feat/AvaloniaMigration-T1004`（コミット `0cacf15`） | worktree `T-1004` |
| Views（T-2006） | worktree `T-1005`（コミット `72adffc`） | `tests/ERGLauncher.Views.*` 同梱 |
| **最終統合候補** | **worktree `t_c0e66663`（ブランチ `task/t_c0e66663`、コミット `158e49e`）** | **T-2002〜T-2005 + Models + テスト 3 プロジェクト（ERGLauncher.Tests / ERGLauncher.Core.Tests / ERGLauncher.ViewModels.Tests）+ Fixtures（appSettings.json / gameSettings.json）が揃った最新状態** |
| global.json（Microsoft.Testing.Platform 強制） | `t_c0e66663` / `T-2007` / `T-1002` / `T-1004` の各ルート | テストプロジェクト側の構成と不整合 |
| ユーザーワークスペースの `feat/AvaloniaMigration` ブランチ | `/home/hermes/repos/workspace/agent` | コミット `35d9806`（スキャフォールドのみ）。**T-2002〜T-2006・テストは未統合** |

### 1.2 QA 指摘（t_c0e66663 body より）

1. **テスト実行構成の不整合**: root の global.json が `"test": { "runner": "Microsoft.Testing.Platform" }` を強制する一方、`tests/ERGLauncher.Tests/ERGLauncher.Tests.csproj` が VSTest 扱いになり `dotnet test --no-restore` が exit 1 で停止。TUnit が一件も実行されない。`dotnet run` も OutputType=Library で exit 1。
2. **要求テストファイルの欠如**: 受入指定の `HistoryCollectionTests.cs` / `AppSettingServiceTests.cs` / `GameSettingServiceTests.cs` / `MainModelTests.cs` が `tests/ERGLauncher.Tests/` 配下に存在しない。現存の `CoreModelTests.cs` は History の Push-after-undo のみで、Push/Back/Forward/Remove/Clear/Peek/At・undo/redo 状態の網羅なし。Services/Models テスト未実装。
   - ※補足: `tests/ERGLauncher.Core.Tests/` 側には同名テストが存在するが、T-2007 受入条件が指定する配置（`tests/ERGLauncher.Tests/` 配下の Core/Services/Models）とプロジェクト構成が異なり、かつ実行構成不整合で実走していない。
3. **JSON fixture の証跡不足**: fixture は BOM なし（先頭バイト `7b22...`）だが、実際の Utf8Json 出力由来であることを示す証跡（生成手順・旧アプリでの保存ログ等）がない。
4. **統合の問題**: T-2002 Core / T-2003 Services / T-2004 Models / テストプロジェクトが統合可能な単一 worktree に揃っていない。ユーザーワークスペース `/home/hermes/repos/workspace/agent` には WPF 旧コードのみで、`feat/AvaloniaMigration` ブランチにも移行成果物が未統合。

### 1.3 T-2008 QA レポート（qa-evidence/verification-report.md）の要点

- T-2008 ワークスペースは当初空で、T-1004（ViewModels のみ、`0cacf15`）をコピーして検証 → そのプロジェクトは `net6-windows` + `UseWPF=true` の WPF プロジェクトであり、`dotnet publish` は win-x64 / linux-x64 とも NETSDK1100 で失敗。
- 受入コマンド `dotnet run --project src/ERGLauncher` のパス自体が存在しない（実際は `src/ERGLauncher.csproj`）。
- `AddProductModel.cs` の `Icon.ExtractAssociatedIcon` に OS ガードなし（静的指摘 X1）。
- 結論: クロスプラットフォーム TFM・Avalonia・AoT 対応の**単一の統合リビジョン**がテスターに渡っていないことが根本原因。

### 1.4 根本原因の要約

- **各 Coder タスクが独立 worktree で成果物を残し、単一の統合ブランチ/worktree に集約されていない**（最大の原因）。
- global.json の Microsoft.Testing.Platform 強制とテストプロジェクト構成（VSTest 扱い / TUnit 未設定）の不整合。
- T-2007 指定パスへのテスト実装不足、Utf8Json 実 fixture の証跡不足。
- T-2008 の受入手順（`src/ERGLauncher/ERGLauncher.csproj`）と実際のプロジェクト配置の不一致、AoT 設定未構成。

---

## 2. 解決のための作業分解

### 工程概要

```
T-3001 (coder)  統合ブランチへの成果物集約 + テスト実行構成修正
T-3002 (coder)  欠如テスト実装 + Utf8Json 実 fixture 証跡生成
T-3003 (tester) 単体テスト受入検証（dotnet test 全パス）
T-3004 (tester) 結合テスト・Native AoT・クロスプラットフォーム検証
T-3005 (reviewer) コードレビュー（要件遵守・禁止ライブラリ・設計品質）
```

### T-3001（coder / priority 1）: 統合ブランチ集約とテスト実行構成の修正

**目的**: テスターがそのまま検証できる「単一の統合 worktree + ブランチ」を提供する。

**作業内容**:
1. ユーザーワークスペース `/home/hermes/repos/workspace/agent` で `feat/AvaloniaMigration` ブランチをチェックアウト。
2. 最終統合候補 worktree `~/.hermes/kanban/boards/erglauncher-avalonia-migration/workspaces/t_c0e66663`（コミット `158e49e`）の内容を基盤とし、以下を集約:
   - `src/ERGLauncher/`（Avalonia アプリ本体。T-2005 Views・T-2004 Models・T-2003 Services・T-2002 Core を含む統合版。ただし T-2006 Views 成果物 `T-1005` worktree の View/XAML 変更が欠けていないか突合し、不足分は T-1005 コミット `72adffc` から取り込む）
   - `src/ERGLauncher.Core/`（T-2003 成果物）
   - `tests/ERGLauncher.Tests/` / `tests/ERGLauncher.Core.Tests/` / `tests/ERGLauncher.ViewModels.Tests/` / （存在すれば）`tests/ERGLauncher.Views.Tests`
   - root `global.json`、ソリューションファイル（.sln / .slnx）
3. **テスト実行構成の修正（QA 指摘 1 の解消）**:
   - 全テストプロジェクト（`tests/**/*.csproj`）を Microsoft.Testing.Platform 構成に統一: `<OutputType>Exe</OutputType>`、TUnit パッケージ（`TUnit` 最新）と `Microsoft.Testing.Extensions.TrxReport` 等の必要拡張を明示し、VSTest 用パッケージ（`Microsoft.NET.Test.Sdk` / `coverlet.collector` 等の VSTest 依存）は TUnit + MTP 構成と整合する形に整理する。
   - root `global.json` の `"test": { "runner": "Microsoft.Testing.Platform" }` を維持したまま、`dotnet test tests/ERGLauncher.Tests/ERGLauncher.Tests.csproj --no-restore` が exit 0 で TUnit テストを実走させることを確認する。
   - アプリ本体 `src/ERGLauncher/ERGLauncher.csproj` は `<OutputType>WinExe</OutputType>`（Avalonia デスクトップアプリ）とし、`dotnet run --project src/ERGLauncher/ERGLauncher.csproj` が Linux で起動することを確認（OutputType=Library で exit 1 となる指摘の解消）。受入コマンドのパス表記（`src/ERGLauncher` vs `src/ERGLauncher/ERGLauncher.csproj`）を実配置に合わせて統一し、ハンドオフ文書に明記する。
4. **クロスプラットフォーム TFM への統一（QA 指摘・T-2008 指摘の解消）**:
   - アプリ本体を `net10.0`（Windows 限定 TFM ではない）+ Avalonia 参照に統一。WPF 依存（`UseWPF`, `net6-windows`）の残存があれば除去。
   - `PublishAot` / `IsAotCompatible` / `InvariantGlobalization` 等の AoT 設定を csproj に追加。
   - `Icon.ExtractAssociatedIcon` 等の Windows 限定 API は `OperatingSystem.IsWindows()` でガードし、Linux では既定アイコンを使用するフォールバックを実装（T-2008 静的指摘 X1 の解消）。
5. `dotnet build -c Release --warnaserror` 成功、`dotnet test`（通常実行）で全テスト実行・成功をローカル確認。
6. `feat/AvaloniaMigration` ブランチへコミット（Conventional Commits）。コミットハッシュ・`dotnet test` 完全ログ・テスト数をタスク body に記録。

**成果物**: 統合済み `feat/AvaloniaMigration` ブランチ（ユーザーワークスペース）+ 検証可能な worktree。

### T-3002（coder / priority 2）: 欠如テスト実装と Utf8Json 実 fixture 証跡の生成

**依存**: T-3001。

**作業内容**:
1. **T-2007 指定パスへのテスト実装（QA 指摘 2 の解消）** — すべて TUnit:
   - `tests/ERGLauncher.Tests/Core/HistoryCollectionTests.cs`: Push / Back / Forward / Remove / Clear / Peek / At の各操作、`IsEnabledUndo` / `IsEnabledRedo` の状態遷移、undo 後の Push で redo 側がクリアされる回帰。
   - `tests/ERGLauncher.Tests/Core/JsonCompatibilityTests.cs`: 旧形式 fixture のデシリアライズ、新規シリアライズ結果のキー構造一致（PascalCase / Culture は `{"Name":"..."}` / enum は数値）、`Item.Icon` が JSON に含まれないこと。
   - `tests/ERGLauncher.Tests/Services/AppSettingServiceTests.cs`: 未存在時 null、Save→Load ラウンドトリップ。
   - `tests/ERGLauncher.Tests/Services/GameSettingServiceTests.cs`: 空/未存在で空の RootItem、ラウンドトリップ、CreateBrandItemAsync / CreateProductItemAsync。
   - `tests/ERGLauncher.Tests/Models/MainModelTests.cs`: LoadSettingAsync で Items にブランド読込、AddItemAsync（重複時は追加されない）、RemoveItemAsync、Back/Forward 履歴遷移。`IFileService` / `IGameSettingService` / `IAppSettingService` を Moq または NSubstitute で差し替え可能であることの実証。
   - 既存 `tests/ERGLauncher.Core.Tests/` 側の同名テストと内容が重複する場合は、T-2007 指定プロジェクトへ集約・移管し、重複実行で二重計上されないよう整理する。
2. **Utf8Json 実 fixture の証跡生成（QA 指摘 3 の解消）**:
   - 旧 WPF アプリ（Utf8Json シリアライザ）または旧フォーマットと同一のシリアライズ設定（Utf8Json 相当: PascalCase キー・CultureInfo は `{"Name":...}`・enum 数値・`[IgnoreDataMember]` 除外）で `appSettings.json` / `gameSettings.json` を生成し、`tests/ERGLauncher.Tests/Fixtures/` に配置。
   - 生成手順（使用コード/ツール・コマンド・先頭バイト `7b22...` = BOM なし UTF-8 であることの確認方法）を `tests/ERGLauncher.Tests/Fixtures/README.md`（新規）に記録し、fixture の由来を追跡可能にする。
3. 正常系・異常系・境界・回帰を網羅し、`dotnet test`（通常実行、global.json の MTP 構成のまま）が全テスト実行・成功することを確認。
4. コミットし、完全ログ・テスト数・コミットハッシュを body に記録。

### T-3003（tester / priority 3）: 単体テスト受入検証

**依存**: T-3002。T-2007 の受入を実質的に引き継ぐ。

**検証内容**:
1. 統合 worktree（`feat/AvaloniaMigration`）で `dotnet test tests/ERGLauncher.Tests/ERGLauncher.Tests.csproj --no-restore --verbosity normal` が exit 0 で TUnit 全テストを実行すること（QA 指摘 1 の再検証）。
2. 指定 5 テストファイル（HistoryCollectionTests / JsonCompatibilityTests / AppSettingServiceTests / GameSettingServiceTests / MainModelTests）が `tests/ERGLauncher.Tests/` 配下に存在し、Core/JSON 互換/Services/Models の正常・異常・境界・回帰を網羅していること。
3. JSON 互換性テストが実 Utf8Json 形式 fixture（BOM なし、生成手順の証跡あり）で検証されていること。
4. モック（Moq または NSubstitute）によるサービス差し替えが実証されていること。
5. 全ログ・テスト数・対象 git commit を記録し、合格なら done、不合格なら指摘を body に記録して coder へ差し戻し。

### T-3004（tester / priority 4）: 結合テスト・Native AoT・クロスプラットフォーム検証

**依存**: T-3003。T-2008 の受入を実質的に引き継ぐ。

**検証内容**:
1. 結合テスト: 起動 → ブランド追加 → 製品追加 → 設定変更 → 終了 → 再起動 → 永続化確認。旧形式 `settings/appSettings.json` / `settings/gameSettings.json` 配置下での読み込み確認。
2. Native AoT: `dotnet publish src/ERGLauncher/ERGLauncher.csproj -c Release -r win-x64 --self-contained` と `-r linux-x64` の両方が成功し、AoT 警告（IL2xxx / IL3xxx）が 0 件であること。publish バイナリの smoke test（起動・設定読み書き）。
   - Linux ホストから win-x64 向け publish が必要な場合に備え、csproj 側で必要な設定（Windows ターゲティングの扱い）を確認し、不可能な場合はその旨と代替証跡を明記する。
3. クロスプラットフォーム: Linux で `Icon.ExtractAssociatedIcon` 相当がスキップされ既定アイコンが使われること、パス区切り文字の適切な処理。
4. 結果はログ/スクリーンショットで `qa-evidence/` に記録。不合格なら coder へ差し戻し。

### T-3005（reviewer / priority 5）: コードレビュー

**依存**: T-3004。T-2009 の受入を実質的に引き継ぐ。

**レビュー観点**（T-2009 チェックリストを踏襲）:
1. 要件遵守: .NET 10 / CommunityToolkit.Mvvm（ReactiveProperty・Prism なし）/ System.Text.Json（Utf8Json なし）/ 設定 JSON スキーマ後方互換 / FluentTheme / TUnit。
2. 禁止ライブラリ 0 件: `Prism.*`, `ReactiveProperty`, `Utf8Json`, `MaterialDesignThemes.*`, `Gu.Wpf.Localization`, `ProcessX`, `Microsoft.Xaml.Behaviors.Wpf`。
3. 設計品質: MVVM / DI / プラットフォーム依存コードのガード（`OperatingSystem.IsWindows()` 等）/ AoT 対応（ソースジェネレーション・リフレクション排除）。
4. コード品質: `dotnet build --warnaserror` 成功、命名・コメントの一貫性。
5. `git diff master..feat/AvaloniaMigration` をレビューし、指摘があれば coder へ差し戻し、全クリアで done。

---

## 3. 受入条件（再計画全体）

QA 指摘をすべてカバーする以下の条件を、新タスク群の総合受入条件とする:

1. root `global.json` の `Microsoft.Testing.Platform` 構成のまま、通常の `dotnet test` が **全テストプロジェクトの全 TUnit テストを実行し成功**する（exit 0）。VSTest 扱いによる停止がないこと。
2. 指定テストファイル 5 件（`HistoryCollectionTests.cs` / `JsonCompatibilityTests.cs` / `AppSettingServiceTests.cs` / `GameSettingServiceTests.cs` / `MainModelTests.cs`）が `tests/ERGLauncher.Tests/` 配下に実装されていること。
3. JSON 互換性検証が**実 Utf8Json 出力由来の fixture**（BOM なし・生成手順の証跡文書付き）で行われていること。
4. T-2002 Core / T-2003 Services / T-2004 Models / T-2005 ViewModels / T-2006 Views / テストプロジェクトが、ユーザーワークスペース `/home/hermes/repos/workspace/agent` の **`feat/AvaloniaMigration` ブランチに統合された単一 worktree** として提供されていること。
5. win-x64 / linux-x64 の `dotnet publish` 成功、AoT 警告 0 件、publish バイナリの smoke test 成功。
6. 各工程で**完全ログ・テスト数・git commit ハッシュ**がタスク body に記録されていること。
7. `dotnet build --warnaserror` 成功、禁止ライブラリ参照 0 件。

---

## 4. 既存 blocked タスク（T-2007 / T-2008 / T-2009）の扱い

- **方針: 新タスク（T-3003 / T-3004 / T-3005）に置き換える。** 既存タスクはステータスを `blocked` のまま維持し、body 末尾に「本タスクは再計画により T-3003（T-3004 / T-3005）に置き換え。受入条件は新タスク側で管理する。」旨の注記を追記する。
- 理由: 既存タスクの受入条件記述は有効だが、前提となる「統合 worktree の提供」が欠けたままでは tester/reviewer が着手不能であり、工程を coder から組み直す新連番タスクに依存関係を移す方が dispatcher の優先度制御（priority 順）と整合するため。
- QA 差戻しタスク `t_c0e66663`（status=done 扱い）は、指摘事項が T-3001 / T-3002 の作業内容に全て織り込まれたことを body 注記で明記する（履歴として残す）。

## 5. 移行要件（全タスク共通・引き継ぎ）

.NET 10 / Windows・Linux クロスプラットフォーム / Native AoT 対応 / MVVM / CommunityToolkit.Mvvm（ReactiveProperty 禁止）/ System.Text.Json（Utf8Json 禁止）/ Prism 禁止 / 既存 JSON 設定ファイルの後方互換 / Fluent Design（FluentTheme）/ TUnit / `feat/AvaloniaMigration` ブランチで記録 / ワークスペース `/home/hermes/repos/workspace/agent`

## 6. 参照

- 元計画: `docs/avalonia-migration-plan.md`（セクション 1, 6, 7）
- QA 差戻し: Kanban タスク `t_c0e66663` body
- T-2008 QA レポート: `~/.hermes/kanban/boards/erglauncher-avalonia-migration/workspaces/T-2008/qa-evidence/verification-report.md`
