#!/usr/bin/env python3
"""PostToolUse hook: warn when a tracked file's line endings no longer match HEAD.

The repo mixes CRLF and LF files and has no .gitattributes, so a tool or script that
rewrites a file can silently switch CRLF <-> LF and turn a small change into a whole-file diff.
Edit/Write: checks the edited file. Bash: checks every tracked file modified vs HEAD.
Never blocks; prints a JSON warning for Claude and the user.
"""

import json
import subprocess
import sys
from pathlib import Path


def git(root, *args):
    return subprocess.run(["git", "-C", str(root), *args], capture_output=True, check=False).stdout


def counts(data):
    crlf = data.count(b"\r\n")
    return crlf, data.count(b"\n") - crlf


def style(crlf, lf):
    if crlf and lf:
        return "mixed"
    return "CRLF" if crlf else "LF" if lf else None


def check(root, rel):
    head = git(root, "show", f"HEAD:{rel}")
    path = root / rel
    if not head or not path.is_file() or b"\0" in head[:8000]:
        return None
    c0, l0 = counts(head)
    c1, l1 = counts(path.read_bytes())
    before = style(c0, l0)
    if before == "CRLF" and l1:
        return f"{rel}: was CRLF in HEAD, now has {l1} LF-only line(s)"
    if before == "LF" and c1:
        return f"{rel}: was LF in HEAD, now has {c1} CRLF line(s)"
    return None


def main():
    try:
        event = json.load(sys.stdin)
    except ValueError:
        return 0
    root = Path(git(Path.cwd(), "rev-parse", "--show-toplevel").decode().strip() or ".")
    tool = event.get("tool_name", "")
    tool_input = event.get("tool_input") or {}

    if tool == "Bash":
        files = git(root, "diff", "--name-only", "HEAD").decode().splitlines()
    else:
        fp = tool_input.get("file_path") or tool_input.get("notebook_path")
        if not fp:
            return 0
        try:
            rel = Path(fp).resolve().relative_to(root.resolve()).as_posix()
        except ValueError:
            return 0
        files = [rel]

    problems = [p for p in (check(root, f) for f in files) if p]
    if not problems:
        return 0
    text = "Line endings changed: " + "; ".join(problems) + (
        ". Restore the original line endings (e.g. convert with python open(..., newline='') "
        "or unix2dos/dos2unix) unless the change was intended.")
    print(json.dumps({
        "systemMessage": text,
        "hookSpecificOutput": {"hookEventName": "PostToolUse", "additionalContext": text},
    }))
    return 0


if __name__ == "__main__":
    sys.exit(main())
