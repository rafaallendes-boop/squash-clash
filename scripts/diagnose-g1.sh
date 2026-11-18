#!/usr/bin/env bash
# Experimento explícito de G1, aislado de las suites de regresión.
set -euo pipefail
if [[ $# != 1 || ( "$1" != run01 && "$1" != run02 ) ]]; then
    printf 'Uso: scripts/diagnose-g1.sh run01|run02\n' >&2
    exit 1
fi
source "$(dirname "$0")/unity-env.sh"
run="$1"
evidence="$ROOT/Logs/experiments/g1/$run-runner"
if [[ -e "$evidence" ]]; then
    printf 'Ya existe evidencia de esta invocación: %s\n' "$evidence" >&2
    exit 1
fi
mkdir -p "$evidence"
# El inventario incluye PID, comando y proyecto. No se cierra otro Editor.
python3 - "$evidence/unity-processes.tsv" <<'PY'
import datetime, re, shlex, subprocess, sys
from pathlib import Path
rows = []
for line in subprocess.check_output(['ps', '-axo', 'pid=,command='], text=True).splitlines():
    match = re.match(r'\s*(\d+)\s+(.*)', line)
    if not match:
        continue
    pid, command = match.groups()
    if not re.search(r'/Unity\.app/Contents/MacOS/Unity(?:\s|$)', command):
        continue
    args = shlex.split(command)
    project = args[args.index('-projectPath')+1] if '-projectPath' in args else '(sin -projectPath)'
    rows.append((pid, command, project))
output = 'utc\tpid\tcommand\tproject\n'
utc = datetime.datetime.now(datetime.timezone.utc).isoformat()
output += ''.join('\t'.join((utc, *row))+'\n' for row in rows)
if not rows:
    output += utc+'\tNONE\t\t\n'
Path(sys.argv[1]).write_text(output)
print(output, end='')
if rows:
    sys.exit('STOP: ya hay un Editor Unity; no se lanza G1.')
PY
export SC_G1_RUN="$run"
started_ns="$(python3 -c 'import time; print(time.time_ns())')"
status=0
"$UNITY" -batchmode -nographics -projectPath "$ROOT" -runTests -testPlatform EditMode \
    -assemblyNames SquashClash.EditMode.Experiments \
    -testFilter SquashClash.Experiments.EditMode.G1BackCourtDiagnosisExperiment.WriteG1BackCourtDiagnosis \
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
if (int(sys.argv[3]) != 0 or result.get('result') != 'Passed'
        or int(result.get('total', 0)) != 1 or int(result.get('passed', 0)) != 1
        or int(result.get('failed', 0)) != 0 or int(result.get('skipped', 0)) != 0
        or not log.exists() or re.search(r'error CS\d+|Scripts have compiler errors', log.read_text(errors='replace'))):
    sys.exit('STOP: experimento incompleto/fallido; conservar evidencia, sin reintento.')
PY
