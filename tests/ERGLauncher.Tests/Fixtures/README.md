# Legacy Utf8Json fixtures

`appSettings.json` and `gameSettings.json` are byte-for-byte snapshots of the
legacy application's Utf8Json output, captured before the System.Text.Json
migration. Keep their PascalCase keys, numeric enum values, compact layout, and
UTF-8 encoding without a BOM unchanged.

`Core/JsonCompatibilityTests.cs` reads the fixture bytes directly. It verifies
that the new source-generated System.Text.Json contexts can deserialize the
legacy payloads and continue to emit the same public JSON shape without a BOM.
