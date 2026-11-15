#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
scripting="$root/scripting"
output="$root/compiled"

spcomp="${SPCOMP:-}"

if [ -z "$spcomp" ]; then
    for candidate in spcomp64 spcomp "$root/.sourcemod/addons/sourcemod/scripting/spcomp64" "$root/.sourcemod/addons/sourcemod/scripting/spcomp"; do
        if command -v "$candidate" > /dev/null 2>&1; then
            spcomp="$(command -v "$candidate")"
            break
        fi
    done
fi

if [ -z "$spcomp" ]; then
    echo "spcomp not found. Put it in the PATH or set SPCOMP to the SourceMod 1.12 compiler." >&2
    exit 1
fi

include="${SOURCEMOD_INCLUDE:-$(dirname "$spcomp")/include}"

if [ ! -d "$include" ]; then
    echo "SourceMod include folder not found: $include. Set SOURCEMOD_INCLUDE to it." >&2
    exit 1
fi

mkdir -p "$output"

failed=0
total=0

for source in "$scripting"/*.sp; do
    name="$(basename "$source" .sp)"
    total=$((total + 1))

    echo "Compiling $name"

    if ! "$spcomp" "$source" -i "$scripting/include" -i "$include" -o "$output/$name.smx"; then
        echo "FAILED: $name" >&2
        failed=$((failed + 1))
    fi
done

if [ "$failed" -gt 0 ]; then
    echo "$failed of $total plugins failed to compile." >&2
    exit 1
fi

echo "All $total plugins compiled into $output"
