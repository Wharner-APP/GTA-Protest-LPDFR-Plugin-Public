# GTA Protest / LSPDFR Callout

A GTA V + LSPDFR callout project. The current branch contains the first platform-independent foundation; it is **not yet a playable plugin**.

## Current foundation

- Session gate that makes exactly one probability roll per session (default: 75%).
- Crowd composition planner with a minimum of 100 civilians.
- Configurable defaults for crowd size and scene options.
- GitHub Actions build for the platform-independent .NET Standard core.

## Repository layout

```
src/
  ProtestCallout.Core/
    Configuration/CalloutSettings.cs
    Crowd/CrowdPlan.cs
    Session/SessionCalloutGate.cs
.github/workflows/build.yml
```

## Build

The Actions workflow builds only `ProtestCallout.Core`. It does not yet build a RAGE Plugin Hook/LSPDFR DLL because those game-specific assemblies are not part of this public repository.

## Game adapter requirements

Before adding the in-game entry point and callout registration, confirm the installed LSPDFR and RAGE Plugin Hook versions. The adapter will need references to the matching game assemblies supplied by the developer's local GTA V installation. Do not commit proprietary GTA V, RAGE Plugin Hook, or LSPDFR binaries to this public repository.

## Performance and scene design

The 100+ crowd target is a scene-planning requirement, not a promise that every pedestrian can run full AI simultaneously. The game adapter will need distance-based activation, entity cleanup, spawn caps/fallbacks, and graceful handling of population limits. Riot officers are intended to hold a line until a scripted scene transition or player action; they should not begin a blanket attack on spawn.
