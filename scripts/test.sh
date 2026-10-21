#!/usr/bin/env bash
set -euo pipefail
summary_printed=0
trap 'if [[ "$summary_printed" == 0 ]]; then printf "RESULT: total=0 passed=0 failed=0 skipped=0\n"; fi' EXIT
if [[ $# -lt 1 || $# -gt 2 ]]; then
    printf 'Uso: scripts/test.sh editmode|playmode [filtro]\n' >&2
    exit 1
fi
case "$1" in
    editmode) platform=EditMode; assembly=SquashClash.Core.Tests ;;
    playmode) platform=PlayMode; assembly=SquashClash.Runtime.Tests ;;
    *) printf 'Plataforma inválida: %s\n' "$1" >&2; exit 1 ;;
esac
source "$(dirname "$0")/unity-env.sh"
mkdir -p "$ROOT/Logs"
results="$ROOT/Logs/$1-results.xml"
log="$ROOT/Logs/$1.log"
started_ns="$(python3 -c 'import time; print(time.time_ns())')"
args=(-batchmode -projectPath "$ROOT" -runTests -testPlatform "$platform"
      -assemblyNames "$assembly" -testResults "$results" -logFile "$log")
if [[ "$platform" == EditMode ]]; then args+=(-nographics); fi
if [[ $# == 2 ]]; then args+=(-testFilter "$2"); fi
unity_status=0
"$UNITY" "${args[@]}" > "$ROOT/Logs/$1-console.log" 2>&1 || unity_status=$?
summary_printed=1
python3 - "$results" "$log" "$started_ns" "$unity_status" <<'PY'
import sys,re
from pathlib import Path
from xml.etree import ElementTree
result,log=map(Path,sys.argv[1:3])
total=passed=failed=skipped=0
reason=None
try:
    if not result.is_file():
        raise ValueError('Unity no generó el XML de resultados.')
    if result.stat().st_mtime_ns <= int(sys.argv[3]):
        raise ValueError('El XML no es posterior al inicio de esta ejecución.')
    data=ElementTree.parse(result).getroot().attrib
    total=int(data.get('total',0)); passed=int(data.get('passed',0))
    failed=int(data.get('failed',0)); skipped=int(data.get('skipped',0))
    if total < 1: raise ValueError('No se ejecutaron tests (total=0).')
    if failed: raise ValueError('Hay tests fallidos; consultar XML y log.')
    if int(sys.argv[4]): raise ValueError('Unity terminó con código '+sys.argv[4]+'.')
    if not log.is_file(): raise ValueError('Falta el log de Unity.')
    if re.search(r'error CS\d+|Scripts have compiler errors',log.read_text(errors='replace')):
        raise ValueError('El log contiene errores de compilación.')
except Exception as e:
    reason=str(e)
print(f'RESULT: total={total} passed={passed} failed={failed} skipped={skipped}')
print(f'XML: {result}\nLOG: {log}')
if reason:
    print('FAIL: '+reason,file=sys.stderr)
    sys.exit(1)
PY
