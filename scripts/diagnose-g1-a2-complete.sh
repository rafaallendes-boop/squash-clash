#!/usr/bin/env bash
# Experimentos explícitos de G1 A2 (completar), aislados de las suites de regresión.
#   scripts/diagnose-g1-a2-complete.sh grid run01|run02
#   scripts/diagnose-g1-a2-complete.sh rally before|after
# Un solo Unity: el llamador usa el candado global. No sobrescribe evidencia ni reintenta.
set -euo pipefail
case "${1:-} ${2:-}" in
    "grid run01"|"grid run02") test=WriteGrid; export SC_A2_RUN="$2" ;;
    "rally before"|"rally after") test=WriteRallyPoints; export SC_A2_RALLY="$2" ;;
    *) printf 'Uso: scripts/diagnose-g1-a2-complete.sh grid run01|run02 | rally before|after\n' >&2; exit 1 ;;
esac
source "$(dirname "$0")/unity-env.sh"
evidence="$ROOT/Logs/experiments/g1a2-complete/$1-$2-runner"
if [[ -e "$evidence" ]]; then printf 'Ya existe evidencia: %s\n' "$evidence" >&2; exit 1; fi
mkdir -p "$evidence"
if pgrep -f "[U]nity.app/Contents/MacOS/Unity" >/dev/null; then echo 'STOP: ya hay un Editor Unity.' >&2; exit 1; fi
started_ns="$(python3 -c 'import time; print(time.time_ns())')"
status=0
"$UNITY" -batchmode -nographics -projectPath "$ROOT" -runTests -testPlatform EditMode \
    -assemblyNames SquashClash.EditMode.Experiments \
    -testFilter "SquashClash.Experiments.EditMode.G1A2CompleteExperiment.$test" \
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
