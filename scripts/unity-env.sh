#!/usr/bin/env bash
# Resolver compartido; no crea ni abre un proyecto.
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]:-$0}")/.." && pwd)"
if [[ ! -f "$ROOT/scripts/device.env" ]]; then
    printf 'Falta scripts/device.env; copiar y completar device.env.example.\n' >&2
    return 1
fi
set -a
source "$ROOT/scripts/device.env"
set +a
if [[ -z "${UNITY_VERSION:-}" ]]; then
    printf 'Falta UNITY_VERSION en scripts/device.env.\n' >&2
    return 1
fi
UNITY="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity"
if [[ ! -x "$UNITY" ]]; then
    printf 'No existe el editor ejecutable: %s\n' "$UNITY" >&2
    return 1
fi
if [[ -f "$ROOT/ProjectSettings/ProjectVersion.txt" ]]; then
    project_version="$(awk '$1 == "m_EditorVersion:" { print $2; exit }' "$ROOT/ProjectSettings/ProjectVersion.txt")"
    if [[ "$project_version" != "$UNITY_VERSION" ]]; then
        printf 'Versión del proyecto %s distinta de UNITY_VERSION=%s.\n' "$project_version" "$UNITY_VERSION" >&2
        return 1
    fi
fi
export ROOT UNITY
