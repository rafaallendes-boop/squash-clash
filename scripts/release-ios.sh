#!/usr/bin/env bash
set -euo pipefail
mode="${1:---plan}"
if [[ $# -gt 1 || ( "$mode" != --plan && "$mode" != --export && "$mode" != --upload ) ]]; then
    printf 'Uso: scripts/release-ios.sh [--plan|--export|--upload]\n' >&2
    exit 1
fi
# The default plan does not source environment files or run Unity/Xcode.
if [[ "$mode" == --plan ]]; then
    cat <<'PLAN'
PLAN ONLY — no export, signing, archive, credentials or upload executed.
1. Configure the paid Developer Program team and App Store Connect app/bundle.
2. Provide RELEASE_TEAM_ID; for upload also ASC_KEY_PATH, ASC_KEY_ID, ASC_ISSUER_ID.
   Use the environment or ignored scripts/release.env. Never commit the .p8 key.
3. Export Unity prod (Release, no Development, no SQUASH_DEV).
4. xcodebuild archive: Unity-iPhone, Release, generic/platform=iOS,
   automatic signing, DEVELOPMENT_TEAM=$RELEASE_TEAM_ID, -allowProvisioningUpdates.
5. xcodebuild -exportArchive: method=app-store-connect, signingStyle=automatic,
   manageAppVersionAndBuildNumber=false; destination=export or upload.
   Upload authenticates with -authenticationKeyPath/-authenticationKeyID/-authenticationKeyIssuerID.
Artifacts: Builds/Release/SquashClash.xcarchive and Builds/Release/export/.
CFBundleVersion = git rev-list --count HEAD. Same HEAD retains the same number.
Stop before --export/--upload until the PO authorizes native/credential use.
PLAN
    exit 0
fi
source "$(dirname "$0")/unity-env.sh"
if [[ -f "$ROOT/scripts/release.env" ]]; then
    set -a
    source "$ROOT/scripts/release.env"
    set +a
fi
: "${RELEASE_TEAM_ID:?Falta RELEASE_TEAM_ID del Developer Program pagado}"
: "${BUNDLE_ID:?Falta BUNDLE_ID}"
auth=()
if [[ "$mode" == --upload ]]; then
    : "${ASC_KEY_PATH:?Falta ASC_KEY_PATH}" "${ASC_KEY_ID:?Falta ASC_KEY_ID}" "${ASC_ISSUER_ID:?Falta ASC_ISSUER_ID}"
    [[ -f "$ASC_KEY_PATH" ]] || { printf 'No existe el archivo de clave local.\n' >&2; exit 1; }
    auth=(-authenticationKeyPath "$ASC_KEY_PATH" -authenticationKeyID "$ASC_KEY_ID" -authenticationKeyIssuerID "$ASC_ISSUER_ID")
fi
# Fail before starting another editor. Do not terminate another session's processes.
if pgrep -f '/Unity[.]app/Contents/|/Unity Hub[.]app/Contents/' >/dev/null; then
    printf 'STOP: otro proceso Unity activo; no se inicia la exportación.\n' >&2
    exit 1
fi
mkdir -p "$ROOT/Logs" "$ROOT/Builds/Release"
export BUILD_PROFILE=prod
if ! "$UNITY" -batchmode -quit -buildTarget iOS -projectPath "$ROOT" \
    -executeMethod SquashClash.EditorTools.BuildScript.BuildIos \
    -logFile "$ROOT/Logs/release-ios-unity.log" > "$ROOT/Logs/release-ios-unity-console.log" 2>&1; then
    printf 'Falló Unity; consultar Logs/release-ios-unity.log.\n' >&2
    exit 1
fi
archive="$ROOT/Builds/Release/SquashClash.xcarchive"
if ! xcodebuild -project "$ROOT/Builds/iOS-prod/Unity-iPhone.xcodeproj" \
    -scheme Unity-iPhone -configuration Release -destination 'generic/platform=iOS' \
    -derivedDataPath "$ROOT/Builds/Release/DerivedData" -archivePath "$archive" \
    -allowProvisioningUpdates ${auth[@]+"${auth[@]}"} "DEVELOPMENT_TEAM=$RELEASE_TEAM_ID" CODE_SIGN_STYLE=Automatic archive \
    > "$ROOT/Logs/release-ios-archive.log" 2>&1; then
    printf 'Falló archive; consultar Logs/release-ios-archive.log.\n' >&2
    exit 1
fi
options="$ROOT/Builds/Release/ExportOptions.plist"
python3 - "$options" "$RELEASE_TEAM_ID" "$mode" <<'PY'
import plistlib,sys
with open(sys.argv[1],'wb') as f:
    plistlib.dump({'method':'app-store-connect','teamID':sys.argv[2],
                  'signingStyle':'automatic','destination':'upload' if sys.argv[3]=='--upload' else 'export',
                  'manageAppVersionAndBuildNumber':False},f)
PY
if ! xcodebuild -exportArchive -archivePath "$archive" -exportOptionsPlist "$options" \
    -exportPath "$ROOT/Builds/Release/export" -allowProvisioningUpdates ${auth[@]+"${auth[@]}"} \
    > "$ROOT/Logs/release-ios-export.log" 2>&1; then
    printf 'Falló exportación/subida; consultar Logs/release-ios-export.log.\n' >&2
    exit 1
fi
printf 'RELEASE-IOS: %s completado. Evidencia local en Logs/release-ios-*.log.\n' "$mode"
