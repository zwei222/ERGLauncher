#!/usr/bin/env bash
# Stage immutable legacy fixtures into a per-run workspace before SettingsSmoke.
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd -- "$SCRIPT_DIR/.." && pwd)"
PROJECT="$ROOT/src/ERGLauncher/ERGLauncher.csproj"
FIXTURES="$ROOT/tests/ERGLauncher.Tests/Fixtures"
RUN_ROOT="$ROOT/qa-artifacts/verification/settings-smoke-$(date +%Y%m%d-%H%M%S-%N)"
SETTINGS_DIR="$RUN_ROOT/settings"
TMP_DIR="$RUN_ROOT/tmp"
LOG="$RUN_ROOT/settings-smoke.log"
FIXTURE_HASHES_BEFORE="$RUN_ROOT/fixture-sha256-before.txt"
FIXTURE_HASHES_AFTER="$RUN_ROOT/fixture-sha256-after.txt"

mkdir -p "$SETTINGS_DIR" "$TMP_DIR"
export TMPDIR="$TMP_DIR"
export TEMP="$TMP_DIR"
export TMP="$TMP_DIR"

for fixture in appSettings.json gameSettings.json; do
    if [[ ! -s "$FIXTURES/$fixture" ]]; then
        printf 'Required non-empty tracked fixture is missing: %s\n' "$FIXTURES/$fixture" >&2
        exit 1
    fi
done

sha256sum "$FIXTURES/appSettings.json" "$FIXTURES/gameSettings.json" > "$FIXTURE_HASHES_BEFORE"
cp -- "$FIXTURES/appSettings.json" "$SETTINGS_DIR/appSettings.json"
cp -- "$FIXTURES/gameSettings.json" "$SETTINGS_DIR/gameSettings.json"

# --no-build needs a matching Release launcher. Build the smallest target only
# when this workspace has not produced it yet.
if [[ ! -x "$ROOT/src/ERGLauncher/bin/Release/net10.0/ERGLauncher" ]]; then
    printf 'Release launcher output is absent; building %s first.\n' "$PROJECT" | tee "$RUN_ROOT/build-message.log"
    dotnet build "$PROJECT" -c Release 2>&1 | tee "$RUN_ROOT/build-release.log"
fi

printf 'Settings smoke run root: %s\n' "$RUN_ROOT" | tee "$LOG"
printf 'Command: dotnet run --project %s -c Release --no-build -- --settings-smoke %s\n' "$PROJECT" "$RUN_ROOT" | tee -a "$LOG"
if ! dotnet run --project "$PROJECT" -c Release --no-build -- --settings-smoke "$RUN_ROOT" 2>&1 | tee -a "$LOG"; then
    printf 'Settings smoke command failed; log: %s\n' "$LOG" >&2
    exit 1
fi

for expected in SETTINGS_LOAD CRUD_CREATE CRUD_UPDATE_DELETE SETTINGS_SAVE RESTART_PERSISTENCE SETTINGS_SMOKE; do
    if ! grep -Eq "^${expected}=PASS([[:space:]]|$)" "$LOG"; then
        printf 'Missing expected smoke result %s=PASS; log: %s\n' "$expected" "$LOG" >&2
        exit 1
    fi
done

sha256sum "$FIXTURES/appSettings.json" "$FIXTURES/gameSettings.json" > "$FIXTURE_HASHES_AFTER"
if ! cmp -s "$FIXTURE_HASHES_BEFORE" "$FIXTURE_HASHES_AFTER"; then
    printf 'Tracked fixtures changed during smoke execution; see %s and %s\n' \
        "$FIXTURE_HASHES_BEFORE" "$FIXTURE_HASHES_AFTER" >&2
    exit 1
fi

printf 'Fixture hashes unchanged. Artifacts: %s\n' "$RUN_ROOT"
