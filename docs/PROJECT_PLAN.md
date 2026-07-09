# Crosspac — Project Plan

## 1. Vision

A cross-platform desktop GUI that makes the everyday **Power Platform CLI (`pac`)**
workflows discoverable and safe, without hiding what the CLI actually does. Target
users: Power Platform developers and ALM engineers who live in `pac` but want a
faster, less error-prone surface for repetitive tasks (switching environments,
exporting/importing solutions, inspecting auth state).

Crosspac is a **wrapper**, not a reimplementation. It shells out to the user's
installed `pac` binary. This keeps it always-compatible with new `pac` versions and
avoids duplicating Dataverse API logic.

## 2. Why Avalonia

| Requirement | How Avalonia meets it |
|---|---|
| One codebase, 3 desktop OSes | Native Windows / Linux / macOS from a single .NET project |
| Familiar stack | C# / XAML / MVVM, same mental model as WPF |
| Modern tooling | Works on .NET 10, CommunityToolkit.Mvvm source generators |
| No per-OS UI rewrite | Fluent theme renders consistently everywhere |

`pac` itself is a .NET tool, so a .NET desktop app shares the same runtime story.

## 3. Scope

### In scope (MVP — v0.1)

Derived from [SKILL.md](../.claude/skills/pac-power-platform-cli/SKILL.md):

- **Auth (`pac auth`)** — list profiles, show active, select/switch active profile.
- **Environments (`pac env`)** — list environments, select the active environment.
- **Solutions (`pac solution`)** — list, export (managed/unmanaged), import.
- **Command log** — every executed `pac` command with live stdout/stderr, exit code,
  and duration. This is the trust anchor of the whole app.
- **pac availability check** — detect whether `pac` is installed/on PATH at startup.

### In scope (later — see ROADMAP)

- `solution unpack` / `pack` / `clone` (source-control workflow) with folder pickers.
- `pac auth create` wizard (interactive + device-code + service principal).
- Developer components: `pcf`, `plugin`, `pages`.
- FetchXML query runner (`pac env fetch`).

### Out of scope (by design)

- **Visual authoring** (canvas apps, flows, forms) → belongs in Maker Portal / Studio.
- **Power BI** → separate tooling entirely.
- **`pac tool` GUIs** (PRT, CMT, Package Deployer) → Windows/.NET-Full only; conflicts
  with the cross-platform goal.
- Re-implementing Dataverse Web API CRUD → use the CLI/SDK/Web API directly.

## 4. Key risks & mitigations

| Risk | Mitigation |
|---|---|
| `pac` text output is tabular, not JSON — fragile to parse | Prefer `--json` where a verb supports it; isolate parsing in `Crosspac.Core` behind services so it's swappable. Keep raw output on every model. |
| `pac` not installed / wrong version | Startup availability check + clear guidance banner; never assume success. |
| Long-running commands (import/export) block UI | All calls are async; runner streams output; UI shows busy state and stays responsive. |
| Interactive commands (`auth create` opening a browser) | MVP only *reads* auth; creation wizard is a later, carefully-designed feature. |
| Destructive ops on the wrong environment | Always display active profile + target env; confirmation dialogs before import/publish/delete. |

## 5. Success criteria for MVP

1. `dotnet build` succeeds on a clean checkout.
2. On a machine with `pac` logged in, the Auth and Environments tabs list real data.
3. Selecting an environment runs `pac env select` and reflects the change.
4. Every command appears in the log with its exact argument list and exit code.
5. Runs on at least two of the three target OSes without code changes.

## 6. Milestones

See [ROADMAP.md](ROADMAP.md).
