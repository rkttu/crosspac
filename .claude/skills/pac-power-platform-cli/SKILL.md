---
name: pac-power-platform-cli
description: >-
  Use the Microsoft Power Platform CLI (`pac`) to manage Dataverse
  environments, authentication profiles, and solutions (export / import /
  unpack / pack / clone), plus PCF, plug-ins, and Power Pages. Trigger whenever
  the user wants to work with Power Platform / Dataverse / Power Apps from the
  command line, do solution ALM / source control, or asks about a `pac ...`
  command.
---

# Power Platform CLI (`pac`)

`pac` is Microsoft's official command-line tool for Power Platform / Dataverse.
It manages authentication, environments, solutions (ALM), and developer
components (PCF, plug-ins, Power Pages, code apps). Installed as a .NET global
tool at `~/.dotnet/tools/pac`.

Official reference: https://learn.microsoft.com/power-platform/developer/cli/reference/

## When to use this skill

- Connecting to a Dataverse tenant/environment or switching between them.
- Solution lifecycle: export, import, unpack/pack (source control), clone, sync,
  version, publish, check, upgrade.
- Scaffolding/pushing developer components: `pcf`, `plugin`, `pages`.
- Any request phrased as "run `pac ...`" or "using the Power Platform CLI".

## First: orient yourself before acting

Auth and environment state is global to the machine, so **always confirm context
before running any command that writes to Dataverse**:

```bash
pac auth list      # which profiles exist, which is Active (*)
pac auth who       # current account + connected environment
pac env list       # environments visible to the active profile
```

Key gotcha: a profile can be **authenticated but have no active environment
selected** (the `Active` column in `pac env list` is blank). Import/export/publish
commands need a target — set one first:

```bash
pac env select --environment <URL | ID | unique-name | partial-name>
# e.g. pac env select --environment https://orgc658acec.crm21.dynamics.com/
```

Almost every write command also accepts a per-invocation `--environment` (`-env`)
override, so you don't have to change the global selection if you only need it once.

## Core command groups

| Group | Purpose |
|---|---|
| `pac auth` | Authentication profiles (create, list, select, delete, who) |
| `pac env` | Environments (list, select, who, fetch, settings) |
| `pac solution` | Solution ALM — the most-used group |
| `pac pcf` | Power Apps Component Framework projects |
| `pac plugin` | Dataverse plug-in class libraries |
| `pac pages` | Power Pages websites (download / upload) |
| `pac pipeline` | Deployment pipelines |
| `pac tool` | Launch companion GUIs (PRT, CMT, Package Deployer) — **Windows/.NET Full only** |
| `pac data` | Import/export Dataverse data |
| `pac admin` | Tenant/admin operations (create/copy/backup envs, service principals) |

See all: `pac help`. See one group's verbs: `pac <group> help`. See a verb's full
options: `pac <group> <verb> help` (e.g. `pac solution export help`).

## Authentication

```bash
# Interactive login to a specific environment, named for reuse
pac auth create --name Contoso-Dev --environment "HR-Dev"

# Service principal (CI/CD, non-interactive)
pac auth create --name Contoso-SPN \
  --applicationId <appId> --clientSecret <secret> --tenant <tenantId>

# Device code (WSL2, Codespaces, headless — no browser available)
pac auth create --environment <envId> --deviceCode

pac auth list                 # list profiles (note the * = active)
pac auth select --index 2     # switch active profile
pac auth name --index 1 --name "Contoso Dev"   # (re)name a profile
pac auth delete --index 2     # remove one profile
pac auth clear                # remove all profiles
```

Notes:
- Use `--environment` (accepts ID, URL, unique name, or partial name). `--url` is
  deprecated — and using `--url` creates a `DATAVERSE`-kind profile rather than a
  `UNIVERSAL` one, which some commands (e.g. `power-fx`) reject.
- Cloud values for sovereign clouds: GCC=`UsGov`, GCC High=`UsGovHigh`, DoD=`UsGovDod`
  (via `--cloud`).

## Solution ALM — the primary workflow

The canonical source-control loop: **export → unpack → commit → (edit) → pack → import**.

```bash
pac solution list                       # solutions in the active environment

# Export a solution as a zip (use the solution's unique Name, not display name)
pac solution export --name MySolution --path ./out --managed        # managed
pac solution export --name MySolution --path ./out                  # unmanaged

# Decompose the zip into source files for Git
pac solution unpack --zipfile ./out/MySolution.zip --folder ./src --packagetype Both

# ...edit files under ./src, commit to source control...

# Recompose source back into a deployable zip
pac solution pack --zipfile ./MySolution.zip --folder ./src --packagetype Managed

# Import into the target environment (override target inline if needed)
pac solution import --path ./MySolution.zip --environment <target URL|ID>
```

Other useful solution verbs:

```bash
pac solution init --publisher-name Contoso --publisher-prefix con   # new project
pac solution clone --name MySolution        # export + unpack into a full project in one step
pac solution sync                            # re-sync local project to the online solution
pac solution version --revision 1            # bump build/revision
pac solution publish                         # publish all customizations
pac solution check --path ./MySolution.zip   # run Power Apps Checker (quality/analysis)
pac solution upgrade --solution-name MySolution --async --max-async-wait-time 60
pac solution delete --solution-name MySolution
```

### unpack/pack format nuance (important)

- Default `pac solution unpack` produces the classic **XML** layout
  (`Other/Solution.xml`, `Controls/`, etc.).
- Solutions from **Dataverse Git integration** or `pac solution clone` use the newer
  **YAML** layout under `solutions/<Name>/` (`solution.yml`, `solutioncomponents.yml`,
  ...). YAML support requires **CLI ≥ 2.4.1**.
- To repack YAML, use `pac solution pack --folder <root>` — the presence of a
  `solutions/` subdirectory auto-selects YAML. Putting YAML files at the repo root
  instead causes a misleading "missing Customizations.xml" error.
- `unpack`, `clone`, and `sync` do **not** currently emit YAML on their own; only
  Git integration and `clone` produce the YAML layout that `pack` can consume.

`pac solution` supersedes the old standalone SolutionPackager.exe — same engine,
now built into the CLI.

## Developer components (quick reference)

```bash
# PCF (Power Apps Component Framework)
pac pcf init --namespace Contoso --name MyControl --template field

# Dataverse plug-in class library
pac plugin init
pac plugin push --pluginId <assembly-or-package-id>   # import into Dataverse

# Power Pages
pac pages download --path ./site --webSiteId <id>
pac pages upload   --path ./site
```

## Environment data / queries

```bash
pac env who                                   # current org info
pac env fetch --xml "<fetch>...</fetch>"       # run a FetchXML query
pac env list-settings --filter <name>          # read org settings
pac data export / pac data import              # bulk data movement
```

## CI/CD without installing globally

With .NET 10+, run any command without a global install (good for pipelines):

```bash
dnx Microsoft.PowerApps.CLI.Tool --yes env list
# i.e. replace `pac` with `dnx Microsoft.PowerApps.CLI.Tool --yes`
```

## Practical tips

- Prefer the solution **unique Name** (not display name) everywhere it's requested.
- Add `--json` to admin/list commands to get machine-readable output for scripting.
- Long operations: many verbs accept `--async` + `--max-async-wait-time <minutes>`.
- `pac tool ...` GUIs (Plugin Registration Tool, Configuration Migration, Package
  Deployer) are **Windows / .NET Full Framework only** — not available on macOS/Linux.
- Discover options interactively with the layered help: `pac`, `pac <group> help`,
  `pac <group> <verb> help`.

## Safety

Import, publish, delete, upgrade, and any `pac admin` write command mutate a live
environment. Before running one, confirm the **active auth profile and target
environment** (`pac auth who`) so you don't push changes to production by mistake.
When the intended target differs from the active one, pass `--environment`
explicitly rather than relying on global state.
