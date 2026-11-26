#!/usr/bin/env python3
"""Cross-check the MT5 command protocol between the C# client, the Python client and the MQL expert.

Checks (no terminal or MetaEditor needed):
  1. Command ids: Mt5CommandType.cs vs PyMtApi5/mt5commandtype.py vs ADD_EXECUTOR(...) in mq5/MtApi5.mq5.
     Only commands a client actually sends can fail the run; declared-but-unused ids are listed.
  2. Payload keys: keys a client method puts into its command parameters vs the top-level keys the
     matching Execute_<Name> handler reads from `jo`. Heuristic (regex based): treat key findings as
     hints to check by hand, e.g. keys read from a nested object or built by a helper are not seen.

Usage (from the repo root):
  python3 .claude/skills/verify/check_protocol.py            # errors + warnings
  python3 .claude/skills/verify/check_protocol.py --missing-py  # also list commands MtApi5Client sends but PyMtApi5 does not
Exit code 1 if an id error is found (key findings never fail the run).
"""

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
CS_ENUM = ROOT / "MtApi5/MtProtocol/Mt5CommandType.cs"
CS_CLIENT = ROOT / "MtApi5/MtApi5Client.cs"
PY_ENUM = ROOT / "PyMtApi5/mt5commandtype.py"
PY_CLIENT = ROOT / "PyMtApi5/mt5apiclient.py"
MQ5 = ROOT / "mq5/MtApi5.mq5"


def read(p):
    return p.read_text(encoding="utf-8-sig", errors="replace")


def strip_line_comments(text, marker):
    return "\n".join(line.split(marker, 1)[0] for line in text.splitlines())


def parse_enum(path, comment):
    entries = re.findall(r"^\s*(\w+)\s*=\s*(\d+)", strip_line_comments(read(path), comment), re.M)
    result, dups = {}, []
    for name, num in entries:
        if name in result:
            dups.append(name)
        result[name] = int(num)
    return result, dups


def parse_executors(text):
    return {name: int(num) for num, name in re.findall(r"^\s*ADD_EXECUTOR\(\s*(\d+)\s*,\s*(\w+)\s*\)", text, re.M)}


def mq5_handler_keys(text):
    """Execute_<Name> -> set of top-level payload keys read from `jo`."""
    keys = {}
    for m in re.finditer(r"^string\s+Execute_(\w+)\s*\(\s*\)\s*\{(.*?)^\}", text, re.M | re.S):
        body = m.group(2)
        found = set(re.findall(r"\(\s*jo\s*,\s*\"(\w+)\"", body))
        found |= set(re.findall(r"\bjo(?:\.p)?\.\w+\(\s*\"(\w+)\"", body))
        keys[m.group(1)] = found
    return keys


def client_keys(text, method_start, key_patterns):
    """Command name -> set of payload keys found in the method that sends it."""
    starts = [m.start() for m in re.finditer(method_start, text, re.M)]
    result = {}
    for m in re.finditer(r"Mt5CommandType\.(\w+)", text):
        begin = max((s for s in starts if s < m.start()), default=0)
        chunk = text[begin:m.start()]
        found = set()
        for pat in key_patterns:
            found |= set(re.findall(pat, chunk))
        result.setdefault(m.group(1), set()).update(found)
    return result


def main():
    show_missing_py = "--missing-py" in sys.argv
    errors, warnings = [], []

    cs, cs_dups = parse_enum(CS_ENUM, "//")
    py, py_dups = parse_enum(PY_ENUM, "#")
    mq5_text = read(MQ5)
    ex = parse_executors(mq5_text)

    for name in cs_dups:
        errors.append(f"C#: {name} declared twice in Mt5CommandType.cs")
    for name in py_dups:
        errors.append(f"Python: {name} declared twice in mt5commandtype.py")
    cs_text, py_text = read(CS_CLIENT), read(PY_CLIENT)
    cs_used = set(re.findall(r"Mt5CommandType\.(\w+)", cs_text))
    py_used = set(re.findall(r"Mt5CommandType\.(\w+)", py_text))
    used = cs_used | py_used
    unused = sorted(set(cs) - used, key=lambda n: cs[n])

    for label, table in (("C#", cs), ("Python", py), ("mq5", ex)):
        by_id = {}
        for name, num in table.items():
            by_id.setdefault(num, []).append(name)
        for num, names in by_id.items():
            if len(names) > 1:
                live = [n for n in names if n in used or label == "mq5"]
                msg = f"{label}: id {num} shared by {', '.join(names)}"
                (errors if len(live) > 1 else warnings).append(msg + ("" if len(live) > 1 else " (only one is sent)"))

    ex_by_id = {v: k for k, v in ex.items()}
    for name in sorted(used, key=lambda n: cs.get(n, -1)):
        num = cs.get(name)
        if num is None:
            errors.append(f"Mt5CommandType.{name} is sent but not declared in Mt5CommandType.cs")
        elif num not in ex_by_id:
            errors.append(f"{name} = {num} is sent but has no ADD_EXECUTOR({num}, ...) in MtApi5.mq5")
        elif ex_by_id[num].replace("_", "").lower() != name.replace("_", "").lower() and cs.get(ex_by_id[num]) != num:
            warnings.append(f"id {num}: C# name {name}, mq5 executor {ex_by_id[num]}")
    for name, num in py.items():
        if name not in cs:
            errors.append(f"Python {name} = {num} is not in Mt5CommandType.cs")
        elif cs[name] != num:
            errors.append(f"Python {name} = {num}, C# has {cs[name]}")
    for name, num in ex.items():
        if num not in cs.values():
            warnings.append(f"mq5 ADD_EXECUTOR({num}, {name}) has no C# command (dead or not wired up)")

    handlers = mq5_handler_keys(mq5_text)
    cs_keys = client_keys(cs_text, r"^ {8}(?:public|private|internal)\s",
                          [r"\{\s*\"(\w+)\"\s*,", r"\[\s*\"(\w+)\"\s*\]\s*="])
    py_keys = client_keys(py_text, r"^ {4}def\s", [r"\"(\w+)\"\s*:", r"\[\s*\"(\w+)\"\s*\]\s*="])
    for label, table in (("C#", cs_keys), ("Python", py_keys)):
        for cmd, keys in sorted(table.items()):
            num = cs.get(cmd)
            handler = ex_by_id.get(num) if num is not None else None
            if handler is None or handler not in handlers:
                continue
            read_keys = handlers[handler]
            sent_only = keys - read_keys
            read_only = read_keys - keys
            if sent_only and read_keys:
                warnings.append(f"{label} {cmd}: sends {sorted(sent_only)} not read by Execute_{handler} (reads {sorted(read_keys)})")
            if read_only and keys:
                warnings.append(f"{label} {cmd}: Execute_{handler} reads {sorted(read_only)} not sent (sends {sorted(keys)})")

    print(f"Commands: C# {len(cs)}, Python {len(py)}, mq5 executors {len(ex)}")
    for e in errors:
        print(f"ERROR   {e}")
    for w in warnings:
        print(f"WARNING {w}")
    print(f"Declared but sent by no client ({len(unused)}): {', '.join(unused)}")
    if show_missing_py:
        missing = sorted(cs_used - py_used, key=lambda n: cs.get(n, -1))
        print(f"\nCommands sent by MtApi5Client but not by Mt5ApiClient ({len(missing)}):")
        for name in missing:
            print(f"  {cs.get(name, -1):4d}  {name}")
    if not errors and not warnings:
        print("OK")
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
