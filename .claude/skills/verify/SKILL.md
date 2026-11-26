---
name: verify
description: Run every check for mtapi that works without Windows or a MetaTrader terminal - MtApi5 unit tests, PyMtApi5 unit tests, C# builds, and a cross-check of command ids and payload keys between MtApi5Client, Mt5ApiClient and the mq5 expert. Use after changing MtApi5, MtClient, PyMtApi5 or mq5/MtApi5.mq5, before committing, or when the user asks to verify, test or check the changes.
---

# Verify mtapi changes

Run from the repo root. Run the independent steps in parallel. Report each step
as passed, failed (with the relevant output) or skipped (and why). Never call
the change "verified" if a step was skipped. Say which step was skipped.

## 1. Protocol cross-check (always)

    python3 .claude/skills/verify/check_protocol.py

This compares `MtApi5/MtProtocol/Mt5CommandType.cs`, `PyMtApi5/mt5commandtype.py`
and the `ADD_EXECUTOR(...)` table in `mq5/MtApi5.mq5`, plus the payload keys each
client method sends against the keys `Execute_<Name>` reads from `jo`.

- `ERROR` lines fail the run (exit 1). They mean a command that a client sends
  has no executor, its id differs between C# and Python, or two sent commands
  share an id. Fix these before committing.
- `WARNING` lines about payload keys are regex heuristics: keys read from a
  nested object (e.g. `TradeRequest`) or built in a helper are not seen. Open
  both sides and confirm before changing anything.
- Known, accepted baseline (do not report as new): ids 68–70 shared with the
  unused `TerminalCompany/Name/Path` entries, `WindowBarsPerChart` (264) not
  wired in C#, and 11 commands declared but sent by no client. Mention only
  findings that differ from this baseline.
- `--missing-py` also lists commands `MtApi5Client` sends that `Mt5ApiClient`
  does not. Use it when the change adds a C# method.

## 2. Unit tests

- MT5 C#: `dotnet test MtApi5.Tests/MtApi5.Tests.csproj`
- Python: `python3 -m unittest discover -s PyMtApi5/tests -t .`

Run only the suites relevant to the changed layers if the user asks for a
quick check; otherwise run both. There are no MT4 (MtApi) unit tests.

## 3. Builds

Build the C# libraries that changed (in Release, as shipped):

- `dotnet build MtApi5/MtApi5.csproj -c Release`
- `dotnet build MtClient/MtClient.csproj -c Release` (if MtClient changed)
- `dotnet build MtApi/MtApi.csproj -c Release` (if MtApi changed)

Report new warnings in changed files, not the whole existing warning list.

These do **not** build on Linux. List them as "needs Windows" when they changed:
WPF test clients (`TestClients/MtApi5TestClient`, `TestApiClientUI`),
`MtService`, `MTConnector`, `MT5Connector`, installers.

## 4. MQL

If `mq5/*.mq5|*.mqh` or `mq4/*.mq4|*.mqh` changed, MetaEditor must recompile
the expert, and the rebuilt `.ex5`/`.ex4` must be committed (it can be a
separate commit, e.g. "Issue #N: mt5 - build ex5 file"). Check
`git status` to see if the `.ex5`/`.ex4` is already updated, and say so. Never
claim the MQL compiles.

If `mq5/json.mqh` or `mq5/hash.mqh` changed, the `mq4/` copies must stay
identical: `cmp mq5/json.mqh mq4/json.mqh && cmp mq5/hash.mqh mq4/hash.mqh`.

## 5. Diff hygiene

- `git diff --stat`: an unexpectedly large diff in a file often means its
  line endings changed (MtClient `.cs` and some MtService files are CRLF).
  Check with `file <path>` and `git diff --ignore-space-at-eol --stat`.
- No credentials, account numbers or broker server names in code, tests or
  examples.

## 6. Summary

End with a short table: step | result. List what still needs Windows or
MetaEditor.
