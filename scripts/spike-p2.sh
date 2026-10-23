#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/unity-env.sh"
mode="${1:-editmode}"
case "$mode" in editmode) platform=EditMode; assembly=SquashClash.EditMode.Experiments; filter=SquashClash.Experiments.EditMode.AnalyticEditModeSpikeExperiment;; playmode) platform=PlayMode; assembly=SquashClash.Experiments; filter=SquashClash.Experiments.AnalyticSpikeExperiment;; *) printf 'Uso: scripts/spike-p2.sh editmode|playmode\n' >&2; exit 1;; esac
mkdir -p "$ROOT/Logs"
export SQUASH_SPIKE_COMMIT="$(git -C "$ROOT" rev-parse --short HEAD)"
started_ns="$(python3 -c 'import time; print(time.time_ns())')"
xml="$ROOT/Logs/spike-p2-$mode-results.xml"
report="$ROOT/Logs/physics-spike-P2-$mode.md"
log="$ROOT/Logs/spike-p2-$mode.log"
args=(-batchmode -projectPath "$ROOT" -runTests -testPlatform "$platform" -assemblyNames "$assembly" -testFilter "$filter" -testResults "$xml" -logFile "$log")
if [[ "$mode" == editmode ]]; then args+=(-nographics); fi
unity_rc=0
"$UNITY" "${args[@]}" > "$ROOT/Logs/spike-p2-$mode-console.log" 2>&1 || unity_rc=$?
python3 - "$xml" "$report" "$log" "$started_ns" "$unity_rc" <<'PY'
import re,sys
from pathlib import Path
from xml.etree import ElementTree as ET
xml,report,log=map(Path,sys.argv[1:4]);started=int(sys.argv[4])
for p in [xml,report,log]:
 if not p.is_file() or p.stat().st_mtime_ns<=started:raise SystemExit('Falta artefacto nuevo: '+str(p))
d=ET.parse(xml).getroot().attrib
print('RESULT: '+' '.join(k+'='+d.get(k,'0') for k in ['total','passed','failed','skipped']))
if int(sys.argv[5]) or int(d.get('total',0))<1 or int(d.get('failed',0)) or re.search(r'error CS\d+|Scripts have compiler errors',log.read_text(errors='replace')):raise SystemExit('Falló el experimento; revisar XML/log.')
s=report.read_text()
if 'Estado: **COMPLETO**' not in s or 'Backend: P2-Analytic' not in s or len(re.findall(r'^- C17 muestra ',s,re.M))!=200:raise SystemExit('Informe incompleto o sin 200 muestras C17.')
print('REPORT: '+str(report));print('Experimento completo; los veredictos físicos son datos, no selección de backend.')
PY
