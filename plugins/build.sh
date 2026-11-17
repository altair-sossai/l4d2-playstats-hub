#!/usr/bin/env bash
set -euo pipefail

expected_compiler="1.12.0.7260"
build_date='__DATE__="reproducible"'
build_time='__TIME__="build"'

root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
scripting="$root/left4dead2/addons/sourcemod/scripting"
plugins="$root/left4dead2/addons/sourcemod/plugins"

usage() {
    echo "Usage: $0 [--check]" >&2
    echo "  (no option)  compiles every plugin into left4dead2/addons/sourcemod/plugins" >&2
    echo "  --check      compiles into a temporary folder and fails if a committed .smx differs" >&2
}

check=0
case "${1:-}" in
    "") ;;
    --check) check=1 ;;
    -h|--help) usage; exit 0 ;;
    *) usage; exit 2 ;;
esac

spcomp="${SPCOMP:-}"

if [ -z "$spcomp" ]; then
    for candidate in spcomp64 spcomp "$root/.sourcemod/addons/sourcemod/scripting/spcomp64" "$root/.sourcemod/addons/sourcemod/scripting/spcomp"; do
        if command -v "$candidate" > /dev/null 2>&1; then
            spcomp="$(command -v "$candidate")"
            break
        fi
    done
fi

if [ -z "$spcomp" ] || [ ! -x "$spcomp" ]; then
    echo "spcomp not found. Put it in the PATH, unpack SourceMod $expected_compiler into plugins/.sourcemod, or set SPCOMP." >&2
    exit 1
fi

compiler="$("$spcomp" 2>&1 | head -n 1 | awk '{ print $NF }' | tr -d '\r' || true)"

if [ "$compiler" != "$expected_compiler" ]; then
    echo "spcomp is version '$compiler', but the committed plugins are built with $expected_compiler." >&2

    if [ "$check" -eq 1 ]; then
        exit 1
    fi

    echo "The .smx files will differ from the committed ones." >&2
fi

include="${SOURCEMOD_INCLUDE:-$(dirname "$spcomp")/include}"

if [ ! -f "$include/sourcemod.inc" ]; then
    echo "SourceMod include folder not found: $include. Set SOURCEMOD_INCLUDE to it." >&2
    exit 1
fi

stage="$(mktemp -d)"
trap 'rm -rf "$stage"' EXIT

cp "$scripting"/*.sp "$stage/"
cp -R "$scripting/include" "$stage/include"
cp -R "$include" "$stage/sourcemod"
mkdir -p "$stage/out"

failed=0
total=0

cd "$stage"

for source in *.sp; do
    name="${source%.sp}"
    total=$((total + 1))

    echo "Compiling $name"

    if ! "$spcomp" "$source" -i include/ -i sourcemod/ --syntax-only -E -v0; then
        echo "FAILED: $name has errors or warnings" >&2
        failed=$((failed + 1))
        continue
    fi

    if ! "$spcomp" "$source" -i include/ -i sourcemod/ -o "out/$name.smx" -z0 -w201 -v0 "$build_date" "$build_time"; then
        echo "FAILED: $name" >&2
        failed=$((failed + 1))
    fi
done

if [ "$failed" -gt 0 ]; then
    echo "$failed of $total plugins failed to compile." >&2
    exit 1
fi

if [ "$check" -eq 1 ]; then
    different=0

    for compiled in out/*.smx; do
        name="$(basename "$compiled")"

        if [ ! -f "$plugins/$name" ]; then
            echo "MISSING: left4dead2/addons/sourcemod/plugins/$name is not committed." >&2
            different=$((different + 1))
        elif ! cmp -s "$compiled" "$plugins/$name"; then
            echo "OUTDATED: left4dead2/addons/sourcemod/plugins/$name is not the result of compiling the committed source." >&2
            different=$((different + 1))
        fi
    done

    for committed in "$plugins"/*.smx; do
        [ -e "$committed" ] || continue
        name="$(basename "$committed" .smx)"

        if [ ! -f "$scripting/$name.sp" ]; then
            echo "ORPHAN: left4dead2/addons/sourcemod/plugins/$name.smx has no source in scripting/." >&2
            different=$((different + 1))
        fi
    done

    if [ "$different" -gt 0 ]; then
        echo "Run plugins/build.sh (or build.ps1) with SourceMod $expected_compiler and commit the .smx files." >&2
        exit 1
    fi

    echo "All $total committed plugins match their sources."
    exit 0
fi

mkdir -p "$plugins"
cp out/*.smx "$plugins/"

echo "All $total plugins compiled into left4dead2/addons/sourcemod/plugins"
