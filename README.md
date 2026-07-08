# PacDesk

A cross-platform desktop GUI client that wraps the **Microsoft Power Platform CLI (`pac`)**.
Built with [Avalonia UI](https://avaloniaui.net/) so a single codebase runs on **Windows, Linux, and macOS**.

PacDesk is a *thin, honest* front-end over `pac`: every action it performs is a real
`pac` command, shown live in a command log. It doesn't reimplement Dataverse APIs — it
drives the CLI you already trust, with a friendlier surface for the most common tasks
(auth profiles, environments, and solution ALM).

> The feature scope and command semantics are derived from the project's
> [`pac` skill](.claude/skills/pac-power-platform-cli/SKILL.md).

## Status

🚧 **MVP + early v0.2.** Buildable, tested skeleton: `pac` process runner, service
layer (auth / env / solution / context), MVVM view models, three functional tabs, a
live command log, an active-context status bar, and a confirmation dialog for
destructive `solution import`. Covered by unit tests (fake runner) and real-`pac`
integration tests. See [docs/ROADMAP.md](docs/ROADMAP.md).

```bash
dotnet build          # 0 warnings, 0 errors
dotnet test           # unit + (real pac) integration tests
```

## Prerequisites

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [Power Platform CLI (`pac`)](https://learn.microsoft.com/power-platform/developer/cli/introduction)
  installed and on `PATH` (`dotnet tool install --global Microsoft.PowerApps.CLI.Tool`)
- An existing `pac auth` profile (PacDesk reads, but does not replace, your CLI login)

## Build & run

```bash
dotnet build
dotnet run --project src/PacDesk.App
```

## Layout

```
PacDesk/
├── PacDesk.slnx              # .NET 10 XML solution format
├── README.md
├── docs/                      # Planning & design
│   ├── PROJECT_PLAN.md
│   ├── ARCHITECTURE.md
│   └── ROADMAP.md
└── src/
    ├── PacDesk.Core/          # UI-agnostic: pac runner, services, models
    └── PacDesk.App/           # Avalonia UI (MVVM, CommunityToolkit.Mvvm)
```

## Design principles

1. **Wrap, don't reinvent.** Each button maps to a documented `pac` command.
2. **Transparency.** The exact command line and its stdout/stderr are always visible.
3. **Cross-platform first.** No Windows-only APIs; `pac tool` GUIs are intentionally out of scope.
4. **Safe by default.** Destructive commands (import/publish/delete) require explicit confirmation
   and always show the active auth profile + target environment first.
