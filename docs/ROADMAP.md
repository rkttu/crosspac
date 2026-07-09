# Crosspac — Roadmap

Incremental milestones. Each builds on the previous; the wrapper/runner core stays stable.

> For the prioritized, command-by-command plan of *which pac verbs to wrap next*, see
> [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md).

## v0.1 — MVP scaffold  ✅ (this scaffold)

- [x] Solution structure: `Crosspac.Core` + `Crosspac.App`, buildable with `dotnet build`.
- [x] `PacRunner` — async process wrapper with streamed output + exit code + duration.
- [x] Services + models for auth / env / solution.
- [x] Avalonia shell: Auth, Environments, Solutions tabs + live Command Log.
- [x] Startup `pac` availability check.

## v0.2 — Make it genuinely usable

- [x] Confirmation dialog for destructive `solution import` (shows target env + profile).
- [x] Always-visible status bar: active auth profile + active environment.
- [x] Content-aware `env list` parsing (GUID/URL extraction) — robust to pac's
      single-space column alignment. Verified by real-`pac` integration tests.
- [x] Unit + integration test project (`Crosspac.Core.Tests`) with a fake `IPacRunner`.
- [x] Capability probe (`IPacCapabilityProbe`) — mechanically detects `--json` support
      per verb via `pac <verb> help`; services take the JSON path when available and fall
      back to text otherwise. (No installed verb advertises `--json` yet, so the path is
      dormant-but-ready.)
- [x] Confirmation dialogs for the remaining destructive verbs (publish / delete).
- [x] Per-command cancellation button wired to the `CancellationToken` (kills the pac
      process tree).
- [x] Persist UI settings (window size, chosen `pac` path) to a JSON config file under
      the OS per-user config dir; a **Settings** tab edits the pac path (applied live).
- [x] Native file/folder pickers for import (.zip) and export (folder) paths.

## v0.3 — Solution source-control workflow

- [ ] `solution export → unpack` and `pack → import` with native folder/file pickers.
- [ ] Detect XML vs YAML unpack layout (CLI ≥ 2.4.1) and surface it.
- [ ] `solution clone` and `solution sync`.
- [ ] `solution version` bump UI + `solution publish`.

## v0.4 — Auth lifecycle

- [ ] `pac auth create` wizard: interactive, `--deviceCode`, and service-principal modes.
- [ ] Rename (`auth name`) and delete (`auth delete`) profiles from the UI.
- [ ] Multi-tenant switching UX.

## v0.5 — Developer components

- [ ] `pcf init` project scaffolding.
- [ ] `plugin init` / `plugin push`.
- [ ] `pages download` / `pages upload`.

## v0.6 — Data & diagnostics

- [ ] FetchXML runner (`pac env fetch`) with a results grid.
- [ ] `pac env list-settings` viewer.
- [ ] `pac solution check` results (Power Apps Checker) rendered as a report.

## Later / nice-to-have

- [ ] Command palette (fuzzy-search any `pac` verb).
- [ ] Raw command console: type any `pac ...` line and run it through the same runner.
- [ ] `Microsoft.Extensions.DependencyInjection` for the object graph.
- [ ] Unit tests with a fake `IPacRunner`; CI matrix across Windows/Linux/macOS.
- [ ] App packaging: MSIX (Windows), AppImage/deb (Linux), .app/dmg (macOS).

## Explicitly not planned

- Visual app/flow authoring, Power BI, and `pac tool` GUI launchers — see
  [PROJECT_PLAN.md §3](PROJECT_PLAN.md#3-scope).
