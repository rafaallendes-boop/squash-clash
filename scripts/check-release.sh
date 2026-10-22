#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/unity-env.sh"
mkdir -p "$ROOT/Logs/evidence"
for profile in dev prod; do
    # These dedicated export directories must never contribute old generated code.
    export BUILD_PROFILE="$profile"
    started_ns="$(python3 -c 'import time; print(time.time_ns())')"
    log="$ROOT/Logs/release-$profile.log"
    if ! "$UNITY" -batchmode -quit -buildTarget iOS -projectPath "$ROOT" \
        -executeMethod SquashClash.EditorTools.BuildScript.BuildIos -logFile "$log" \
        > "$ROOT/Logs/release-$profile-console.log" 2>&1; then
        tail -n 35 "$log" >&2
        exit 1
    fi
    python3 - "$ROOT" "$profile" "$started_ns" <<'PY'
import json,re,sys,shutil
from pathlib import Path
root=Path(sys.argv[1]);profile=sys.argv[2];started=int(sys.argv[3])
output=root/f'Builds/iOS-{profile}/Il2CppOutputProject'
registration=output/'Source/il2cppOutput/Il2CppCodeRegistration.cpp'
metadata=root/'Assets/SquashClash/Runtime/Resources/BuildInfo.json'
log=(root/f'Logs/release-{profile}.log').read_text(errors='replace')
if re.search(r'error CS\d+|Scripts have compiler errors',log):raise SystemExit('Compiler errors in '+profile)
for p in [output,registration,metadata]:
    if not p.exists() or p.stat().st_mtime_ns <= started:raise SystemExit('STALE OR MISSING '+profile+': '+str(p))
modules=sorted(set(re.findall(r'g_SquashClash_\w+_CodeGenModule',registration.read_text())))
# A reflection string in Runtime does NOT prove that DevPanel is compiled.
expected=['g_SquashClash_Core_CodeGenModule','g_SquashClash_Runtime_CodeGenModule']
if profile=='dev':expected.append('g_SquashClash_DevPanel_CodeGenModule')
if modules!=sorted(expected):raise SystemExit('Unexpected product modules for '+profile+': '+repr(modules))
info=json.loads(metadata.read_text())
if info['profile']!=profile:raise SystemExit('Wrong build metadata profile.')
shutil.copyfile(metadata,root/f'Logs/evidence/buildinfo-{profile}.json')
(root/f'Logs/evidence/release-{profile}.json').write_text(json.dumps({'profile':profile,'metadata':info,'freshExport':True,'productIl2CppModules':modules},indent=2)+'\n')
print('RELEASE-CHECK: '+profile+' modules='+','.join(modules))
PY
done
printf 'RELEASE-CHECK: OK\n'
