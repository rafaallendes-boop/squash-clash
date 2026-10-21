#!/usr/bin/env bash
set -euo pipefail
if [[ $# -lt 1 || $# -gt 2 ]]; then
    printf 'Uso: scripts/install-ios.sh dev|prod|debug [--console]\n' >&2
    exit 1
fi
case "$1" in
    dev|prod) configuration=Release ;;
    debug) configuration=Debug ;;
    *) printf 'Perfil inválido: %s\n' "$1" >&2; exit 1 ;;
esac
if [[ $# == 2 && "$2" != --console ]]; then
    printf 'Opción inválida: %s\n' "$2" >&2
    exit 1
fi
profile="$1"
source "$(dirname "$0")/unity-env.sh"
: "${DEVICE_ID:?Falta DEVICE_ID}" "${BUNDLE_ID:?Falta BUNDLE_ID}"
app="$ROOT/Builds/DerivedData-$profile/Build/Products/$configuration-iphoneos/SquashClash.app"
if [[ ! -d "$app" ]]; then
    printf 'No existe la app; ejecutar primero build-ios.sh %s.\n' "$profile" >&2
    exit 1
fi
mkdir -p "$ROOT/Logs"
xcrun devicectl device install app --device "$DEVICE_ID" "$app" \
    --json-output "$ROOT/Logs/install-$profile.json"
if [[ $# == 2 ]]; then
    xcrun devicectl device process launch --device "$DEVICE_ID" --terminate-existing --console "$BUNDLE_ID"
else
    xcrun devicectl device process launch --device "$DEVICE_ID" --terminate-existing "$BUNDLE_ID" \
        --json-output "$ROOT/Logs/launch-$profile.json"
fi
