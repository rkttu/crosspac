# PacDesk — Implementation Plan (pac command coverage)

This document prioritizes **which `pac` verbs to wrap next**, and in what order.

- [ROADMAP.md](ROADMAP.md) tracks *version milestones* (v0.1 → v0.6).
- **This file** tracks *command coverage* — the concrete pac verbs, scored and sequenced.

Grounded in the installed CLI: **pac 2.5.1** (.NET 10). Verb availability was enumerated
from `pac <group> help`, so this plan reflects what the CLI actually exposes today, not
just the online docs.

## Current coverage

| Group | Implemented | Available but not yet wrapped |
|---|---|---|
| **auth** | list, select, (create — service only) | **who, name, delete, update, clear** + a create UI |
| **env** | list, select | **who, list-settings**, update-settings, **fetch** |
| **solution** | list, export, import, publish, delete | **unpack, pack, clone, sync, version, online-version, check**, upgrade, init, add-reference, add-solution-component, create-settings, add-license |
| **pages** | — | list, download, upload, clone, migrate-datamodel, … |
| **pcf / plugin** | — | init, push |
| **connection / connector** | — | list, create, update, delete, (connector) download, init |
| **catalog / pipeline / application** | — | catalog list/install/status, pipeline list/deploy, application list/install |
| **copilot** | — | list, status, publish, extract/merge-translation, … |

> `pac data` exposes no subcommands in this build, so bulk data import/export is out of
> scope until a pac version reinstates it.

## Prioritization criteria

Each candidate is weighed on:

1. **Value** — usefulness for Dataverse dev/ALM **and** presales demo impact.
2. **Effort** — parameters, interactivity, need for pickers, long-running/streaming, JSON.
3. **Risk** — read-only vs write vs destructive (live-environment mutation).
4. **Reuse** — how much it leans on infrastructure we already have: path pickers,
   confirmation dialogs, cancellation, the command log, and the `--json` capability probe.

## Recommended first step → Phase A

**Complete the Auth and Environments tabs.** Rationale:

- **Lowest effort, lowest risk** — simple flag commands, mostly read-only, no new infra.
- **Completes the foundation everything else needs** — every solution operation requires
  an auth profile and a selected environment first.
- Immediate polish for credible end-to-end demos (auth → env → solution).

## Phased plan

| Phase | Scope | Value | Effort | Risk |
|---|---|---|---|---|
| **A — Foundation** | auth `who` / `name` / `delete` / `update` + **create wizard** (interactive · `--deviceCode` · service principal); env `who` / `list-settings` | Medium | **Low** | Low |
| **B — Solution source control** (headline) | `export → unpack`, `pack → import`, `clone`, `sync`, `version` / `online-version` | **High** | Medium | Medium |
| **C — Demo-worthy insight** | env **`fetch`** (FetchXML runner + results grid), solution **`check`** (Power Apps Checker report) | **High (demo impact)** | Medium | Low |
| **D — Pro-dev & Power Pages** | pages `list` / `download` / `upload`; pcf & plugin `init` / `push` | Medium | Medium–High | Low–Medium |
| **E — Ecosystem** (later) | connection, connector, catalog `install` / `status`, pipeline `deploy`, application `install`, copilot `list` / `status` | Situational | High | Medium |

## Phase A — task breakdown (ready to start)

- **`AuthService`**: add `WhoAsync`, `RenameAsync(index, name)`, `DeleteAsync(index)`,
  `UpdateAsync(...)`; surface the existing `CreateAsync` behind a **create wizard**
  dialog (mode: interactive / `--deviceCode` / service principal).
- **`EnvironmentService`**: add `WhoAsync`, `ListSettingsAsync` (read-only viewer).
- **UI**: Auth tab gains rename/delete/update/create actions; Environments tab gains a
  "who am I" panel and a settings viewer.
- **Reuse**: destructive `delete` / `clear` go through the existing confirmation dialog;
  read commands surface transparently in the command log.
- **Only non-trivial item**: `auth create` is *interactive* (browser or device-code). Solve
  the device-code flow here — it becomes the template for any future interactive command.

## Cross-cutting engineering considerations

Address these once, early, so later phases reuse them:

1. **Long-running commands** (import · check · upgrade · pipeline deploy · copilot status):
   design an `--async` + status-polling pattern at the runner level **before** Phase B.
2. **Results-grid component** (fetch · check · pages list): build one reusable grid in
   Phase C and share it.
3. **Interactive commands**: the `auth create` device-code flow (Phase A) is the reusable
   pattern for anything that needs a browser/interactive handshake.
4. **JSON everywhere for free**: every new verb goes through the `IPacCapabilityProbe`, so
   it automatically prefers `--json` if a future pac version adds it — no per-verb code.

## Notes on pac 2.5.1 (observed)

- No `list`/read verb advertises a `--json` **output** flag → text parsing runs today; the
  JSON path stays dormant-but-ready.
- Many verbs require an **active environment**; the status bar + env selection built in
  v0.2 are the prerequisite and are already in place.
- `pac tool` GUI launchers remain **out of scope** (Windows/.NET-Full only; conflicts with
  the cross-platform goal).
