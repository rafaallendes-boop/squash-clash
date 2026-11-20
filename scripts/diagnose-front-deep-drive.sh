#!/usr/bin/env bash
# Experimento explícito «drive profundo desde adelante» (20 nov), aislado de las suites de regresión.
#   SC_FDD_METRICS=<carpeta de métricas del PO> scripts/diagnose-front-deep-drive.sh baseline baseline01|baseline02
#   SC_FDD_METRICS=<carpeta de métricas del PO> scripts/diagnose-front-deep-drive.sh fix fix01|fix02
# Un solo Unity: el llamador usa el candado global. No sobrescribe evidencia ni reintenta.
set -euo pipefail
case "${1:-} ${2:-}" in
    "baseline baseline01"|"baseline baseline02") test=WriteBaseline; export SC_FDD_RUN="$2" ;;
    "fix fix01"|"fix fix02") test=WriteFix; export SC_FDD_RUN="$2" ;;
    *) printf 'Uso: scripts/diagnose-front-deep-drive.sh baseline baseline01|baseline02 | fix fix01|fix02\n' >&2; exit 1 ;;
esac
[[ -d "${SC_FDD_METRICS:-}" ]] || { printf 'Falta SC_FDD_METRICS.\n' >&2; exit 1; }
source "$(dirname "$0")/unity-env.sh"
evidence="$ROOT/Logs/experiments/front-deep-drive/$2-runner"
if [[ -e "$evidence" ]]; then printf 'Ya existe evidencia: %s\n' "$evidence" >&2; exit 1; fi
mkdir -p "$evidence"
if pgrep -f "[U]nity.app/Contents/MacOS/Unity" >/dev/null; then echo 'STOP: ya hay un Editor Unity.' >&2; exit 1; fi
started_ns="$(python3 -c 'import time; print(time.time_ns())')"
status=0
"$UNITY" -batchmode -nographics -projectPath "$ROOT" -runTests -testPlatform EditMode \
    -assemblyNames SquashClash.EditMode.Experiments \
    -testFilter "SquashClash.Experiments.EditMode.FrontDeepDriveExperiment.$test" \
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
