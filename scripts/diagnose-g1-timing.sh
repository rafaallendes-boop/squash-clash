#!/usr/bin/env bash
# Guardas de esquema y experimento A1-bis, sin ejecutar la grilla de fase A.
set -euo pipefail
if [[ $# != 1 || ( "$1" != run01 && "$1" != run02 && "$1" != schema-red && "$1" != schema-green ) ]]; then
    printf 'Uso: scripts/diagnose-g1-timing.sh schema-red|schema-green|run01|run02\n' >&2
    exit 1
fi
source "$(dirname "$0")/unity-env.sh"
run="$1"
evidence="$ROOT/Logs/experiments/g1-timing/$run-runner"
if [[ -e "$evidence" ]]; then
    printf 'Ya existe evidencia de esta invocación: %s\n' "$evidence" >&2
    exit 1
fi
mkdir -p "$evidence"
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
utc = datetime.datetime.now(datetime.timezone.utc).isoformat()
output = 'utc\tpid\tcommand\tproject\n'+''.join('\t'.join((utc,*row))+'\n' for row in rows)
if not rows:
    output += utc+'\tNONE\t\t\n'
Path(sys.argv[1]).write_text(output)
print(output, end='')
if rows:
    sys.exit('STOP: ya hay un Editor Unity; no se lanza G1.')
PY
export SC_G1_TIMING_RUN="$run"
filter=SquashClash.Experiments.EditMode.G1TimingDiagnosisExperiment.WriteG1TimingDiagnosis
if [[ "$run" == schema-* ]]; then filter=SquashClash.Experiments.EditMode.G1ExportSchemaTests; fi
started_ns="$(python3 -c 'import time; print(time.time_ns())')"
status=0
"$UNITY" -batchmode -nographics -projectPath "$ROOT" -runTests -testPlatform EditMode \
    -assemblyNames SquashClash.EditMode.Experiments -testFilter "$filter" \
    -testResults "$evidence/results.xml" -logFile "$evidence/unity.log" > "$evidence/console.log" 2>&1 || status=$?
python3 - "$evidence" "$started_ns" "$status" "$run" <<'PY'
import re, sys
from pathlib import Path
from xml.etree import ElementTree as ET
root = Path(sys.argv[1])
xml, log = root/'results.xml', root/'unity.log'
if not xml.exists() or xml.stat().st_mtime_ns <= int(sys.argv[2]):
    sys.exit('STOP: falta XML fresco; conservar evidencia, sin reintento.')
tree = ET.parse(xml).getroot()
result = tree.attrib
print('RESULT:', result)
text = log.read_text(errors='replace') if log.exists() else ''
if not log.exists() or re.search(r'error CS\d+|Scripts have compiler errors', text):
    sys.exit('STOP: falta log o hay error CS.')
if sys.argv[4] == 'schema-red':
    assert int(result.get('total', 0)) == 4 and int(result.get('failed', 0)) == 4, result
    assert all(n.find('failure') is not None and n.get('label') != 'Error' for n in tree.iter('test-case')), 'RED no válido por aserción'
    print('RED válido: cuatro aserciones de esquema, sin errores CS; no se ejecutó fase A.')
else:
    expected = 4 if sys.argv[4] == 'schema-green' else 1
    if (int(sys.argv[3]) != 0 or result.get('result') != 'Passed'
            or int(result.get('total', 0)) != expected or int(result.get('passed', 0)) != expected
            or int(result.get('failed', 0)) != 0 or int(result.get('skipped', 0)) != 0):
        sys.exit('STOP: experimento incompleto/fallido; conservar evidencia, sin reintento.')
PY
