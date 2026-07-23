# T-3004 Native AoT 差戻し修正・再検証

- 判定: **Linux 要件は合格 / Windows x64 検証は Linux build 環境のため対象外**
- 対象ブランチ: `feat/AvaloniaMigration`
- ベース commit: `a131cdc3c9e94097ccf1dcf39b6487f68870779e`
- 変更 commit: Kanban 完了記録に記載
- 実行日時: 2026-07-24
- 実行環境: Linux 7.0.9+parrot7-amd64, x64, .NET SDK 10.0.302

## 修正内容

1. `MainView.axaml` に `x:CompileBindings="True"` と `x:DataType="views:IMainViewDataContext"` を設定し、項目テンプレートにも `core:Item` の型を付与した。これにより到達可能な全 MainView binding が ReflectionBinding から compiled binding になった。
2. `IMainViewDataContext` を追加し、MainView の binding surface を型付きで定義した。
3. publish 済み実行ファイルから呼び出せる `--settings-smoke <base-directory>` を追加した。旧形式 JSON の読込、ブランド/製品の Create・Read・Update・Delete、app/game settings 保存、サービス再生成後の永続化、別プロセス再起動後の読込を検証する。
4. compiled binding 契約テストと settings smoke 統合テストを追加した。

## 検証結果

| ID | コマンド / 手順 | 結果 | 証跡 |
|---|---|---|---|
| B-01 | `dotnet build ERGLauncher.sln -c Release --no-restore --warnaserror --verbosity minimal` | PASS。警告 0、エラー 0、exit 0 | `build-warnaserror.log` |
| T-01 | `dotnet test ERGLauncher.sln -c Release --no-restore --verbosity minimal` | PASS。62 passed / 0 failed / 0 skipped、exit 0 | `test-integration.log` |
| A-02 | `dotnet publish src/ERGLauncher/ERGLauncher.csproj -c Release -r linux-x64 --self-contained` | PASS。exit 0、IL2xxx/IL3xxx 0件 | `publish-linux-x64.log` |
| S-01 | publish ELF を旧 fixture に対し `--settings-smoke` で起動 | PASS。旧 app/game settings 読込、CRUD、保存、サービス再生成後の永続化、exit 0 | `smoke-linux-x64.log` |
| S-02 | 同じ publish ELF を同じ保存先に対して別プロセスで再起動 | PASS。保存済み culture/theme/brand/product を再読込、exit 0 | `smoke-linux-x64.log` |
| A-01 | Linux host で win-x64 Native AoT publish 可否を確認 | SDK が `Cross-OS native compilation is not supported.`、exit 1。Windows build 環境時のみの条件なので判定対象外 | `publish-win-x64.log` |


## Linux publish smoke の具体的な確認値

- 成果物: ELF 64-bit x86-64 Native AoT executable
- 初回: `culture=ja-JP theme=Dark brands=1` を旧 JSON から読込
- CRUD: brand/product create、product rename update、削除対象 product delete
- 保存後: `appSettings.json` は `Culture.Name=en-US`, `Theme=1`
- 同一プロセス内サービス再生成: `RESTART_PERSISTENCE=PASS`
- 別プロセス再起動: `CRUD_READ_AFTER_RESTART=PASS`
- 最終 SHA-256:
  - appSettings.json: `8b5a626e0016ce13f4ab0737e6da1398e1b5d6d60c394bfbf95bd7c45c1520a9`
  - gameSettings.json: `7976ebc542ada19b246e3f4604a8ac87a6c572c7f0543ad277539232a16ccb00`

## Windows 条件の扱い

受入条件 2 は build 環境が Windows の場合のみ適用される。実行環境は Linux x64 であり、Linux からの win-x64 Native AoT cross compilation は .NET SDK 非対応のため、Windows publish と `.exe` smoke は実行していない。未実行結果を成功扱いにはせず、条件対象外として記録する。

## 残存リスク

Windows x64 上での Native AoT publish と publish 済み `.exe` の settings smoke は未実測である。Windows build 環境で本変更を検証する際は、`publish-win-x64.log` と `.exe --settings-smoke` の完全ログを取得すること。
