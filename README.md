# Crosspac

A cross-platform desktop GUI client that wraps the **Microsoft Power Platform CLI (`pac`)**.
Built with [Avalonia UI](https://avaloniaui.net/) so a single codebase runs on **Windows, Linux, and macOS**.

Crosspac is a *thin, honest* front-end over `pac`: every action it performs is a real
`pac` command, shown live in a command log. It doesn't reimplement Dataverse APIs — it
drives the CLI you already trust, with a friendlier surface for the most common tasks
(auth profiles, environments, and solution ALM).

> The feature scope and command semantics are derived from the project's
> [`pac` skill](.claude/skills/pac-power-platform-cli/SKILL.md).

## Status

✅ **v0.2 complete.** A buildable, tested cross-platform client (0 warnings / 0 errors,
**22 tests** incl. real-`pac` integration). Next work is prioritized in
[docs/IMPLEMENTATION_PLAN.md](docs/IMPLEMENTATION_PLAN.md); version milestones in
[docs/ROADMAP.md](docs/ROADMAP.md).

```bash
dotnet build src/Crosspac.slnx    # 0 warnings, 0 errors
dotnet test  src/Crosspac.slnx    # unit + (real pac) integration tests
```

### Features today

- **Auth** — list / switch authentication profiles.
- **Environments** — list / select the active Dataverse environment.
- **Solutions** — list · export · import · publish · delete.
- **Live command log** — the exact `pac` command line + stdout/stderr, always visible.
- **Active-context status bar** — the active profile + environment, at all times.
- **Safe by default** — confirmation dialogs before destructive ops (import/publish/delete).
- **Cancellable** — any running command; cancel kills the pac process tree.
- **Native file/folder pickers** and persisted **settings** (window size, pac path).
- **`--json` capability probe** — detects `--json` support per verb via `pac <verb> help`
  and prefers structured output when available, falling back to resilient text parsing.

## Prerequisites

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [Power Platform CLI (`pac`)](https://learn.microsoft.com/power-platform/developer/cli/introduction)
  installed and on `PATH` (`dotnet tool install --global Microsoft.PowerApps.CLI.Tool`)
- An existing `pac auth` profile (Crosspac reads, but does not replace, your CLI login)

## Build & run

```bash
dotnet build src/Crosspac.slnx
dotnet run --project src/Crosspac.App
```

## Layout

```
Crosspac/
├── README.md
├── docs/                          # Planning & design
│   ├── PROJECT_PLAN.md            # Vision, scope, risks
│   ├── ARCHITECTURE.md            # Layering, runner, parsing, capability probe
│   ├── ROADMAP.md                 # Version milestones (v0.1 → v0.6)
│   ├── IMPLEMENTATION_PLAN.md     # Prioritized plan for the remaining pac verbs
│   └── slides/                    # Marp deck (Azure Presales intro)
└── src/
    ├── Crosspac.slnx              # .NET 10 XML solution format
    ├── Crosspac.Core/             # UI-agnostic: pac runner, services, models, settings
    ├── Crosspac.App/              # Avalonia UI (MVVM, CommunityToolkit.Mvvm)
    └── Crosspac.Core.Tests/       # Unit (fake runner) + real-pac integration tests
```

## Documentation

- [Project plan](docs/PROJECT_PLAN.md) · [Architecture](docs/ARCHITECTURE.md)
- [Version roadmap](docs/ROADMAP.md) · [Implementation plan](docs/IMPLEMENTATION_PLAN.md) (what to build next)
- [`pac` skill reference](.claude/skills/pac-power-platform-cli/SKILL.md)
- Presentation: `docs/slides/crosspac-azure-presales.md` (render with the [Marp](https://marp.app/) CLI or VS Code extension)

## Design principles

1. **Wrap, don't reinvent.** Each button maps to a documented `pac` command.
2. **Transparency.** The exact command line and its stdout/stderr are always visible.
3. **Cross-platform first.** No Windows-only APIs; `pac tool` GUIs are intentionally out of scope.
4. **Safe by default.** Destructive commands (import/publish/delete) require explicit confirmation
   and always show the active auth profile + target environment first.

## License

Licensed under the **Apache License, Version 2.0**. See [LICENSE](LICENSE) and [NOTICE](NOTICE).

Copyright 2026 Jung Hyun, Nam
