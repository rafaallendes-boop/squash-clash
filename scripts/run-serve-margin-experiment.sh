#!/usr/bin/env bash
set -euo pipefail
source scripts/unity-env.sh
kind="$1"
run="$2"
case "$kind" in
  options) filter=SquashClash.Experiments.EditMode.ServeReceptionMarginExperiment.MeasureBoundedMargin; export SC_MARGIN_RUN="$run" ;;
  n2) filter=SquashClash.Experiments.EditMode.RivalStylesCalibrationExperiment.ConfirmBoundedServeMargin; export SC_RS_RUN="$run"; export SC_MARGIN_MS="$3" ;;
  *) exit 2 ;;
esac
evidence="$ROOT/Logs/evidence/$run"
[[ ! -e "$evidence" ]] || { printf 'STOP: existing evidence\n'; exit 2; }
mkdir -p "$evidence"
git rev-parse HEAD > "$evidence/head.txt"
git status --porcelain > "$evidence/status.txt"
[[ ! -s "$evidence/status.txt" ]] || { printf 'STOP: dirty source\n'; exit 2; }
until mkdir /Users/rafaallendes/sc-unity.lock 2>/dev/null; do sleep 30; done
trap 'rmdir /Users/rafaallendes/sc-unity.lock' EXIT
if pgrep -f '[U]nity.app/Contents/MacOS/Unity' >/dev/null; then printf 'STOP: existing Unity\n'; exit 3; fi
started_ns="$(python3 -c 'import time; print(time.time_ns())')"
status=0
"$UNITY" -batchmode -nographics -projectPath "$ROOT" -runTests -testPlatform EditMode \
 -assemblyNames SquashClash.EditMode.Experiments -testFilter "$filter" \
 -testResults "$evidence/results.xml" -logFile "$evidence/unity.log" > "$evidence/console.log" 2>&1 || status=$?
python3 - "$evidence" "$started_ns" "$status" <<'PY'
import sys,re
from pathlib import Path
from xml.etree import ElementTree as E
p=Path(sys.argv[1]); xml=p/'results.xml'; log=p/'unity.log'
assert xml.is_file() and xml.stat().st_mtime_ns>int(sys.argv[2]),'No fresh XML'
r=E.parse(xml).getroot().attrib
print('RESULT:',r)
assert not re.search(r'error CS\d+|Scripts have compiler errors',log.read_text(errors='replace')),'Compiler errors'
assert int(sys.argv[3])==0 and r.get('result')=='Passed' and int(r.get('total',0))==int(r.get('passed',0))==1 and int(r.get('skipped',0))==0,'STOP: preserve failure evidence'
PY
