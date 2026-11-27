#!/usr/bin/env bash
# Experimento explícito «defensa alta y larga» (26 nov), aislado de las suites de regresión.
#   scripts/measure-deep-defense.sh <run> [metrics|n2|both]
# metrics: DeepDefenseExperiment.WriteDeepDefense (Logs/experiments/deep-defense/<run>).
# n2: RivalStylesCalibrationExperiment.ConfirmPerStyle sin cambios (Logs/experiments/rival-styles-calibration/<run>-n2).
# Toma el candado global de Unity; un solo Unity. No sobrescribe evidencia ni reintenta.
set -euo pipefail
run="${1:?Uso: scripts/measure-deep-defense.sh <run> [metrics|n2|both]}"
what="${2:-both}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
if [[ -f "$ROOT/scripts/device.env" ]]; then source "$(dirname "$0")/unity-env.sh"
else
    # Sin device.env: el editor de ProjectVersion.txt (no hace falta el iPhone).
    v="$(sed -n 's/^m_EditorVersion: //p' "$ROOT/ProjectSettings/ProjectVersion.txt")"
    UNITY="/Applications/Unity/Hub/Editor/$v/Unity.app/Contents/MacOS/Unity"; [[ -x "$UNITY" ]] || { printf 'STOP: no existe %s\n' "$UNITY" >&2; exit 2; }
fi
cd "$ROOT"
[[ -z "$(git status --porcelain)" ]] || { printf 'STOP: árbol sucio\n' >&2; exit 2; }
head="$(git rev-parse HEAD)"
until mkdir /Users/rafaallendes/sc-unity.lock 2>/dev/null; do sleep 20; done
trap 'rmdir /Users/rafaallendes/sc-unity.lock' EXIT
one() {
    local name="$1" filter="$2" evidence="$3"
    [[ ! -e "$evidence" ]] || { printf 'STOP: ya existe evidencia: %s\n' "$evidence" >&2; exit 2; }
    mkdir -p "$evidence"; printf '%s\n' "$head" > "$evidence/head.txt"
    if pgrep -f '[U]nity.app/Contents/MacOS/Unity' >/dev/null; then printf 'STOP: ya hay un Editor Unity.\n' >&2; exit 3; fi
    local started_ns status=0
    started_ns="$(python3 -c 'import time; print(time.time_ns())')"
    "$UNITY" -batchmode -nographics -projectPath "$ROOT" -runTests -testPlatform EditMode \
        -assemblyNames SquashClash.EditMode.Experiments -testFilter "$filter" \
        -testResults "$evidence/results.xml" -logFile "$evidence/unity.log" > "$evidence/console.log" 2>&1 || status=$?
    python3 - "$evidence" "$started_ns" "$status" "$name" <<'PY'
import re, sys
from pathlib import Path
from xml.etree import ElementTree as ET
root = Path(sys.argv[1]); xml, log = root/'results.xml', root/'unity.log'
if not xml.exists() or xml.stat().st_mtime_ns <= int(sys.argv[2]):
    sys.exit('STOP: falta XML fresco; conservar evidencia, sin reintento.')
r = ET.parse(xml).getroot().attrib
print(sys.argv[4], 'RESULT:', {k: r.get(k) for k in ('result', 'total', 'passed', 'failed', 'skipped')})
if (int(sys.argv[3]) != 0 or r.get('result') != 'Passed' or int(r.get('total', 0)) != 1
        or int(r.get('passed', 0)) != 1 or not log.exists()
        or re.search(r'error CS\d+|Scripts have compiler errors', log.read_text(errors='replace'))):
    sys.exit('STOP: experimento incompleto/fallido; conservar evidencia, sin reintento.')
PY
}
if [[ "$what" == metrics || "$what" == both ]]; then
    export SC_DD_RUN="$run"; one metrics SquashClash.Experiments.EditMode.DeepDefenseExperiment.WriteDeepDefense$ \
        "$ROOT/Logs/experiments/deep-defense/$run-runner"
fi
if [[ "$what" == n2 || "$what" == both ]]; then
    export SC_RS_RUN="$run-n2"; one n2 SquashClash.Experiments.EditMode.RivalStylesCalibrationExperiment.ConfirmPerStyle$ \
        "$ROOT/Logs/experiments/rival-styles-calibration/$run-n2-runner"
fi
