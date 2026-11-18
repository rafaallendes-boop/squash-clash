#!/usr/bin/env bash
# Diagnóstico G1 A2 D1 (solo lectura de producto): una invocación Unity por proceso, run01 o run02.
set -euo pipefail
if [[ $# != 1 || ( "$1" != run01 && "$1" != run02 ) ]]; then
    printf 'Uso: scripts/diagnose-g1-a2-d1.sh run01|run02\n' >&2
    exit 1
fi
source "$(dirname "$0")/unity-env.sh"
run="$1"
evidence="$ROOT/Logs/experiments/g1-a2-d1/$run-runner"
if [[ -e "$evidence" ]]; then
    printf 'Ya existe evidencia de esta invocación: %s\n' "$evidence" >&2
    exit 1
fi
if pgrep -fl "[U]nity.app/Contents/MacOS/Unity" > /dev/null; then
    printf 'STOP: ya hay un Editor Unity; no se lanza D1.\n' >&2
    exit 1
fi
mkdir -p "$evidence"
export SC_A2_D1_RUN="$run"
started_ns="$(python3 -c 'import time; print(time.time_ns())')"
status=0
"$UNITY" -batchmode -nographics -projectPath "$ROOT" -runTests -testPlatform EditMode \
    -assemblyNames SquashClash.EditMode.Experiments \
    -testFilter SquashClash.Experiments.EditMode.G1A2D1Experiment.WriteG1A2D1 \
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
print('RESULT:', result)
text = log.read_text(errors='replace') if log.exists() else ''
if not log.exists() or re.search(r'error CS\d+|Scripts have compiler errors', text):
    sys.exit('STOP: falta log o hay error CS.')
if (int(sys.argv[3]) != 0 or result.get('result') != 'Passed' or int(result.get('total', 0)) != 1
        or int(result.get('passed', 0)) != 1 or int(result.get('failed', 0)) != 0 or int(result.get('skipped', 0)) != 0):
    sys.exit('STOP: experimento incompleto/fallido; conservar evidencia, sin reintento.')
PY
