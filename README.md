# GTA Protest / LSPDFR Callout

A GTA V + LSPDFR public-disturbance callout project. Development is taking place on the `feature/protest-callout-foundation` branch.

## Current implementation

- A 75% random gate is evaluated once when the player first goes on duty during a plugin session.
- If enabled, the callout is registered with LSPDFR.
- The first accepted occurrence builds a scene with 100 civilian participants and a separate line of 11 police NPCs.
- Civilian groups use simple ambient idle/wander tasks to limit AI overhead.
- The police line is assigned hold-position tasks only. No automatic attack or pursuit is started.
- All created pedestrians are tracked and removed when the callout ends.
- The platform-independent core has regression checks for the one-roll session gate and crowd composition.

The 75% gate controls whether the callout is registered for the session. LSPDFR's own callout scheduler still controls when an available callout is offered.

## Repository layout

```
src/
  ProtestCallout.Core/
    Configuration/CalloutSettings.cs
    Crowd/CrowdPlan.cs
    Session/SessionCalloutGate.cs
  ProtestCallout.Plugin/
    Callouts/MassCivilUnrestCallout.cs
    Systems/CrowdScene.cs
    Main.cs
tests/
  ProtestCallout.Core.Tests/
scripts/
  Build-Plugin.ps1
.github/workflows/build.yml
```

## GitHub Actions

The workflow builds the platform-independent core and runs its regression checks. It uploads the core DLL as an Actions artifact.

The full game plugin is not built by the hosted runner yet because the required RAGE Plugin Hook and LSPDFR assemblies must match the local GTA V installation and are not included in this public repository.

## Build the game plugin locally

The plugin project targets .NET Framework 4.7.2 and resolves the API assemblies from the GTA V installation. The expected paths are:

- `<GTA V folder>\SDK\RagePluginHook.dll`
- `<GTA V folder>\Plugins\LSPD First Response.dll`

Run PowerShell from the repository:

```powershell
.\scripts\Build-Plugin.ps1 -Gta5Dir "D:\Games\Grand Theft Auto V"
```

Replace the example directory with the folder containing your `GTA5.exe`. The script checks both references and builds the plugin without copying files into the game directory.

Do not commit proprietary GTA V, RAGE Plugin Hook, or LSPDFR binaries to this public repository.

## Compatibility note

The implementation was started against the LSPDFR 0.4.9 API shape. The current LCPDFR listing identifies Build 9695; exact compatibility still needs to be verified against the installed game edition and local RAGE Plugin Hook SDK. The game-specific DLL has not yet been compiled or tested in-game.

## Performance notes

A 100-civilian scene plus 11 officers is demanding for GTA V. Pedestrians are spawned gradually rather than in one synchronous burst. This is an initial scene prototype, not a guarantee of stable performance on every PC or every game configuration. The next optimization pass should add distance-based AI activation, spawn-failure fallback, and a configurable entity cap.
