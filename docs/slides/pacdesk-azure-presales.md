---
marp: true
theme: default
paginate: true
footer: 'PacDesk · Introduction for Azure Presales'
title: 'PacDesk — A Cross-Platform Desktop Client for Power Platform CLI'
author: 'Jung Hyun, Nam'
style: |
  :root {
    --azure: #0078D4;
    --azure-dark: #243A5E;
    --azure-cyan: #50E6FF;
    --ink: #1B1A19;
    --muted: #605E5C;
    --line: #E1DFDD;
  }
  section {
    font-family: "Segoe UI", "Helvetica Neue", Arial, "Noto Sans KR", sans-serif;
    font-size: 26px;
    color: var(--ink);
    padding: 60px 70px;
    background: #ffffff;
  }
  h1 { color: var(--azure-dark); font-size: 48px; line-height: 1.1; }
  h2 {
    color: var(--azure-dark);
    font-size: 34px;
    border-bottom: 3px solid var(--azure);
    padding-bottom: 10px;
  }
  h3 { color: var(--azure); font-size: 26px; margin-bottom: 4px; }
  strong { color: var(--azure-dark); }
  a { color: var(--azure); }
  code {
    background: #F3F2F1; color: #004578;
    padding: 1px 6px; border-radius: 4px; font-size: 0.9em;
  }
  pre {
    background: var(--azure-dark); color: #E8EEF7;
    border-radius: 8px; font-size: 20px; padding: 18px 22px;
  }
  pre code { background: transparent; color: inherit; }
  table { font-size: 22px; border-collapse: collapse; }
  th { background: var(--azure-dark); color: #fff; padding: 8px 14px; text-align: left; }
  td { border-bottom: 1px solid var(--line); padding: 8px 14px; }
  blockquote {
    border-left: 5px solid var(--azure-cyan);
    color: var(--muted); padding-left: 18px; font-style: normal;
  }
  footer { color: var(--muted); font-size: 14px; }
  section::after { color: var(--muted); font-size: 14px; }

  /* Lead / section-divider slides */
  section.lead, section.divider {
    background: linear-gradient(135deg, #243A5E 0%, #0F2038 100%);
    color: #FFFFFF;
    justify-content: center;
  }
  section.lead h1, section.divider h1 { color: #FFFFFF; }
  section.lead h2, section.divider h2 { color: var(--azure-cyan); border: none; }
  section.lead p, section.divider p { color: #C7D2E0; }
  section.lead strong { color: var(--azure-cyan); }

  .pill {
    display: inline-block; background: #DEECF9; color: #004578;
    border-radius: 999px; padding: 4px 14px; font-size: 18px; margin: 2px;
  }
  .cols { display: flex; gap: 34px; }
  .cols > div { flex: 1; }
  .muted { color: var(--muted); }

  /* App mockup */
  .mock { border: 1px solid var(--line); border-radius: 10px; overflow: hidden; font-size: 18px; box-shadow: 0 8px 24px rgba(0,0,0,.12); }
  .mock .bar { background: var(--azure-dark); color: #fff; padding: 8px 14px; font-weight: 600; }
  .mock .status { background: #EFF6FC; color: #004578; padding: 6px 14px; border-bottom: 1px solid var(--line); }
  .mock .tabs span { display: inline-block; padding: 6px 14px; color: var(--muted); }
  .mock .tabs .on { color: var(--azure-dark); font-weight: 700; border-bottom: 3px solid var(--azure); }
  .mock .body { padding: 14px; color: var(--muted); background: #fff; }
  .mock .log { background: #0B1620; color: #E4E7EB; font-family: Menlo, Consolas, monospace; padding: 12px 14px; font-size: 15px; }
  .mock .log .g { color: #627D98; }
---

<!-- _class: lead -->
<!-- _paginate: false -->

# PacDesk

## A cross-platform desktop client for the Power Platform CLI

<br>

**Introduction for the Azure Presales team**

<span class="pill">Windows</span> <span class="pill">Linux</span> <span class="pill">macOS</span> &nbsp; · &nbsp; .NET 10 · Avalonia

<!--
Speaker: One-minute framing — PacDesk is a thin, transparent GUI over the pac CLI.
Goal today: give presales a way to talk about, demo, and position Power Platform
tooling for pro-dev / ALM conversations.
-->

---

## The problem

- **Power Platform CLI (`pac`)** is the pro-dev / ALM control plane for Dataverse — auth, environments, solutions, plug-ins, Power Pages.
- It is **CLI-only**. Powerful, scriptable… but:
  - Steep for newcomers and non-CLI stakeholders
  - Easy to run a destructive command against the **wrong environment**
  - Hard to *show* in a demo or onboarding session
- Presales & customers repeatedly ask: *"Is there a UI for this?"*

> Opportunity: a friendly, safe, cross-platform surface — **without** hiding what pac actually does.

---

## Quick context — what `pac` is

`pac` **is effectively the Dataverse CLI.** Its command groups target Dataverse directly:

| Group | Purpose |
|---|---|
| `auth` | Authentication profiles (tenants) |
| `env` | Dataverse environments (organizations) |
| `solution` | Solution ALM — export / import / pack / unpack |
| `pcf` · `plugin` · `pages` | Pro-dev components |

<span class="muted">Installed & verified in this project: pac 2.5.1 on .NET 10.</span>

---

<!-- _class: divider -->
<!-- _paginate: false -->

# Introducing PacDesk

## Wrap the CLI you already trust — with a safer, cross-platform face

---

## What PacDesk is

<div class="cols">
<div>

### A transparent wrapper
- Every button maps to a **real `pac` command**
- The exact command line + stdout/stderr is **always visible** in a live log
- No re-implementation of Dataverse APIs

</div>
<div>

### One codebase, three OSes
- Built on **Avalonia UI** (.NET)
- Native **Windows / Linux / macOS**
- Same stack Azure/.NET devs already know

</div>
</div>

<br>

> Design principle: **wrap, don't reinvent.** PacDesk is `git`/`kubectl` for pac — it drives and observes, it isn't a new IDE.

---

## What it looks like

<div class="mock">
  <div class="bar">PacDesk — Power Platform CLI</div>
  <div class="status">Active profile: user@contoso.onmicrosoft.com &nbsp;•&nbsp; Active environment: mzc-dev-environment</div>
  <div class="tabs"><span>Auth</span><span>Environments</span><span class="on">Solutions</span><span>Settings</span></div>
  <div class="body">▸ Solution grid: Unique Name · Friendly Name · Version · Managed &nbsp;— Export / Import / Publish / Delete</div>
  <div class="log"><span class="g">10:24:01</span> &gt; pac solution export --name Contoso --path ./out --managed<br><span class="g">10:24:07</span> &gt; pac solution list</div>
</div>

<br>

<span class="muted">Status bar shows the active profile + environment at all times — the anti-"wrong-tenant" guardrail.</span>

---

## What it does today

<div class="cols">
<div>

### Core workflows
- **Auth** — list / switch profiles
- **Environments** — list / select active
- **Solutions** — list · export · import · publish · delete

</div>
<div>

### Safety & UX
- Live **command log** (full transparency)
- **Active-context status bar**
- **Confirmation dialogs** before destructive ops
- **Cancel** any running command
- Native **file/folder pickers**, **settings** persistence

</div>
</div>

---

## Architecture at a glance

```
 PacDesk.App  (Avalonia, MVVM)  ──►  PacDesk.Core  (no UI)
                                         │
                                         ▼
                                    pac  (external process)
```

- **Core** = pure .NET: launches pac, streams output, typed services. Fully unit-testable.
- **App** = Avalonia presentation only; never touches the process directly.
- Clean split → the wrapper could power a CLI, web, or test harness unchanged.

---

## Engineering quality (credibility for the field)

<div class="cols">
<div>

- **0 warnings / 0 errors** build
- **22 automated tests**, incl. **real-`pac` integration** tests
- Safe process handling: args via `ArgumentList`, cancel **kills the process tree**

</div>
<div>

- Integration test **caught a real parsing bug** (pac's fixed-width columns) → fixed
- Destructive commands gated behind explicit confirmation
- Honest scope & docs (plan / architecture / roadmap)

</div>
</div>

---

## The clever bit — mechanical `--json` detection

- pac's text tables are brittle to parse; JSON is preferable **when available**.
- We **don't assume** `--json` — we **probe** `pac <verb> help` and detect it mechanically.

```
if  probe.SupportsFlag(["solution","list"], "--json")  → JSON path
else                                                    → resilient text parsing
```

- Finding: **pac 2.5.1 exposes no `--json` on list verbs** → text fallback runs today.
- The JSON path is **dormant-but-ready** — a future pac lights it up with **zero code change**.

> Forward-compatible by design — a strong "we build for longevity" talking point.

---

## Cross-platform = an Azure-dev-friendly story

- **Avalonia + .NET 10** → single codebase, native on **Windows / Linux / macOS**
- No Windows-only APIs; runs where your customers' developers run
- Same ecosystem as Azure SDKs, `dotnet`, and CI/CD pipelines
- CI-friendly: services usable headless; pac itself runnable via `dnx` with no install

<span class="muted">Fits naturally into DevOps / ALM conversations alongside Azure DevOps & GitHub Actions.</span>

---

## Where it fits — and where it doesn't

<div class="cols">
<div>

### 🟢 Strong fit
- Dataverse **dev & ALM** (solutions)
- Environment / auth **management**
- Pro-dev components (roadmap)
- **Demos, onboarding, POCs**

</div>
<div>

### 🔴 Out of scope (by design)
- Visual app / flow **authoring** → Maker portal
- **Power BI** → separate tooling
- Windows-only `pac tool` GUIs

</div>
</div>

<br>

> Honesty sells: PacDesk is the **deploy/manage/observe** surface, not a new authoring studio.

---

## Talking points for presales

- **"Is there a UI?"** → Yes — cross-platform, and it shows the exact CLI it runs.
- **Lower onboarding friction** for customers new to Power Platform ALM.
- **Safer demos**: active-context banner + confirmations reduce wrong-environment risk.
- **Pro-dev + low-code** narrative: meets developers in their OS and toolchain.
- **Trust**: open, transparent, tested — nothing hidden behind the buttons.

---

## Roadmap

| Version | Focus |
|---|---|
| ✅ v0.1–v0.2 | Auth · env · solutions · log · safety · settings · pickers |
| v0.3 | Solution **source-control** flow (`unpack`/`pack`/`clone`/`sync`) |
| v0.4 | **Auth wizard** (interactive · device-code · service principal) |
| v0.5 | Dev components — `pcf` · `plugin` · `pages` |
| v0.6 | FetchXML runner · Checker report · data tools |

<span class="muted">Later: config profiles, packaging (MSIX / AppImage / dmg), CI test matrix.</span>

---

<!-- _class: lead -->
<!-- _paginate: false -->

# Summary

**PacDesk** = a transparent, cross-platform GUI over `pac`
that makes Power Platform ALM **easier to show, safer to run, and easy to trust.**

<br>

### Ask
Feedback on presales scenarios · pilot with a customer onboarding · roadmap priorities

<br>

<span class="muted">Repo: local `main` · .NET 10 · Avalonia 11.3 · 22 tests green</span>

---

## Appendix — tech stack & layout

<div class="cols">
<div>

### Stack
- .NET 10 · C#
- Avalonia UI 11.3
- CommunityToolkit.Mvvm
- xUnit (unit + integration)

</div>
<div>

### Repo layout
- `src/PacDesk.Core` — wrapper, services
- `src/PacDesk.App` — Avalonia UI
- `tests/…` — 22 tests
- `docs/` — plan · architecture · roadmap

</div>
</div>

<br>

<span class="muted">Rendering: VS Code "Marp for VS Code" extension, or <code>marp docs/slides/pacdesk-azure-presales.md --pdf</code>.</span>
