#!/usr/bin/env bash
# Canonical MTP/TUnit verification: restore, matching Release build, then test.
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd -- "$SCRIPT_DIR/.." && pwd)"
SOLUTION="$ROOT/ERGLauncher.sln"
RUN_ROOT="$ROOT/qa-artifacts/verification/tests-$(date +%Y%m%d-%H%M%S-%N)"
TMP_DIR="$RUN_ROOT/tmp"
COMMANDS_LOG="$RUN_ROOT/commands.log"

mkdir -p "$TMP_DIR" "$RUN_ROOT/TestResults"
export TMPDIR="$TMP_DIR"
export TEMP="$TMP_DIR"
export TMP="$TMP_DIR"

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

# --no-build is valid only after this matching Release build has generated the
# MTP/TUnit runner executables in the same workspace/configuration.
run_step restore dotnet restore "$SOLUTION"
run_step build dotnet build "$SOLUTION" -c Release --no-restore --warnaserror
run_step test dotnet test "$SOLUTION" -c Release --no-build --results-directory "$RUN_ROOT/TestResults"

printf 'Verification completed successfully. Artifacts: %s\n' "$RUN_ROOT"
