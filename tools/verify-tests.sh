#!/usr/bin/env bash
# Canonical MTP/TUnit verification: restore, matching Release build, then test.
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd -- "$SCRIPT_DIR/.." && pwd)"
SOLUTION="$ROOT/ERGLauncher.sln"
RUN_ROOT="$ROOT/qa-artifacts/verification/tests-$(date +%Y%m%d-%H%M%S-%N)"
TMP_DIR="$RUN_ROOT/tmp"
NUGET_PACKAGES_DIR="$RUN_ROOT/nuget-packages"
NUGET_HTTP_CACHE_DIR="$RUN_ROOT/nuget-http-cache"
DOTNET_HOME="$RUN_ROOT/dotnet-home"
TEST_RESULTS_DIR="$RUN_ROOT/TestResults"
COMMANDS_LOG="$RUN_ROOT/commands.log"

mkdir -p "$TMP_DIR" "$NUGET_PACKAGES_DIR" "$NUGET_HTTP_CACHE_DIR" "$DOTNET_HOME" "$TEST_RESULTS_DIR"
export TMPDIR="$TMP_DIR"
export TEMP="$TMP_DIR"
export TMP="$TMP_DIR"
export NUGET_PACKAGES="$NUGET_PACKAGES_DIR"
export NUGET_HTTP_CACHE_PATH="$NUGET_HTTP_CACHE_DIR"
export DOTNET_CLI_HOME="$DOTNET_HOME"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

run_step() {
    local name="$1"
    shift
    local log="$RUN_ROOT/${name}.log"

    printf '$' | tee -a "$COMMANDS_LOG"
    printf ' %q' "$@" | tee -a "$COMMANDS_LOG"
    printf '\n' | tee -a "$COMMANDS_LOG"
    "$@" 2>&1 | tee "$log"
}

printf 'Verification run root: %s\n' "$RUN_ROOT"
printf 'TMPDIR/TEMP/TMP: %s\n' "$TMP_DIR"
printf 'NUGET_PACKAGES: %s\n' "$NUGET_PACKAGES"
printf 'NUGET_HTTP_CACHE_PATH: %s\n' "$NUGET_HTTP_CACHE_PATH"
printf 'DOTNET_CLI_HOME: %s\n' "$DOTNET_CLI_HOME"
printf 'Test results: %s\n' "$TEST_RESULTS_DIR"

# --no-build is valid only after this matching Release build has generated the
# MTP/TUnit runner executables in the same workspace/configuration.
run_step restore dotnet restore "$SOLUTION"
run_step build dotnet build "$SOLUTION" -c Release --no-restore --warnaserror
run_step test dotnet test "$SOLUTION" -c Release --no-build --results-directory "$TEST_RESULTS_DIR"

printf 'Verification completed successfully. Artifacts: %s\n' "$RUN_ROOT"
