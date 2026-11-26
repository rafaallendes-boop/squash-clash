#!/usr/bin/env bash
# Experimento explícito de las posiciones de saque (spec 2026-11-26-posiciones-saque), aislado de las suites de regresión.
#   scripts/diagnose-serve-positions.sh before|after|after-aims|after-aims2 [partidos por estilo, 100 por defecto]
# Un solo Unity: el llamador usa el candado global. No sobrescribe evidencia ni reintenta.
set -euo pipefail
case "${1:-}" in
    before|after|after-aims|after-aims2) export SC_POSITIONS_RUN="$1" ;;
    *) printf 'Uso: scripts/diagnose-serve-positions.sh before|after|after-aims|after-aims2 [partidos]\n' >&2; exit 1 ;;
esac
export SC_POSITIONS_MATCHES="${2:-100}"
source "$(dirname "$0")/unity-env.sh"
evidence="$ROOT/Logs/experiments/serve-positions/$1-runner"
if [[ -e "$evidence" ]]; then printf 'Ya existe evidencia: %s\n' "$evidence" >&2; exit 1; fi
mkdir -p "$evidence"
if pgrep -f "[U]nity.app/Contents/MacOS/Unity" >/dev/null; then echo 'STOP: ya hay un Editor Unity.' >&2; exit 1; fi
git -C "$ROOT" rev-parse HEAD > "$evidence/head.txt"
git -C "$ROOT" status --porcelain > "$evidence/status.txt"
started_ns="$(python3 -c 'import time; print(time.time_ns())')"
status=0
"$UNITY" -batchmode -nographics -projectPath "$ROOT" -runTests -testPlatform EditMode \
    -assemblyNames SquashClash.EditMode.Experiments \
    -testFilter "SquashClash.Experiments.EditMode.ServePositionsExperiment.MeasureServePositions" \
    -testResults "$evidence/results.xml" -logFile "$evidence/unity.log" > "$evidence/console.log" 2>&1 || status=$?
python3 - "$evidence" "$started_ns" "$status" <<'PY'
import re, sys
from pathlib import Path
from xml.etree import ElementTree as ET
root = Path(sys.argv[1])
xml, log = root/'results.xml', root/'unity.log'
if not xml.exists() or xml.stat().st_mtime_ns <= int(sys.argv[2]):
    sys.exit('STOP: falta XML fresco; conservar evidencia, sin reintento.')
result = ET.parse(xml).getroot().attrib
print('RESULT:', {k: result.get(k) for k in ('result', 'total', 'passed', 'failed')})
if (int(sys.argv[3]) != 0 or result.get('result') != 'Passed' or int(result.get('total', 0)) != 1
        or int(result.get('passed', 0)) != 1 or not log.exists()
        or re.search(r'error CS\d+|Scripts have compiler errors', log.read_text(errors='replace'))):
    sys.exit('STOP: experimento incompleto/fallido; conservar evidencia, sin reintento.')
PY
