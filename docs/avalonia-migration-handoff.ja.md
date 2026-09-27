# Avalonia移行 統合引き継ぎ

[English](avalonia-migration-handoff.md) | [日本語](avalonia-migration-handoff.ja.md)


統合検証用ワークツリーはブランチ `feat/AvaloniaMigration` の `/home/hermes/repos/workspace/agent` です。

リポジトリのルートから実行してください:

```bash
dotnet build -c Release --warnaserror
dotnet test
dotnet test tests/ERGLauncher.Tests/ERGLauncher.Tests.csproj --no-restore
dotnet run --project src/ERGLauncher/ERGLauncher.csproj
```

`global.json` ではテストランナーとして `Microsoft.Testing.Platform` を維持しています。本ソリューションには、4つのTUnit実行可能テストプロジェクト `ERGLauncher.Tests`、`ERGLauncher.Core.Tests`、`ERGLauncher.ViewModels.Tests`、および `ERGLauncher.Views.Tests` が含まれています。

アプリケーションプロジェクトは `src/ERGLauncher/ERGLauncher.csproj` です（レガシーなルートレベルの `src/ERGLauncher.csproj` ではありません）。`net10.0` をターゲットとし、Avalonia FluentTheme を使用し、`PublishAot` と `IsAotCompatible` が有効化されています。デスクトップ起動コマンドには稼働中のX11/Waylandディスプレイが必要です。ヘッドレスシェルでは、ウィンドウが作成される前に `XOpenDisplay` で失敗します。

2026-07-23の統合検証:

- Releaseビルド: 0警告、0エラーで成功。
- Microsoft.Testing.Platform 全体実行: 全4プロジェクトで60件成功、0件失敗、0件スキップ。
- 対象を絞った `ERGLauncher.Tests` の no-restore 実行: 6件成功、0件失敗、0件スキップ。
- デスクトップコマンド: 実行可能ファイルはAvaloniaプラットフォームの初期化に到達しましたが、ワーカーシェルに承認されたディスプレイがありませんでした（`XOpenDisplay failed`）。GUIスモークテストは、デスクトップ環境が利用可能なテスター向けに残されています。
