#!/usr/bin/env bash
set -euo pipefail
case "${1:-}" in baseline|off|soft|adjusted) exp=f1-fatigue;; f2off|f2all) exp=f2-fatigue; export SC_EXPERIMENT=f2;; *) exit 2;; esac
source "$(dirname "$0")/unity-env.sh"
export SC_F1_VARIANT="$1"
evidence="$ROOT/Logs/experiments/$exp/$1-runner"
[[ ! -e "$evidence" ]] || { printf 'STOP: evidence exists\n' >&2; exit 1; }
mkdir -p "$evidence"
python3 "$ROOT/scripts/f1-unity-inventory.py" "$evidence/unity-processes.tsv"
git rev-parse HEAD > "$evidence/head.txt"
git status --porcelain > "$evidence/status.txt"
[[ ! -s "$evidence/status.txt" ]] || { printf 'STOP: dirty source\n' >&2; exit 1; }
started_ns="$(python3 -c 'import time; print(time.time_ns())')"
status=0
"$UNITY" -batchmode -nographics -projectPath "$ROOT" -runTests -testPlatform EditMode \
 -assemblyNames SquashClash.EditMode.Experiments -testFilter SquashClash.Experiments.EditMode.FatigueMatchExperiment.WriteMatches \
 -testResults "$evidence/results.xml" -logFile "$evidence/unity.log" > "$evidence/console.log" 2>&1 || status=$?
python3 - "$evidence" "$started_ns" "$status" <<'PY'
import sys,re
from pathlib import Path
from xml.etree import ElementTree as ET
p=Path(sys.argv[1]);xml=p/'results.xml';log=p/'unity.log'
assert xml.is_file() and xml.stat().st_mtime_ns>int(sys.argv[2]),'No fresh XML'
r=ET.parse(xml).getroot().attrib
print('RESULT:',r)
assert log.is_file() and not re.search(r'error CS\d+|Scripts have compiler errors',log.read_text(errors='replace')),'Compiler errors'
assert int(sys.argv[3])==0 and r.get('result')=='Passed' and int(r.get('total',0))==int(r.get('passed',0))==1 and int(r.get('skipped',0))==0,'Experiment stopped; preserve evidence'
PY
