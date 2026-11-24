---
name: commit-message
description: Write a commit message for staged changes in the mtapi repo, in the form "Issue #N: Component - Description". Use when the user asks to commit, write a commit message, or summarize staged changes.
---

# Commit message for mtapi

## Steps

1. Run `git status` and `git diff --staged`. If nothing is staged, tell the
   user and ask whether to stage the changes. Never run `git add -A`
   without asking.
2. Find the issue number, in this order:
   - the user's request (e.g. "commit for issue 300")
   - the current branch name (`git branch --show-current`), e.g. `issue-300`,
     `300-getquote`
   - otherwise, ask the user. Do not guess or invent a number.
   If the user says there is no issue, omit the `Issue #N: ` prefix.
3. Check the diff for things that must not be committed:
   - credentials, account numbers, passwords, or broker server names
   - build output (`build/`, `bin/`, `obj/`, `*.msi`)
   - unrelated changes mixed together (suggest splitting into several commits)
4. Write the message using the format below.
5. Show the message to the user and commit only after they confirm.

## Format

    Issue #<number>: <Component> - <Description>

Single line by default. Add a body only if the change needs extra explanation.

- Prefix: exactly `Issue #<number>: ` (capital I, plain text, no markdown link).
- Component: the part of the project that changed, followed by ` - `:

  | Component        | Meaning                                    |
  |------------------|--------------------------------------------|
  | MtApi            | MT4 C# client library                      |
  | MtApi5           | MT5 C# client library                      |
  | MtClient         | C# WebSocket client                        |
  | MtService        | C++ WebSocket server                       |
  | MTConnector      | MT4 C++ connector DLL                      |
  | MT5Connector     | MT5 C++ connector DLL                      |
  | mq4 / mq5        | MQL Expert Advisors                        |
  | Installer        | WiX installers                             |
  | Examples         | Examples and TestClients                   |

  For several components, list them separated by commas (`MtApi5, mq5 - ...`).
  For repo-wide changes (README, solution files), omit the component.
- Description: starts with a capital letter, past tense ("Added", "Fixed",
  "Updated", "Removed", "Changed"), no trailing period. Describe what changed
  and its effect for users. Include important details such as default values
  or units in the same line, e.g. "Default value 30 sec".
- Keep it to one sentence. Do not pad it; length follows the content.
- Do not use Conventional Commits types or scopes (no `fix(mtapi5):`).
- Do not add "Fixes #N" or "Closes #N" unless the user asks, because
  those words close the issue automatically on GitHub.

## Repo-specific rules

- If `.mq4`/`.mq5` sources changed, mention whether the precompiled
  `.ex4`/`.ex5` files were rebuilt (in a body line).
- Keep MT4 and MT5 changes in separate commits unless they are one logical change.
- Do not add "Generated with Claude" or co-author trailers unless the user asks.

## Examples

    Issue #296: MtApi5 - Added property CommandTimeout to define time for waiting result of response from MetaTrader. Default value 30 sec

    Issue #300: MtClient - Added command type and a hint about expert version to "Unknown command type" error

    Issue #300: Updated README with troubleshooting note for outdated expert file

    Issue #215: MtApi - Fixed exception in OrderSend when volume is zero
