# Circle Defense Game Instructions

## Project overview

- This is a .NET 10 Raylib game. The executable project is `CircleDefenseGame\CircleDefenseGame.csproj`
- `Program.cs` owns the Raylib frame loop. `GameManager` creates the game state and manages behaviours.
- Keep gameplay changes deterministic when possible: `GameSettings.GameSeed` initializes the game's `Random`, and visual tests depend on repeatable initial state.

## Build and run

- Use the solution file from the repository root:

```powershell
dotnet build CircleDefenseGame.slnx
dotnet run --project CircleDefenseGame\CircleDefenseGame.csproj
```

- The game and its tests are Windows-specific

## Tests

- Run the full test suite from the repository root:

```powershell
dotnet test CircleDefenseGame.slnx
```

- Tests use TUnit through Microsoft.Testing.Platform
- Visual tests launch `CircleDefenseGame.exe`, wait for its window, capture the client area with Windows Graphics Capture, then compare every pixel with PNG baselines in `CircleDefenseGame.Tests\Snapshots`.
	- A missing baseline is created and treated as passing. Review and commit any newly created baseline only when the visual change is intentional. A mismatch attaches a side-by-side expected/difference/actual image to the TUnit test output.
- The game process is serialized by `GameRunner` because tests require a single interactive game window
