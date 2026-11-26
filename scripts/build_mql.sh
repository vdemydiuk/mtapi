#!/usr/bin/env bash
# Compile the MtApi experts (mq5/MtApi5.mq5, mq4/MtApi.mq4) on Linux with
# MetaEditor running under Wine.
#
# Usage: scripts/build_mql.sh [--check] [mq5] [mq4]
#   --check   compile only, do not copy the .ex5/.ex4 into the repo
#   mq5/mq4   which experts to build (default: every one whose MetaEditor is found)
#
# Environment (defaults in brackets):
#   MT5_WINEPREFIX [~/.mt5]   MT5_DIR [$MT5_WINEPREFIX/drive_c/Program Files/MetaTrader 5]
#   MT4_WINEPREFIX [~/.mt4]   MT4_DIR [first "MetaTrader 4*" folder in Program Files (x86)]
#
# The sources are compiled in a temporary copy of the terminal's MQL5/MQL4
# folder, so angle-bracket includes (<json.mqh>, <Trade\...>) resolve against
# the standard library plus the repo's .mqh files without touching the install.
# Exit code: 0 when every requested expert compiled with 0 errors, 1 otherwise.

set -euo pipefail

REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CHECK_ONLY=0
TARGETS=()
for arg in "$@"; do
    case "$arg" in
        --check) CHECK_ONLY=1 ;;
        mq5|mq4) TARGETS+=("$arg") ;;
        -h|--help) sed -n '2,17p' "$0"; exit 0 ;;
        *) echo "Unknown argument: $arg" >&2; exit 2 ;;
    esac
done

command -v wine >/dev/null || { echo "wine is not installed" >&2; exit 2; }

MT5_WINEPREFIX="${MT5_WINEPREFIX:-$HOME/.mt5}"
MT5_DIR="${MT5_DIR:-$MT5_WINEPREFIX/drive_c/Program Files/MetaTrader 5}"
MT4_WINEPREFIX="${MT4_WINEPREFIX:-$HOME/.mt4}"
if [[ -z "${MT4_DIR:-}" ]]; then
    MT4_DIR="$(find "$MT4_WINEPREFIX/drive_c/Program Files (x86)" -maxdepth 1 -type d -name 'MetaTrader 4*' 2>/dev/null | head -n1 || true)"
fi

# MetaEditor may need a display even when compiling; use a virtual one if there is none.
RUN_PREFIX=()
if [[ -z "${DISPLAY:-}" && -z "${WAYLAND_DISPLAY:-}" ]] && command -v xvfb-run >/dev/null; then
    RUN_PREFIX=(xvfb-run -a)
fi

WORK_DIR="$(mktemp -d)"
trap 'rm -rf "$WORK_DIR"' EXIT

# compile <name> <prefix> <terminal dir> <metaeditor exe> <mql folder> <src dir> <source> <binary ext>
compile() {
    local name="$1" prefix="$2" term_dir="$3" editor="$4" mql="$5" src_dir="$6" src="$7" ext="$8"

    if [[ ! -f "$term_dir/$editor" ]]; then
        echo "[$name] MetaEditor not found: $term_dir/$editor" >&2
        return 1
    fi

    local stage="$WORK_DIR/$name/$mql"
    mkdir -p "$stage/Experts"
    cp -r "$term_dir/$mql/Include" "$stage/Include"
    cp "$REPO_DIR/$src_dir"/*.mqh "$stage/Include/"
    cp "$REPO_DIR/$src_dir/$src" "$stage/Experts/"

    local src_w include_w log_w log="$stage/build.log"
    src_w="$(WINEPREFIX="$prefix" winepath -w "$stage/Experts/$src" 2>/dev/null)"
    include_w="$(WINEPREFIX="$prefix" winepath -w "$stage" 2>/dev/null)"
    log_w="$(WINEPREFIX="$prefix" winepath -w "$log" 2>/dev/null)"

    echo "[$name] compiling $src_dir/$src"
    # MetaEditor's exit code is not reliable; the log decides.
    WINEPREFIX="$prefix" WINEDEBUG=-all "${RUN_PREFIX[@]}" \
        wine "$term_dir/$editor" "/compile:$src_w" "/include:$include_w" "/log:$log_w" >/dev/null 2>&1 || true

    if [[ ! -f "$log" ]]; then
        echo "[$name] no build log produced" >&2
        return 1
    fi

    # The log is UTF-16LE with CRLF line endings.
    local text
    text="$(iconv -f UTF-16LE -t UTF-8 "$log" 2>/dev/null | tr -d '\r' | sed 's/^\xEF\xBB\xBF//')"
    # Report locations against the repo, not the staging copy.
    text="${text//"$include_w\\Experts\\"/"$src_dir/"}"
    echo "$text" | grep -iE 'error|warning' | grep -viE '^\s*$' | sed "s/^/[$name]   /" || true

    local errors
    errors="$(echo "$text" | grep -ioE '([0-9]+) errors?' | tail -n1 | grep -oE '[0-9]+' || true)"
    local binary="$stage/Experts/${src%.*}.$ext"
    if [[ "$errors" != "0" || ! -f "$binary" ]]; then
        echo "[$name] FAILED"
        return 1
    fi

    if (( CHECK_ONLY )); then
        echo "[$name] OK (check only, $src_dir/${src%.*}.$ext not updated)"
    else
        cp "$binary" "$REPO_DIR/$src_dir/"
        echo "[$name] OK -> $src_dir/${src%.*}.$ext"
    fi
}

if (( ${#TARGETS[@]} == 0 )); then
    [[ -f "$MT5_DIR/MetaEditor64.exe" ]] && TARGETS+=(mq5)
    [[ -n "$MT4_DIR" && -f "$MT4_DIR/metaeditor.exe" ]] && TARGETS+=(mq4)
    if (( ${#TARGETS[@]} == 0 )); then
        echo "No MetaEditor found (looked in '$MT5_DIR' and '$MT4_WINEPREFIX')" >&2
        exit 2
    fi
fi

status=0
for t in "${TARGETS[@]}"; do
    case "$t" in
        mq5) compile mq5 "$MT5_WINEPREFIX" "$MT5_DIR" MetaEditor64.exe MQL5 mq5 MtApi5.mq5 ex5 || status=1 ;;
        mq4) compile mq4 "$MT4_WINEPREFIX" "${MT4_DIR:-}" metaeditor.exe MQL4 mq4 MtApi.mq4 ex4 || status=1 ;;
    esac
done
exit $status
