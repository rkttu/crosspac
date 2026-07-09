# Crosspac — Architecture

## Overview

Two projects, strict one-way dependency:

```
Crosspac.App (Avalonia UI, MVVM)  ─────►  Crosspac.Core (no UI dependencies)
                                              │
                                              ▼
                                         `pac` process (external)
```

- **`Crosspac.Core`** knows nothing about Avalonia. It's pure C#: it launches `pac`,
  captures output, and exposes typed services. Unit-testable in isolation.
- **`Crosspac.App`** is the Avalonia presentation layer (Views + ViewModels). It never
  starts a process directly — it only talks to `Crosspac.Core` services.

This split means the wrapper logic could later be reused by a different front-end
(CLI, web, tests) without change.

## Crosspac.Core

### Execution layer (`Execution/`)

The heart of the wrapper.

- **`IPacRunner` / `PacRunner`** — starts `pac` via `System.Diagnostics.Process` with
  `ArgumentList` (never string concatenation, so arguments are safely escaped).
  Redirects stdout/stderr, reads them asynchronously line-by-line, and raises
  `OutputReceived` for live logging. Returns a `PacCommandResult`.
- **`PacCommandResult`** — `record` with `CommandLine`, `ExitCode`, `StandardOutput`,
  `StandardError`, `Duration`, and a `Succeeded` convenience flag.
- **`PacOutputLine`** — one line of streamed output, tagged `IsError` + timestamp.
- **`PacExecutionException`** — thrown when `pac` can't even be started (not installed
  / not on PATH), so the UI can show actionable guidance instead of a raw stack trace.

Design choices:
- **`ArgumentList`, not a joined string** — avoids quoting bugs and injection.
- **Streaming via events** — the command log updates in real time; the runner has no
  knowledge of the UI (it just raises events; the log VM marshals to the UI thread).
- **Cancellation** — `RunAsync` accepts a `CancellationToken` so long ops are cancelable.

### Services layer (`Services/`)

One service per `pac` command group. Each turns raw CLI output into typed models and
exposes intention-revealing methods.

- `IAuthService` → `pac auth list / select / create`
- `IEnvironmentService` → `pac env list / select`
- `ISolutionService` → `pac solution list / export / import`

Services depend only on `IPacRunner`, so they're trivially mockable.

### Models (`Models/`)

Plain records/classes: `AuthProfile`, `DataverseEnvironment`, `Solution`. **Every model
keeps the raw source line** so nothing is lost when parsing is imperfect.

### ⚠️ Output parsing — the known fragile point

`pac` prints human-oriented, column-aligned tables (not JSON) for `auth list` /
`env list` / `solution list`. Naive "split on 2+ spaces" (`TableParser.SplitColumns`)
breaks because pac pads columns to fixed widths, leaving as little as **one** space
between a full-width value and the next column — which merged the Environment ID, URL,
and Unique Name into one field. (A real-`pac` integration test caught exactly this.)

Current strategy, isolated in `Crosspac.Core.Services`:

- **`env list`** → **content-aware** parsing: extract the Environment ID by GUID shape
  and the URL by `https?://` shape (`TableParser.Guid()` / `Url()`), independent of
  spacing. This is the robust pattern to extend to other verbs.
- **`auth list` / `solution list`** → still use the 2+ space heuristic (adequate for
  their layouts today). Every model keeps `Raw`, and the command log shows true output,
  so nothing is silently lost.

Preferred long-term fix: use `--json` wherever a verb supports it and parse structured
output instead of scraping text — see the capability probe below.

### Capability probe — detecting `--json` mechanically

We do **not** assume `--json` exists. `IPacCapabilityProbe` / `PacCapabilityProbe` runs
`pac <verb> help`, parses the advertised flags (the indented `  --flag` list and the
`[--flag]` tokens in the usage line), and caches them per verb. Services then do:

```
if (probe.SupportsFlag(["solution","list"], "--json"))  → run with --json, parse JSON (PacJson)
else                                                      → fall back to text parsing
```

Reality check: on the probed install (pac 2.5.1) **no** `list`/read verb advertises a
`--json` *output* flag, so the text fallback is what actually runs today. The JSON path
is dormant but forward-compatible — the moment a newer pac exposes `--json`, services use
it automatically with no code change. `PacJson` reads properties case-insensitively and
accepts several candidate names, since the JSON schema isn't observable until then.

### Cancellation

Every service method takes a `CancellationToken`. View models create a
`CancellationTokenSource` per operation and expose a `Cancel` command; `PacRunner`
registers the token to **kill the child process tree**, so cancelling actually stops pac
rather than just abandoning the await. Cancelled operations surface as "Cancelled." in
the status line.

### Settings & pickers

- **`ISettingsStore` / `JsonSettingsStore`** (Core) persist `AppSettings` (window size,
  pac path override) as JSON under the OS per-user config dir
  (`%APPDATA%` / `~/.config`) — `~/.config/Crosspac/settings.json` on this machine. Load is
  tolerant (missing/corrupt → defaults); save is best-effort and never throws. The store
  takes an explicit path in tests.
- **pac path** is read at startup to construct `PacRunner`; the Settings tab edits it and
  applies it live via `IPacExecutable` (the runner's mutable `ExecutablePath`), then saves.
- **Window size** binds two-way and is persisted in the window's `OnClosing`.
- **`IStoragePickerService` / `StoragePickerService`** (App) wrap Avalonia's
  `StorageProvider` for native file (.zip) / folder pickers, injected into view models so
  they stay UI-agnostic.

**Never let parsing fragility leak into the UI.** Models always carry `Raw`, and the
command log always shows the true output, so the user can fall back to reading it
directly even if a parser misses a column.

## Crosspac.App (Avalonia + MVVM)

### Pattern

- **CommunityToolkit.Mvvm** source generators: `[ObservableProperty]` for bindable
  state, `[RelayCommand]` for async commands. No boilerplate `INotifyPropertyChanged`.
- **ViewLocator** resolves a `FooViewModel` to its `FooView` by naming convention, wired
  as an `Application.DataTemplates` entry. Tabs bind to child VMs and the right View is
  materialized automatically.
- **Composition root** is `App.OnFrameworkInitializationCompleted` — a small manual
  object graph (runner → services → view models). No DI container needed at MVP size;
  can graduate to `Microsoft.Extensions.DependencyInjection` later.

### View models

| ViewModel | Responsibility |
|---|---|
| `MainWindowViewModel` | Hosts child VMs + shared `CommandLogViewModel`; runs the startup `pac` availability check. |
| `AuthViewModel` | List/refresh profiles, set active. |
| `EnvironmentsViewModel` | List/refresh environments, select active. |
| `SolutionsViewModel` | List solutions, export (managed/unmanaged), import. |
| `CommandLogViewModel` | Subscribes to `IPacRunner.OutputReceived`; appends entries on the UI thread via `Dispatcher.UIThread`. |

### Threading rule

`pac` runs off the UI thread. The runner raises `OutputReceived` from a background
thread; `CommandLogViewModel` is the **only** place that marshals back to the UI thread
(`Dispatcher.UIThread.Post`). View models keep an `IsBusy` flag so the UI can disable
buttons during a command.

## Cross-platform notes

- `PacRunner` uses `FileName = "pac"` and lets the OS resolve it on PATH — works on all
  three platforms. A future setting can override the path (e.g. a pinned `dnx` invocation).
- No `System.Windows`, no P/Invoke, no Windows-only packages. `OutputType=WinExe` only
  suppresses a console window on Windows; it's inert elsewhere.
- Fonts: bundled Inter (`Avalonia.Fonts.Inter`) so text renders identically everywhere.

## Testing (`src/Crosspac.Core.Tests`, xUnit)

- **Unit tests** run services against a **`FakePacRunner`** returning canned CLI output —
  verifies argument construction + parsing without a real Dataverse tenant.
- **Integration tests** (`[Trait("Category","Integration")]`) drive the real `pac` and
  assert live data; they return early when `pac` isn't on PATH. Run only these with
  `dotnet test --filter Category=Integration`.
- Run everything with `dotnet test`.
