# レガシー Utf8Json フィクスチャ

[English](README.md) | [日本語](README.ja.md)


`appSettings.json` および `gameSettings.json` は、レガシー設定スキーマの生成されたバイトスナップショットです。`tools/LegacyUtf8JsonFixtureGenerator` にあるジェネレーターは、レガシー WPF 形式と同じ `Utf8Json` 1.3.7 パッケージ、プロパティの順序、カルチャフォーマッター、数値のテーマ値、および無視されるランタイム `Icon` プロパティを使用します。したがって、チェックインされているファイルは手動で作成された JSON ではなく、実際の `Utf8Json.JsonSerializer.Serialize` の出力です。

リポジトリのルートから、以下を実行して両方のフィクスチャを再生成します。

```sh
dotnet run --project tools/LegacyUtf8JsonFixtureGenerator/LegacyUtf8JsonFixtureGenerator.csproj -- \
  tests/ERGLauncher.Tests/Fixtures
```

ジェネレーターはシリアライザーのバイトを `File.WriteAllBytes` で直接書き込みます。テキストライターを経由せず、BOM や末尾の改行は追加しません。先頭のバイトとハッシュは以下で確認します。

```sh
xxd -g1 -l3 tests/ERGLauncher.Tests/Fixtures/appSettings.json
xxd -g1 -l3 tests/ERGLauncher.Tests/Fixtures/gameSettings.json
sha256sum tests/ERGLauncher.Tests/Fixtures/*.json
```

両方の `xxd` 出力は、UTF-8 BOM の `ef bb bf` ではなく、`7b 22`（`{"`）で始まる必要があります。再生成後の想定される SHA-256 値は以下のとおりです。

```text
4ca1069f0cef62bd67f4c43f4155225dae38acf6de7837433898e8033b292e99  appSettings.json
00d1464efdc64b802f881b03c77dfe066ac38fd4c952fc468833215379dddc6d  gameSettings.json
```

`Core/JsonCompatibilityTests.cs` はこれらと完全に一致するバイトを読み込み、ソース生成された System.Text.Json コンテキストがレガシーペイロードをデシリアライズし、引き続き PascalCase のキー、`{"Name":"..."}` 形式の `Culture`、数値の enum 値を出力し、シリアライズされた `Item.Icon` プロパティを出力しないことを検証します。
