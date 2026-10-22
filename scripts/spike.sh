#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/unity-env.sh"
variant="${1:-baseline}"
case "$variant" in baseline|speculative|offset05|solver12|no-ed) ;; *) printf 'Variante desconocida: %s\n' "$variant" >&2; exit 1;; esac
mkdir -p "$ROOT/Logs"
export SQUASH_SPIKE_VARIANT="$variant" SQUASH_SPIKE_COMMIT="$(git -C "$ROOT" rev-parse --short HEAD)"
backup="$(mktemp "$ROOT/Logs/spike-settings.XXXXXX")"
cp "$ROOT/ProjectSettings/DynamicsManager.asset" "$backup"
trap 'cp "$backup" "$ROOT/ProjectSettings/DynamicsManager.asset"; rm -f "$backup"' EXIT
"$UNITY" -batchmode -nographics -quit -projectPath "$ROOT" -executeMethod SquashClash.EditorTools.ProjectSetup.ConfigureSpikePhysics -logFile "$ROOT/Logs/spike-setup-$variant.log" > "$ROOT/Logs/spike-setup-$variant-console.log" 2>&1
if rg -q 'error CS[0-9]+|Scripts have compiler errors' "$ROOT/Logs/spike-setup-$variant.log"; then exit 1; fi
started_ns="$(python3 -c 'import time; print(time.time_ns())')"
result="$ROOT/Logs/spike-$variant-results.xml"
report="$ROOT/Logs/physics-spike-P1-$variant.md"
log="$ROOT/Logs/spike-$variant.log"
unity_rc=0
"$UNITY" -batchmode -projectPath "$ROOT" -runTests -testPlatform PlayMode -assemblyNames SquashClash.Experiments -testFilter SquashClash.Experiments.PhysXSpikeExperiment -testResults "$result" -logFile "$log" > "$ROOT/Logs/spike-$variant-console.log" 2>&1 || unity_rc=$?
python3 - "$result" "$report" "$log" "$started_ns" "$unity_rc" <<'PY'
import re,sys
from pathlib import Path
from xml.etree import ElementTree as ET
xml,report,log=map(Path,sys.argv[1:4]); start=int(sys.argv[4])
for p in (xml,report,log):
    if not p.is_file() or p.stat().st_mtime_ns <= start: raise SystemExit('Falta artefacto nuevo: '+str(p))
d=ET.parse(xml).getroot().attrib
print('RESULT: '+ ' '.join(k+'='+d.get(k,'0') for k in ('total','passed','failed','skipped')))
if int(sys.argv[5]) or int(d.get('total',0))<1 or int(d.get('failed',0)) or re.search(r'error CS\d+|Scripts have compiler errors',log.read_text(errors='replace')): raise SystemExit('La ejecución del experimento falló; revisar XML/log.')
if 'Estado: **COMPLETO**' not in report.read_text(): raise SystemExit('Informe incompleto.')
print('REPORT: '+str(report))
print('Ejecución completa; los veredictos físicos son datos, no autorización de I4c.')
PY
