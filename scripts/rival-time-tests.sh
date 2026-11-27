#!/usr/bin/env bash
# EditMode filtered run without device.env (editor from ProjectVersion.txt); takes the shared Unity lock.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"; out="$1"; filter="$2"
v="$(sed -n 's/^m_EditorVersion: //p' "$ROOT/ProjectSettings/ProjectVersion.txt")"
UNITY="/Applications/Unity/Hub/Editor/$v/Unity.app/Contents/MacOS/Unity"
mkdir -p "$out"; git -C "$ROOT" rev-parse HEAD > "$out/head.txt"; git -C "$ROOT" status --porcelain > "$out/status.txt"
until mkdir /Users/rafaallendes/sc-unity.lock 2>/dev/null; do sleep 20; done
trap 'rmdir /Users/rafaallendes/sc-unity.lock' EXIT
if pgrep -f '[U]nity.app/Contents/MacOS/Unity' >/dev/null; then echo 'STOP: ya hay un Editor Unity.'; exit 3; fi
start=$(python3 -c 'import time;print(time.time_ns())'); st=0
"$UNITY" -batchmode -nographics -projectPath "$ROOT" -runTests -testPlatform EditMode -assemblyNames SquashClash.Core.Tests \
  -testFilter "$filter" -testResults "$out/results.xml" -logFile "$out/unity.log" > "$out/console.log" 2>&1 || st=$?
python3 - "$out" "$start" "$st" <<'PY'
import sys,re
from pathlib import Path
from xml.etree import ElementTree as ET
o=Path(sys.argv[1]);x=o/'results.xml'
if not x.exists() or x.stat().st_mtime_ns<=int(sys.argv[2]):print('STOP: sin XML fresco; exit',sys.argv[3]);sys.exit(1)
r=ET.parse(x).getroot()
print('RESULT',{k:r.attrib.get(k) for k in('result','total','passed','failed')},'exit',sys.argv[3])
for t in r.iter('test-case'):
    if t.attrib.get('result')!='Passed':
        m=t.find('.//message');print(t.attrib['result'],t.attrib['fullname'],'|',(m.text or '').strip().splitlines()[0] if m is not None and m.text else '')
if re.search(r'error CS\d+',(o/'unity.log').read_text(errors='replace')):print('ERRORES CS')
PY
