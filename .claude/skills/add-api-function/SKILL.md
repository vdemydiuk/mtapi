---
name: add-api-function
description: Expose a new MetaTrader (MQL) function through the mtapi stack end to end, from the MQL expert to the C# client. Use when the user asks to add, implement, or wire up an API function or command in MtApi (MT4) or MtApi5 (MT5).
disable-model-invocation: true
---

# Add an API function to mtapi

A function must be added consistently across several layers. This skill is a
checklist; always verify each step against the real code first.

## 0. Before editing

1. Ask which platform: MT5 (MtApi5, mq5), MT4 (MtApi, mq4), or both.
   Default to one platform per change unless the user says otherwise.
2. Check the issue number (branch name or user request) and the branch to
   target: run `git branch -a` and ask whether to base the work on `master`
   or `dev`.
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
- Register the executor in `preinit()` in numeric order of the command id.
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

## 4. Test client

- `TestClients/MtApi5TestClient` (MT5) or the MT4 equivalent.
- Add a button in `MainWindow.xaml` and a command in `ViewModel.cs`, next to
  similar functions. Reuse existing input textboxes where possible.
- Skip UI for functions that are hard to test safely (anything that places
  or modifies trades must not run automatically).

## 5. Unit test (MT5)

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

## 6. Build and verify

- Build the changed C# projects: `dotnet build <project>.csproj -c Release`
  for `MtApi5` (and `MtClient` if touched). Report warnings and errors.
  Some projects (WPF test client, C++ connectors, installers) may only build
  on Windows; say so instead of claiming success.
- The `.mq5`/`.mq4` file must be compiled with MetaEditor, which is not
  available on Linux. Do not claim it compiles. Tell the user the expert
  needs recompiling in MetaEditor, and that the precompiled `.ex5`/`.ex4`
  in the repo must be updated and committed with the change.
- Check that every payload key in the C# method appears in the MQL handler
  and vice versa (grep both).

## 7. Safety review (trading functions)

If the function sends, modifies, or closes orders or positions, stop and
review: parameter validation, handling of failed results, and no default
values that could trade unexpectedly. Recommend testing on a demo account
only. Never put credentials, account numbers, or server names in code or
examples.

## 8. Finish

- Summarize the files changed per layer and what still needs manual
  action (recompile, test on a demo terminal).
- Suggest a commit message in the repo's format via the commit-message skill.
- Do not commit or push unless the user asks.
