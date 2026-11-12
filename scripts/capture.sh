#!/usr/bin/env bash
# Renders the Main scene to PNG (practice, rally in Demo, match) through the SquashClash.Capture PlayMode fixture.
# Needs graphics: runs without -nographics. Usage: scripts/capture.sh <set> [output-dir] [fixture]
# fixture: ZVisualCapture (default, PNG captures) or ZVisualPerf (editor measurement, writes perf-<set>.json).
set -euo pipefail
if [[ $# -lt 1 || $# -gt 3 ]]; then
    printf 'Uso: scripts/capture.sh <conjunto> [directorio] [ZVisualCapture|ZVisualPerf]\n' >&2
    exit 1
fi
source "$(dirname "$0")/unity-env.sh"
mkdir -p "$ROOT/Logs"
export SQUASH_CAPTURE_SET="$1"
export SQUASH_CAPTURE_DIR="${2:-$ROOT/Logs/capture}"
results="$ROOT/Logs/capture-results.xml"
log="$ROOT/Logs/capture.log"
rm -f "$results"
status=0
"$UNITY" -batchmode -projectPath "$ROOT" -runTests -testPlatform PlayMode \
    -assemblyNames SquashClash.Capture -testFilter "${3:-ZVisualCapture}" -testResults "$results" -logFile "$log" > "$ROOT/Logs/capture-console.log" 2>&1 || status=$?
if [[ ! -f "$results" ]] || ! grep -q 'result="Passed"' "$results" || grep -Eq 'error CS[0-9]+' "$log"; then
    printf 'FAIL: la captura no terminó bien (Unity %s). Ver %s\n' "$status" "$log" >&2
    exit 1
fi
if [[ "${3:-ZVisualCapture}" == ZVisualPerf ]]; then
    cat "$SQUASH_CAPTURE_DIR/perf-$SQUASH_CAPTURE_SET.json"
else
    printf 'CAPTURE: %s\n' "$SQUASH_CAPTURE_DIR/$SQUASH_CAPTURE_SET"
    ls "$SQUASH_CAPTURE_DIR/$SQUASH_CAPTURE_SET"
fi
