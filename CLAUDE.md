# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

`Conspiratio.Lib` holds **all game logic** of Conspiratio, a free 2D medieval strategy game, as a
netstandard2.0 library published to nuget.org. It has no UI of its own; two clients consume it:

- `C:\Projekte\Godot\Conspiratio.Godot` — the Godot 4.7 (C#/.NET 8) client, actively developed.
- `D:\Projekte\C# Projekte\Conspiratio.WinForms` — the legacy client. **Reference implementation** for
  anything not yet migrated; do not add features there, but keep it compiling.
- `D:\Projekte\C# Projekte\Conspiratio.Wiki.wiki` — the game's wiki/documentation.

Domain terms, code comments, commit messages and the CHANGELOG are **German**; keep new naming German
(`ErpressungManager`, `GetBeweispunkte`). Only this guidance file is English.

**Two hard constraints that shape every change:**

- **Language level is C# 7.3** (netstandard2.0 without an explicit `LangVersion`). No switch
  expressions, no target-typed `new`, no records, no nullable reference types.
- **Savegames must keep loading.** See the rules below — this is the single most common way to break
  players' games, and it is not caught by a build.

## Build & release

```bash
dotnet build      # also writes Conspiratio.Lib.<Version>.nupkg into Conspiratio.Lib/bin/Debug
```

`GeneratePackageOnBuild` is on, so every build produces a package. Bump `<Version>` in
`Conspiratio.Lib.csproj` per change (`<AssemblyVersion>`/`<FileVersion>` stay at the last real
release). Debug symbols are embedded (`DebugType=embedded`) so end-user stack traces carry file and
line numbers.

`CHANGELOG.md`: append new changes as bilingual bullets (all DE bullets, then all EN bullets) under the
single `## [Unreleased]` heading — **no** per-change version header or date. That block is only cut
into a dated `## <Version>` section when a real GitHub release is made.

A feature that touches both repos is committed **Lib first, then Godot**, and the Godot commit's
subject references the Lib version it depends on (`… (Conspiratio.Lib 3.95.0)`). Commit only when asked.

### Letting the Godot client see an unreleased version

The Godot repo resolves nuget.org only, so an unreleased build has to be planted in the **global NuGet
cache**:

```bash
dotnet build                                          # 1. after bumping <Version>
rm -rf ~/.nuget/packages/conspiratio.lib/<version>    # 2. NuGet won't re-extract a version it has
dotnet restore --source "D:/Projekte/C# Projekte/Conspiratio.Lib/Conspiratio.Lib/bin/Debug"  # 3. from the harness
```

Step 2 is the one that bites: rebuilding without purging leaves the old code in the cache and the Godot
build silently keeps using it.

## Verifying a change

There are no automated tests. Logic is verified with a **throwaway console harness** (net8.0) that
references the same package version and drives a real game state:

```csharp
SW.Statisch.Initialisieren();
var ngm = new NewGameManager(@"C:\temp");
ngm.CreateNewGame("Test", 1, false, true, false, out _);
SW.UI.Initialisieren(new JaNein(), new Text(), null, null, null, null, null, null, null);
var setup = new PlayerSetupManager(); setup.Starte();
setup.ErstelleSpieler("Alrik", true, 3, SW.Statisch.GetRelKathID(), 5, true, 1); setup.Beende();
SW.Dynamisch.SetAktiverSpieler(1);
// ... now exercise the manager and print/assert
```

`SW.UI` needs at least stub implementations of `IYesNoQuestion` and `IShowText`; the remaining slots may
be `null`. Three habits that repeatedly paid off:

- **Pin randomized inputs.** Much state is rolled per game (KI `Bosheit`, and with it opponent
  strength). Without pinning, two runs differ enough to look like a regression.
- **Assert on rates, not single outcomes,** for anything probabilistic — a few thousand runs. Numbers
  from a design document (e.g. the blackmail concept's 56 / 48 / 40 %) make excellent exact assertions.
- **Regression-compare a refactor against the original** over the whole input space. When
  `GetAmtsPrivilegien` was extracted to mirror the office conditions of `PrivilegienAktualisieren`, the
  harness compared both across all offices — that proved equivalence and now guards against drift.

## Architecture

### The SW facade

Static entry point `SW` (`Conspiratio.Lib.Gameplay.Spielwelt`):

- `SW.Statisch` — immutable game data (offices, laws, titles, tips). `Initialisieren()` first.
- `SW.Dynamisch` — mutable session state (players, territories, year, settings).
- `SW.UI` — the only way the Lib talks to a client: interfaces such as `IYesNoQuestion`, `IShowText`,
  `IPolitischeWeltkarteDialog`, `IDuellDialog`, `IErpressungDialog`, registered once by the client via
  `SW.UI.Initialisieren(...)`.

**Add a new `SW.UI` interface as an optional trailing parameter** of `UIHelper.Initialisieren`. Both
clients call that method, and the WinForms client must keep compiling — so callers inside the Lib have
to tolerate a `null` slot and fall back to a plain text message (`SW.Dynamisch.BelTextAnzeigen`), as in
`KontrahentenManager` case 14.

### Manager pattern

Gameplay lives in manager classes (`Allgemein/*Manager.cs` plus the `Gameplay/*` areas): a manager owns
one feature's rules, exposes `[PublicAPI]` methods returning plain data/DTOs, and leaves presentation to
the client. New features are built by extracting logic from the WinForms client into such a manager and
then putting a thin view on top. Keep the split honest — deciding *what happens* belongs here, deciding
*how it looks* does not.

### Domain model — facts that are easy to get wrong

Discovered the hard way; check these before designing around an assumption:

- **Office and territory belong together.** Use `SetAmt(amtId, gebietId)`, never `SetAmtID` alone. An
  office without its territory makes `GetAmtNameUndOrt()` and `AmtVonXfreigeben()` dereference a null
  territory — this also means a harness must not simply re-assign an office that was just vacated.
- **The law table has fixed capacity 100 and is sparsely populated**: Finanz `0–4`, Straf `20–25`,
  Kirche `40–44`, with block boundaries in `GetGesetzgrenzeFinanz/Straf/Kirche`. Adding a law is
  therefore free and needs no savegame migration — take a free index inside the right block, and both
  the court (`GerichtsverhandlungManager.SammleVorwuerfe`) and `PrivJurist` pick it up via those
  boundaries.
- **Spy evidence is a point total, not a count.** `AktiveSpionagen.GetDelikte()` accumulates the
  *strength* of each find (1–4, worded by `BeweisStaerkeText`); the concrete accusations live per law in
  `Spieler.GetBegingVerbrechenX(i)`. Use the total for thresholds, the per-law delicts to list charges.
- **Privileges are gated by office but execute generically.** All office dependence sits in the single
  `if`-chain of `DynamischeSpieldaten.PrivilegienAktualisieren()`; the 35 `Priv*.cs` act on
  `SW.Dynamisch.GetAktHum()` and only `PrivEinkommen` reads the office. Granting a player someone
  else's office privileges therefore needs no change to the privileges themselves (`GetAmtsPrivilegien`).
- **Fixed-size arrays are a recurring bug source.** Several arrays were sized by a hard-coded number
  instead of the matching `GetMax…()` constant, which overflows once the game data grows
  (`GetFreieAemterFuerSpX`, `GetUntergebene`). When touching such code, size arrays from the constants.

### Savegame compatibility

Savegames are JSON (Newtonsoft, `TypeNameHandling.Auto`, custom binder). `SpielstandContractResolver`
serializes **fields** and creates objects via `FormatterServices.GetUninitializedObject` — i.e.
**constructors never run when loading**. Consequences:

- A field added later arrives as `null` (or an array of the old length) from an existing savegame.
  Never touch such a field directly; expose it through an accessor that creates or grows it on demand —
  `Spieler.BegingVerbrechenSicher()`, `HumSpieler.GetAhnentafelListe()`, `GetErpressungen()`.
- Adding a field or a list is safe. Renaming or moving a type is not: `SpielstandJsonTypBinder` resolves
  types and keeps compatibility aliases, which is why the unit types deliberately stay in the old
  `Conspiratio.Kampf` namespace.
- One-off fix-ups for old saves belong in `SpeicherManager.AlteObjekteNachLadenAnreichern`.
- Cross-compatibility with WinForms savegames is explicitly **not** a goal; keeping *this* library's own
  older saves loadable is.

## Committing

- **Write commit messages with a POSIX heredoc**, `git commit -F - <<'EOF' … EOF`. The Bash tool is Git
  Bash, not PowerShell: a PowerShell here-string (`@'…'@`) is not parsed and prefixes a stray `@` to the
  subject line.
- **Don't use `git add -A` blindly.** The working copy carries untracked files that are not part of the
  change (`CONTRIBUTING.md`, `CONTRIBUTING-en.md`); stage the paths you touched, or check `git status`
  before committing.
