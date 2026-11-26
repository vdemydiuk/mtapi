---
name: add-api-function
description: Expose a new MetaTrader (MQL) function through the mtapi stack end to end, from the MQL expert to the C# client and (for MT5) the Python client PyMtApi5. Use when the user asks to add, implement, or wire up an API function or command in MtApi (MT4) or MtApi5 (MT5).
disable-model-invocation: true
---

# Add an API function to mtapi

A function must be added consistently across several layers. This skill is a
checklist; always verify each step against the real code first.

## 0. Before editing

1. Ask which platform: MT5 (MtApi5, mq5), MT4 (MtApi, mq4), or both.
   Default to one platform per change unless the user says otherwise.
2. Check the issue number (branch name or user request). Work happens on
   `dev` or a feature branch created from `dev` (e.g. `issue_345_...`);
   never on `master`. If the current branch is `master`, stop and ask.
3. Find the closest existing function with the same shape (same parameter
   and return types) and trace it through every layer below. Copy its
   conventions exactly. Good references for MT5: `Alert`, `Print`,
   `SymbolInfoInteger`.
4. Read the MQL docs for the function being wrapped (name, parameters,
   return type, enum types).

## 1. Command enum (C#)

- MT5: `MtApi5/MtProtocol/Mt5CommandType.cs`. MT4: the equivalent
  `MtCommandType` file in `MtApi`.
- Some commands may already be declared there but not wired up (look for
  `//TODO`). Reuse the existing id; otherwise add a new one with the next
  unused number. Never renumber existing commands.

## 2. MQL expert handler

- MT5: `mq5/MtApi5.mq5`. MT4: `mq4/`.
- Add `Execute_<Name>` following the style of nearby handlers:
  read the JSON payload, extract each parameter by its key, call the MQL
  built-in, return the result as JSON.
- Register the executor in `preinit()` with `ADD_EXECUTOR(<id>, <Name>);`
  in numeric order of the command id. The executor name should match the
  C# enum name.
- Payload key names must match exactly what the C# client sends
  (case-sensitive).
- Strings coming from the broker that go into JSON must be escaped at the
  call site; `JSONString::toString()` does not escape.
- Do not change other handlers unless required.

## 3. C# client

- MT5: `MtApi5/MtApi5Client.cs`. MT4: `MtApi/MtApiClient.cs`.
- Add a public method with an XML doc comment (copy the wording of the MQL
  docs, shortened) and the same name as the MQL function.
- Build a `Dictionary<string, object>` of parameters using the exact
  payload keys from step 2, then call `SendCommand` with the new command
  type. Mirror an existing method for error handling and null responses.
- Convert times with the existing time converter helpers (for MT5,
  `Mt5TimeConverter`); do not hand-roll conversions.
- Add new DTO/struct classes next to similar existing ones, with default
  constructors.
- Follow the file's existing ordering (methods are grouped by MQL category).

## 4. Python client (MT5 only)

Every MT5 function is also added to PyMtApi5 in the same change (both
clients currently send the same commands, apart from the C#-only `Copy*1/2`
overloads, `OrderCloseAll`, `PositionOpen`, `SymbolInfoString2` and
`BacktestingReady`).

- `PyMtApi5/mt5commandtype.py`: add the same name and id as in
  `Mt5CommandType.cs`, in the same position.
- `PyMtApi5/mt5apiclient.py`: add a snake_case method in the matching
  category section, preceded by a `# <MqlName>` comment, e.g.
  `symbol_info_integer`. Build `cmd_params` with **the same payload keys** as
  the C# method and call
  `self.__send_command(self.__get_default_expert(), Mt5CommandType.X, cmd_params)`.
  Convert enum results with the enum classes in `mt5enums.py`.
- New result classes go in `mt5apiclient.py` next to similar ones
  (`__init__(self, j)` reading the C# JSON names, plus `__repr__`), and are
  exported in `PyMtApi5/__init__.py` (import list and `__all__`).
- Unit test in `PyMtApi5/tests/test_<category>.py` (trading, history,
  global_variables, charts, objects, terminal, indicators, common): add a row
  to the `CASES` table of a `TradingTestCase` subclass
  `(name, call, command, payload, reply, expected)`. Write a separate test
  for result classes and conversions.
- `TestClients/TestPyMtApi5/` (flet test app): add the function to the
  matching tab in `command_tabs.py`.

## 5. Test clients and samples

C# WPF test client:

- `TestClients/MtApi5TestClient` (MT5) or the MT4 equivalent.
- Add a button in `MainWindow.xaml` and a command in `ViewModel.cs`, next to
  similar functions. Reuse existing input textboxes where possible.
- Skip UI for functions that are hard to test safely (anything that places
  or modifies trades must not run automatically).

If the function is a new feature (not just a missing MQL wrapper), consider
an example in `Examples/MtApi5Samples`. Ask the user before adding one.

## 6. Unit test (MT5)

- Add a test in `MtApi5.Tests` for each new `MtApi5Client` method. Put it in
  the file matching its category (`MtApi5ClientCommandTests.cs`,
  `...SeriesTests.cs`, `...IndicatorTests.cs`, `...TableTests.cs`); model
  classes go to `MtApi5ModelTests.cs`.
- Use `MtApi5ClientTestHelper.Connect()` to get a client over `FakeRpcClient`,
  `fake.Reply("<value json>")` to set the expert response, then assert
  `fake.Last().CommandType` and the keys/values of `fake.LastPayload()`
  (these must match the MQL handler keys), plus the converted return value.
- Run `dotnet test MtApi5.Tests/MtApi5.Tests.csproj`; report failures.
  There are no unit tests for MtApi (MT4) yet.

## 7. Build and verify

- Run the `verify` skill: protocol cross-check (command ids and payload keys
  across C#, Python and mq5), MtApi5 and PyMtApi5 unit tests, and the C#
  builds. Also run `check_protocol.py --missing-py` and confirm the new
  command is not listed.
- The `.mq5`/`.mq4` file must be compiled with MetaEditor, which is not
  available on Linux. Do not claim it compiles. Tell the user the expert
  needs recompiling in MetaEditor, and that the precompiled `.ex5`/`.ex4`
  in the repo must be updated and committed with the change.
- The cross-check is regex based. For payloads with nested objects, also
  compare the keys by hand.

## 8. Safety review (trading functions)

If the function sends, modifies, or closes orders or positions, stop and
review: parameter validation, handling of failed results, and no default
values that could trade unexpectedly. Recommend testing on a demo account
only. Never put credentials, account numbers, or server names in code or
examples.

## 9. Finish

- Summarize the files changed per layer (enum, mq5, C#, Python, tests,
  test clients) and what still needs manual action (recompile the expert in
  MetaEditor and commit the `.ex5`/`.ex4`, test on a demo terminal).
- Suggest a commit message in the repo's format via the commit-message skill.
- Do not commit or push unless the user asks.
