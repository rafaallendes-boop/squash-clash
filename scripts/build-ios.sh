#!/usr/bin/env bash
set -euo pipefail
if [[ $# != 1 ]]; then
    printf 'Uso: scripts/build-ios.sh dev|prod|debug\n' >&2
    exit 1
fi
case "$1" in
    dev|prod) configuration=Release ;;
    debug) configuration=Debug ;;
    *) printf 'Perfil inválido: %s\n' "$1" >&2; exit 1 ;;
esac
profile="$1"
source "$(dirname "$0")/unity-env.sh"
: "${DEVICE_UDID:?Falta DEVICE_UDID}" "${TEAM_ID:?Falta TEAM_ID}" "${BUNDLE_ID:?Falta BUNDLE_ID}"
mkdir -p "$ROOT/Logs"
export BUILD_PROFILE="$profile"
unity_log="$ROOT/Logs/build-$profile.log"
xcode_log="$ROOT/Logs/xcode-$profile.log"
printf 'Unity: %s\n' "$unity_log"
if ! "$UNITY" -batchmode -quit -buildTarget iOS -projectPath "$ROOT" \
    -executeMethod SquashClash.EditorTools.BuildScript.BuildIos -logFile "$unity_log" \
    > "$ROOT/Logs/build-$profile-console.log" 2>&1; then
    tail -n 45 "$unity_log" >&2
    printf 'Falló la exportación de Unity. Log: %s\n' "$unity_log" >&2
    exit 1
fi
printf 'Xcode: %s\n' "$xcode_log"
if ! xcodebuild -project "$ROOT/Builds/iOS-$profile/Unity-iPhone.xcodeproj" \
    -scheme Unity-iPhone -configuration "$configuration" -destination "id=$DEVICE_UDID" \
    -derivedDataPath "$ROOT/Builds/DerivedData-$profile" -allowProvisioningUpdates \
    "DEVELOPMENT_TEAM=$TEAM_ID" CODE_SIGN_STYLE=Automatic build > "$xcode_log" 2>&1; then
    tail -n 60 "$xcode_log" >&2
    printf 'Falló xcodebuild. Log: %s\n' "$xcode_log" >&2
    exit 1
fi
app="$ROOT/Builds/DerivedData-$profile/Build/Products/$configuration-iphoneos/SquashClash.app"
python3 - "$app" "$BUNDLE_ID" <<'PY'
import sys,plistlib
from pathlib import Path
app=Path(sys.argv[1])
with (app/'Info.plist').open('rb') as f:
    info=plistlib.load(f)
if info.get('CFBundleIdentifier') != sys.argv[2]:
    raise SystemExit('El bundle ID compilado no coincide con BUNDLE_ID.')
print('** BUILD SUCCEEDED **')
print('APP_PATH='+str(app))
PY
