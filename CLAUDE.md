# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Kodoku is a grid-based inventory system game built on **s.box** (Facepunch's Source 2 engine), using C# 14 / .NET 10.0 and Razor Components for UI. The project is split into:

- `Code/` — game-specific integration and glue code (references KodokuLib)
- `Libraries/KodokuLib/Code/` — the reusable inventory/interaction library (zero game dependencies)
- `Libraries/KodokuLib/UnitTests/` — MSTest unit tests for pure logic in KodokuLib

## Build & Run

Building and running happens through the **s.box editor** (Steam). The `.slnx` solution can be opened in Visual Studio or Rider for IDE support, but compilation and hot-reload are driven by the engine.

Unit tests can be run from the CLI:

```
dotnet build kodoku.slnx
dotnet test Libraries/KodokuLib/UnitTests/
```

Tests are MSTest (`[TestClass]`, `[TestMethod]`, `Assert.*`). The output path is routed into the s.box `.vs/output/` directory. Visual Studio is also supported.

The `Docs/EditorComponentPlacement.md` file is the authoritative guide for wiring up components in the s.box scene editor (it is in French).

## Architecture

### Two-Layer Structure

**KodokuLib** (`Libraries/KodokuLib/Code/`) is a standalone library. It must never import anything from the main `Code/` project. The main project's `Code/Glue/` layer bridges between the two via concrete component implementations.

**Glue components** (e.g. `WorldInventoryInteractionBridge`, `DebugInventoryBootstrapper`) are the only place where game-specific logic (camera, player input, scene queries) touches KodokuLib abstractions.

### Grid Inventory Model

`InventoryContainer` (pure C# class, not a Component) holds a flat list of `InventoryItemPlacement` records. Each placement stores an `ItemInstance`, an (x, y) origin, and a rotation flag. Items occupy multiple cells based on their `ItemDefinition.Width`/`Height`. Call `CanAddItemAt()` before `TryAddItem()` — placement is never forced.

Key types:
- `ItemDefinition` — `GameResource` asset defining an item's properties, size, and equipment slot.
- `ItemInstance` — runtime item with a unique `InstanceId`, stack count, and reference to its `ItemDefinition`.
- `InventoryContainer` — grid storage with stacking and split support.
- `InventoryComponent` — s.box Component that owns one `pockets` container and delegates to `LoadoutComponent` for equipped items.
- `LoadoutComponent` / `LoadoutSlotRegistry` — manages equipment slots (headwear, armor, backpack, sling weapon, back weapon, etc.).
- `WorldItemComponent` — represents a pickable item in the world. Automatically creates and fits a `BoxCollider` to the model bounds (retry loop up to 10 frames). Do not add a collider manually unless `OverrideExistingCollider` is set.

### Action Result Pattern

All inventory mutations return `InventoryActionResult` (immutable, `Success` + `Reason`). Always check `.Success` before using the result. `InventoryComponent` has a `LogActionResults` property that logs every result to `Sandbox.Log`.

### Interaction Pipeline

`WorldInteractionComponent` does a raycast every frame and exposes the current target. `WorldInventoryInteractionBridge` subscribes to that, builds a list of `IWorldInteractionAction`s, and dispatches the selected one on input. Mouse wheel cycles actions; `E` executes.

### UI

UI is Razor Components (`.razor` files). `GameMenuComponent` manages open/close state and the active `GameMenuTab` (Inventory, Stats, Quests, Map, Options). `GameMenuUI.razor` renders the active page. Tab-key toggles the menu. The menu auto-resolves `InventoryComponent` from the scene using `Scene.GetAllComponents<InventoryComponent>()`.

`WorldInteractionHud.razor` renders the context-sensitive interaction prompt (action list + selected index) based on `WorldInteractionComponent` state.

## Asset Conventions

Asset paths in s&box are **case-insensitive** and relative to `Assets/`. Canonical folder layout:

| Folder | Content |
|---|---|
| `Assets/Data/Items/` | `ItemDefinition` assets (`.item` files) |
| `Assets/UI/Icons/` | Per-item icons, organised by sub-category (`Default/`, `Medical/`, `Drinks/`, `Equipment/`) |
| `Assets/UI/equipment/` | Loadout slot placeholder images (`slot_headwear.png`, etc.) |
| `Assets/UI/Inventory/` | General inventory UI images |
| `Assets/scenes/` | Scene files (`.scene`). Do not move — `.scene_c`/`.scene_d` files are compiled by s&box |

Centralised path constants live in `KodokuLib`:
- `KodokuItemAssetPaths` — one constant per item definition (e.g. `KodokuItemAssetPaths.Bandage`)
- `KodokuUiAssetPaths` — one constant per loadout slot icon (e.g. `KodokuUiAssetPaths.SlotHeadwear`)
- `ItemDefinition.DefaultIconPath` — fallback icon path used when an item has no specific icon

Always update these constants when moving asset files; do not hardcode paths outside `AssetPaths/`.

## s.box Component Conventions

- Components derive from `Component` and use `[Property]` for serialized fields.
- Use `[Title]`, `[Category("Kodoku/...")]`, and `[Icon(...)]` attributes for editor visibility.
- Call `Components.Get<T>()` or `Scene.GetAllComponents<T>()` to resolve sibling/scene dependencies at runtime (in `OnStart`/`EnsureInitialized`, not the constructor).
- The `GameObject` named `"Inventory"` is used by convention for auto-discovery in some UI paths — do not rename it.

## Namespaces

| Namespace | Content |
|---|---|
| `Kodoku.Lib.Inventory` | `InventoryContainer`, `InventoryComponent`, `LootContainerComponent`, `WorldItemComponent` |
| `Kodoku.Lib.Items` | `ItemDefinition`, `ItemInstance`, `InventoryItemPlacement` |
| `Kodoku.Lib.Loadout` | `LoadoutComponent`, `LoadoutSlotRegistry`, `InventoryEquipmentSlot` |
| `Kodoku.Lib.Interaction` | `WorldInteractionComponent`, interaction actions |
| `Kodoku.Lib.UI` | All Razor UI components |
| `Kodoku.Lib.GameMenu` | `GameMenuComponent`, `GameMenuState`, `GameMenuTab` |
| `Sandbox` (root) | Game-specific glue in `Code/` |
