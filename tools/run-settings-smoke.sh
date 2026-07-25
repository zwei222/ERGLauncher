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
NUGET_PACKAGES_DIR="$RUN_ROOT/nuget-packages"
NUGET_HTTP_CACHE_DIR="$RUN_ROOT/nuget-http-cache"
DOTNET_HOME="$RUN_ROOT/dotnet-home"
TEST_RESULTS_DIR="$RUN_ROOT/TestResults"
LOG="$RUN_ROOT/settings-smoke.log"
FIXTURE_HASHES_BEFORE="$RUN_ROOT/fixture-sha256-before.txt"
FIXTURE_HASHES_AFTER="$RUN_ROOT/fixture-sha256-after.txt"

mkdir -p "$SETTINGS_DIR" "$TMP_DIR" "$NUGET_PACKAGES_DIR" "$NUGET_HTTP_CACHE_DIR" "$DOTNET_HOME" "$TEST_RESULTS_DIR"
export TMPDIR="$TMP_DIR"
export TEMP="$TMP_DIR"
export TMP="$TMP_DIR"
export NUGET_PACKAGES="$NUGET_PACKAGES_DIR"
export NUGET_HTTP_CACHE_PATH="$NUGET_HTTP_CACHE_DIR"
export DOTNET_CLI_HOME="$DOTNET_HOME"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

for fixture in appSettings.json gameSettings.json; do
    if [[ ! -s "$FIXTURES/$fixture" ]]; then
        printf 'Required non-empty tracked fixture is missing: %s\n' "$FIXTURES/$fixture" >&2
        exit 1
    fi
done

sha256sum "$FIXTURES/appSettings.json" "$FIXTURES/gameSettings.json" > "$FIXTURE_HASHES_BEFORE"
cp -- "$FIXTURES/appSettings.json" "$SETTINGS_DIR/appSettings.json"
cp -- "$FIXTURES/gameSettings.json" "$SETTINGS_DIR/gameSettings.json"

printf 'Settings smoke run root: %s\n' "$RUN_ROOT" | tee "$LOG"
printf 'TMPDIR/TEMP/TMP: %s\n' "$TMP_DIR" | tee -a "$LOG"
printf 'NUGET_PACKAGES: %s\n' "$NUGET_PACKAGES" | tee -a "$LOG"
printf 'NUGET_HTTP_CACHE_PATH: %s\n' "$NUGET_HTTP_CACHE_PATH" | tee -a "$LOG"
printf 'DOTNET_CLI_HOME: %s\n' "$DOTNET_CLI_HOME" | tee -a "$LOG"
printf 'Reserved test results: %s\n' "$TEST_RESULTS_DIR" | tee -a "$LOG"
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
