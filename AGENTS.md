# AGENTS.md - RealMK

This file provides guidance for AI coding agents working on this project.

## Project Overview

**RealMK** is a digital adaptation of a Mage Knight-style board game built with:
- **Engine**: Godot 4.6 with C# (.NET 8.0)
- **Architecture**: Command/Event pattern with deterministic game state
- **Testing**: xUnit with a separate test project

The game features hex-based maps, deck-building mechanics, combat, and hero progression.

## Directory Structure

```
├── content/             # JSON game data definitions
│   ├── cards/           # Card definitions (basic_actions.json, etc.)
│   └── tiles/           # Tile definitions for hex map
├── nodes/               # Reusable Godot nodes (currently empty)
├── scenes/              # Godot scenes and their scripts
│   ├── game_manager/    # GameManager - main game facade
│   ├── world_map/       # WorldMap scene
│   └── test_scenes/     # Test/debug scenes
├── scripts/             # Pure C# game logic (engine-agnostic where possible)
│   ├── cards/           # CardDefinition, CardInstance
│   ├── commands/        # IGameCommand interface and implementations
│   ├── content/         # JSON parsers and ContentLoader
│   ├── core/            # GameState, EventBus, CommandDispatcher
│   ├── effects/         # IEffect and effect implementations
│   ├── events/          # IGameEvent interface and implementations
│   ├── mana/            # ManaPool and mana logic
│   ├── map/             # Hex coordinate system, MapState, TileDefinition
│   └── util/            # Utilities: Log, DeterministicRandom, Ids, Enums
└── tests/               # xUnit test project
```

## Code Architecture

### Core Patterns

1. **Command/Event Sourcing**: Player actions are `IGameCommand` objects that produce `IGameEvent` results. This enables replays, undo, and network sync.

2. **Deterministic Game State**: All randomness uses `DeterministicRandom` (XorShift128+) with seeds for reproducibility.

3. **Content-Driven**: Game data (cards, tiles, enemies) is defined in JSON and loaded via `ContentLoader`.

4. **Strongly-Typed IDs**: Use wrapper structs (`PlayerId`, `CardId`, `TileId`, etc.) instead of raw primitives.

### Key Components

| Component | Purpose |
|-----------|---------|
| `GameState` | Root aggregate containing all game state |
| `GameManager` | Godot Node facade for presentation layer |
| `CommandDispatcher` | Validates and executes commands, publishes events |
| `EventBus` | Pub/sub for decoupled event handling |
| `ContentDatabase` | Holds loaded definitions (cards, tiles, etc.) |

### Hex Coordinate System

Uses **axial coordinates** (q, r) with cube coordinate derivation:
- `X = Q`, `Y = -Q - R`, `Z = R`
- See `HexCoord` for operations (distance, neighbors, directions)

## Coding Conventions

### C# Style

- **Namespace**: All game code uses `namespace RealMK;`
- **Nullable**: Enabled globally (`<Nullable>enable</Nullable>`)
- **Access Modifiers**: Use explicit access modifiers
- **Properties**: Prefer `{ get; init; }` for immutable DTOs and definitions
- **Records**: Use `record struct` for value types with equality (see `HexCoord`, ID types)

### Naming

- **Classes/Interfaces**: PascalCase (`CardDefinition`, `IGameCommand`)
- **Methods/Properties**: PascalCase (`DistanceTo`, `BasicEffect`)
- **Private Fields**: `_camelCase` with underscore prefix
- **Constants**: PascalCase (`DirectionNE`)
- **Enums**: PascalCase for both type and values

### Documentation

- XML doc comments on all public APIs
- `<summary>` tags required for classes, interfaces, and public members
- Use `<param>`, `<returns>`, `<remarks>` where appropriate

### Godot Integration

- Scene scripts use `partial class` for Godot source generation
- Keep game logic separate from Godot-specific code where possible
- Use `ProjectSettings.GlobalizePath()` for `res://` paths in C#

## Content Format

### Card JSON Structure
```json
{
  "cards": [{
    "id": "card.march",
    "type": "basic",
    "color": "green",
    "nameKey": "card.march.name",
    "descriptionKey": "card.march.desc",
    "sidewaysValue": 1,
    "basicEffect": { "type": "movement", "value": 2 },
    "enhancedEffect": { "type": "movement", "value": 4 }
  }]
}
```

### Tile JSON Structure
```json
{
  "tiles": [{
    "id": "tile.starting_01",
    "category": "starting",
    "hexes": [
      { "q": 0, "r": 0, "terrain": "plains" },
      { "q": 1, "r": -1, "terrain": "forest", "locationId": "loc.village" }
    ]
  }]
}
```

## Testing

### Mandatory Testing

**Every code change MUST include corresponding tests.** No exceptions.

- **Framework**: xUnit
- **Project**: `tests/RealMK.Tests.csproj` (references main project)
- **Naming**: `[ClassName]Tests.cs` with descriptive method names
- **Categories**: Use `[Trait("Category", "Unit")]` for categorization

### Running Tests
```powershell
dotnet test tests/RealMK.Tests.csproj
```

### Test Patterns
```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var coord = new HexCoord(3, -2);
    
    // Act
    int distance = coord.DistanceTo(new HexCoord(0, 0));
    
    // Assert
    Assert.Equal(3, distance);
}
```

## Logging

Use the static `Log` class with appropriate levels:
```csharp
Log.Trace("Detailed trace info");
Log.Debug("Debug information");
Log.Info("General information");
Log.Warning("Warning message");
Log.Error("Error message");
```

Log methods auto-capture file, line, and member name via `[Caller*]` attributes.

## Common Tasks

### Adding a New Card Type
1. Add effect type to `EffectType` enum if needed
2. Implement `IEffect` interface for new effect
3. Update `CardParser` to parse the new effect
4. Add JSON definitions to `content/cards/`
5. Add unit tests

### Adding a New Command
1. Create class implementing `IGameCommand` in `scripts/commands/`
2. Add validation in rules engine (when implemented)
3. Add execution logic producing `IGameEvent` results
4. Add unit tests

### Adding a New Tile Feature
1. Update `TileHexDefinition` if new hex properties needed
2. Update `TileParser` and DTOs
3. Add to `content/tiles/` JSON
4. Update map rendering logic

## Build & Run

### Prerequisites
- .NET 8.0 SDK
- Godot 4.6 with .NET support

### Building
```powershell
# Build main project
dotnet build RealMK.csproj

# Build tests
dotnet build tests/RealMK.Tests.csproj
```

### Running
Open `project.godot` in Godot Editor and run, or use:
```powershell
godot --path . --editor
```

## Running Godot Scenes (REQUIRED)

When modifying Godot scenes or scripts in `Nodes/`, **you MUST verify the changes work at runtime**.

### Finding Godot

Godot location varies by machine. Common approaches:

```powershell
# Search for Godot executable (mono/C# version required)
Get-ChildItem -Path "C:\" -Filter "*Godot*mono*.exe" -Recurse -ErrorAction SilentlyContinue -Depth 5 | Select-Object FullName -First 5

# Check common locations
Get-ChildItem "C:\Programs\Godot*" -Recurse -Filter "*.exe" | Where-Object { $_.Name -like "*mono*" -and $_.Name -like "*console*" }
```

Use the **console** version (e.g., `Godot_v4.6-stable_mono_win64_console.exe`) for terminal output.


### Verification Commands

```powershell
# 1. Build C# first (required before running)
dotnet build RealMK.csproj

# 2. Check for Godot errors (headless mode, quick validation)
& "path\to\Godot_console.exe" --path . --headless --quit 2>&1

# 3. Run specific scene to test
& "path\to\Godot_console.exe" --path . res://scenes/test/TestMenu.tscn 2>&1
```

### Reading Godot Output

When running Godot from terminal, watch for:

| Output Type | Meaning | Action |
|-------------|---------|--------|
| `ERROR:` | Critical failure, scene won't work | Fix immediately |
| `WARNING:` | Non-fatal issue, may cause problems | Fix if possible |
| `Script error` | C# runtime exception | Check exception details |
| No output | Scene loaded successfully | ✓ Good |

### Common Godot Errors

| Error | Cause | Fix |
|-------|-------|-----|
| `Cannot open file 'res://...'` | Script path wrong or file moved | Update `.tscn` script path |
| `invalid UID` | Stale UID cache after moving files | Remove UID from `.tscn` ext_resource or delete `.godot/uid_cache.bin` |
| `Failed loading resource` | Missing dependency | Verify all referenced files exist |
| `Parse Error: [ext_resource]` | Invalid scene reference | Fix path in `.tscn` file |
| `Cannot find class` | C# not built or namespace wrong | Run `dotnet build`, check namespace |

### Clearing Godot Cache

If you encounter stale reference errors after moving files:

```powershell
# Remove cached files (Godot will regenerate)
Remove-Item ".godot/uid_cache.bin" -Force -ErrorAction SilentlyContinue
Remove-Item ".godot/editor/filesystem_cache10" -Force -ErrorAction SilentlyContinue
Remove-Item ".godot/global_script_class_cache.cfg" -Force -ErrorAction SilentlyContinue
```

### Scene File Format (.tscn)

Script references in `.tscn` files look like:

```gdscript
# Good - path only (portable)
[ext_resource type="Script" path="res://Nodes/MyScript.cs" id="1_id"]

# Avoid - UID can become stale when files move
[ext_resource type="Script" uid="uid://xyz123" path="res://Nodes/MyScript.cs" id="1_id"]
```

When creating new scripts, omit the `uid` attribute from ext_resource lines.

## Work in Progress

The following systems are scaffolded but not fully implemented (marked with `TODO` comments):
- `RulesEngine` - Command validation and execution
- `ActionPreview` - Previewing command effects
- Combat system
- Hero abilities
- Unit recruitment
- Enemy AI
- Scenarios and win conditions
- Day/Night cycle
- Save/Load functionality

## Notes for AI Agents

1. **Preserve Patterns**: Follow existing command/event architecture
2. **Keep Logic Pure**: Game logic in `scripts/` should not depend on Godot APIs
3. **Type Safety**: Use strongly-typed IDs, not raw strings/ints
4. **Determinism**: Any randomness must use `DeterministicRandom`
5. **Content Over Code**: Game data goes in JSON, not hardcoded
6. **Test Coverage**: Add tests for new game logic
7. **XML Docs**: Document all public APIs
