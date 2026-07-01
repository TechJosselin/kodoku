# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).

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
- `ItemDefinition` — `GameResource` asset. Fields: `ItemId`, `DisplayName`, `Description`, `IconPath`, `ModelPath`, `PrefabPath`, `ItemKind` (`InventoryItemKind` enum), `Width`, `Height`, `CanRotate`, `IsStackable`, `MaxStack`, `Weight`, `StorageWidth` (capped at `ItemDefinition.MaxStorageWidth` = 6), `StorageHeight`. **No `EquipSlot` or `WeaponKind` fields.** `CreatesContainer` is a computed property (`StorageWidth > 0 && StorageHeight > 0`). Consumable fields: `IsUsable`, `ConsumeOnUse`, `UseQuantity`, and per-vital deltas (`UseHealthDelta`, `UseStaminaDelta`, `UseHungerDelta`, `UseThirstDelta`, `UseMadnessDelta`) applied via `PlayerVitalsComponent.ApplyItemUseEffects()`; `HasUseEffects()` checks whether any delta is non-zero.
- `ItemInstance` — runtime item with a unique `InstanceId`, stack count, and reference to its `ItemDefinition`.
- `InventoryContainer` — grid storage with stacking and split support. Tagged with an `InventoryContainerKind`: `Pockets` (player's own inventory), `Loot` (`LootContainerComponent` world containers), or `Backpack` (item-owned sub-containers, created lazily via `ItemInstance.EnsureStoredContainer()` when `CreatesContainer == true`).
- `InventoryComponent` — s.box Component that owns one `pockets` container, delegates to `LoadoutComponent` for equipped items, and resolves sibling `HotbarComponent`/`PlayerVitalsComponent` references in `EnsureInitialized()`. `TryUseItem(itemId)` validates `IsUsable`/`HasUseEffects()`, applies vitals deltas, and consumes the item (via `ConsumeOwnedItemQuantity`) when `ConsumeOnUse` is set.
- `LoadoutComponent` / `LoadoutSlotRegistry` — manages equipment slots (headwear, armor, backpack, sling weapon, back weapon, etc.).
- `HotbarComponent` — 8 fixed slots (`SlotCount`) mapping number keys to owned item IDs. `UseSlot(index)` calls `InventoryComponent.TryUseItem()` for usable items, or arms `Weapon`-kind items (sets `ActiveSlotIndex`/`ActiveItemId`) if equipped; otherwise fails.
- `PlayerVitalsComponent` — owns five `VitalStat`s (Health, Stamina, Hunger, Thirst, Madness). `ApplyItemUseEffects()` adds deltas from item use. `OverrideWithDebugValues` (off by default) re-applies the `Debug*` slider values every frame — leave it off so item effects persist; only `ApplyDebugValues()`-on-`OnStart` runs once otherwise.
- `WorldItemComponent` — represents a pickable item in the world. Automatically creates and fits a `BoxCollider` to the model bounds (retry loop up to 10 frames). Do not add a collider manually unless `OverrideExistingCollider` is set.
- `LootContainerComponent` — world container that players can loot from. Owns an `InventoryContainer` of kind `InventoryContainerKind.Loot`. Configured via an `InitialItems` list seeded on `OnStart`; shows a debug-sphere visual in the editor. Do not create the `InventoryContainer` manually — `EnsureSetup()` manages it internally.

### Action Result Pattern

All inventory mutations return `InventoryActionResult` (immutable, `Success` + `Reason`). Always check `.Success` before using the result. `InventoryComponent` has a `LogActionResults` property that logs every result to `Sandbox.Log`.

### Interaction Pipeline

`WorldInteractionComponent` does a raycast every frame and exposes the current target. `WorldInventoryInteractionBridge` subscribes to that, builds a list of `IWorldInteractionAction`s, and dispatches the selected one on input. Mouse wheel cycles actions; `E` executes.

### UI

UI is Razor Components (`.razor` files). `GameMenuUI.razor` is the real central controller: it owns its own `GameMenuState` instance (open/closed + active `GameMenuTab`) and renders the active page. **`GameMenuComponent.cs` (and `GameMenuSidebar.razor`/`GameMenuHeader.razor`) are dead code** — never instantiated or referenced anywhere; do not treat them as the source of truth. The menu auto-resolves `InventoryComponent` from the scene using `Scene.GetAllComponents<InventoryComponent>()`.

`GameMenuUI` handles all its own keyboard shortcuts in `OnUpdate()` (no separate input manager): `ToggleKey` ("TAB") toggles Inventory, `MapKey` ("M") toggles Map, `StatsKey` ("I") toggles Stats — all via the same `MenuState.Toggle(tab)` call. Escape is handled separately via `Input.EscapePressed`: if an s&box overlay is already open (checked through `IsSandboxOverlayOpen()`, a workaround since `Game.Overlay.IsOpen`/`IsPauseMenuOpen` are — unusually — instance properties while every `Show*`/`Close` method on `Game.Overlay` is static) Escape is left untouched; otherwise it's consumed (`Input.EscapePressed = false`) and toggles the menu open on the Options tab. `IsBlockingUiActive()` guards the Map/Stats shortcuts against firing while an overlay or the debug menu is open.

`OptionsPage.razor` has an s&box System section with buttons wrapping `Game.Overlay.ShowPauseMenu()` and `Game.Overlay.ShowSettingsModal("keybinds"|"audio"|"video")`.

`WorldInteractionHud.razor` renders the context-sensitive interaction prompt (action list + selected index) based on `WorldInteractionComponent` state.

`DebugMenuUI.razor` is a standalone debug panel (comma key toggle, works independently of inventory). It provides per-item **Give** (add to inventory) and **Spawn** (drop in world) actions with a configurable quantity (1–99). It resolves `IInventoryDebugActions` from the scene at runtime. `GameMenuUI` reads `DebugMenuUI.IsOpen` in its own `RootClass` getter (and `BuildHash`) to apply `debug-overlay-active`, which disables pointer events on the inventory panel while the debug panel is open.

`GameHudUI.razor` composes `HotbarHud` and `VitalsHud`, hiding itself (`game-hud-hidden`) whenever `GameMenuUI.IsOpen`. It resolves `InventoryComponent` (by `GameObject.Name == "Inventory"`) and then `PlayerVitalsComponent` off that same GameObject, falling back to a scene-wide lookup for either.

### s&box UI — Pointer Events Between PanelComponents

`z-index` affects visual stacking only; inter-PanelComponent hit-testing follows **panel tree order** (later sibling wins). Two important rules:

1. **Do not call `Panel.SetClass()` from an external component.** The owning component's next Razor re-render (`StateHasChanged`) replaces the full class list on its root, discarding any externally-set class. Cross-component state must be read inside the owning component's own `RootClass` getter and tracked in `BuildHash()`.

2. **`pointer-events: none` on a root does not propagate if descendants declare `pointer-events: all`.** Interactive children (inventory slots, drag targets) set this explicitly. To block an entire component subtree, use:
   ```scss
   &.blocked-state,
   &.blocked-state * { pointer-events: none !important; }
   ```

## Asset Conventions

Asset paths in s&box are **case-insensitive** and relative to `Assets/`. Canonical folder layout:

| Folder | Content |
|---|---|
| `Assets/Data/Items/` | `ItemDefinition` assets (`.item` files), organised by category |
| `Assets/Models/Items/` | 3D models for items (`.vmdl`) — mirrors Data/Items structure |
| `Assets/Prefabs/Items/` | s&box prefabs for items — mirrors Data/Items structure |
| `Assets/UI/Game/Icons/Items/` | Per-item icons — mirrors Data/Items structure |
| `Assets/UI/Game/Icons/System/` | System icons: `Default/Icon_default.png` (fallback), `Placeholders/` |
| `Assets/UI/GameMenu/InventoryPage/Background/` | Inventory UI background textures |
| `Assets/UI/GameMenu/InventoryPage/Icons/` | Loadout slot placeholder images (`slot_headwear.png`, etc.) |
| `Assets/Scenes/` | Scene files (`.scene`). Do not move — `.scene_c`/`.scene_d` are compiled by s&box |
| `Assets/Materials/Containers/`, `Assets/Models/Containers/`, `Assets/Prefabs/Containers/`, `Assets/Textures/Containers/` | Assets for `LootContainerComponent` world objects (e.g. wardrobes), organised by container type |

**Item category structure** (identical across Data, Models, Prefabs, UI/Game/Icons/Items):
`Equipment/{Armor,Backpacks,Clothing,Weapons}` · `Consumables/{Drinks,Food,Medical}` · `Tools` · `Keys` · `Quest` · `Resources` · `Debug`

The folder describes **what the object is**, not its equipment slot:
- Body armor → `Equipment/Armor/Chest/` · Backpack → `Equipment/Backpacks/` · Pistol → `Equipment/Weapons/Sidearms/`
- A crowbar goes in `Tools/` even if it can deal damage

Centralised path constants live in `KodokuLib`:
- `KodokuItemAssetPaths` — one constant per item definition (e.g. `KodokuItemAssetPaths.Bandage`)
- `KodokuUiAssetPaths` — one constant per loadout slot icon (e.g. `KodokuUiAssetPaths.SlotHeadwear`)
- `ItemDefinition.DefaultIconPath` — fallback icon (`ui/game/icons/system/default/icon_default.png`)

Always update these constants when moving asset files; do not hardcode paths outside `AssetPaths/`.
See `Assets/Data/Items/README.md` for the full category reference and path examples.

## s.box Component Conventions

- Components derive from `Component` and use `[Property]` for serialized fields.
- Use `[Title]`, `[Category("Kodoku/...")]`, and `[Icon(...)]` attributes for editor visibility.
- Call `Components.Get<T>()` or `Scene.GetAllComponents<T>()` to resolve sibling/scene dependencies at runtime (in `OnStart`/`EnsureInitialized`, not the constructor).
- The `GameObject` named `"Inventory"` is used by convention for auto-discovery in some UI paths — do not rename it.

## Namespaces

| Namespace | Content |
|---|---|
| `Kodoku.Lib.Inventory` | `InventoryContainer`, `InventoryComponent`, `HotbarComponent`, `LootContainerComponent`, `WorldItemComponent` |
| `Kodoku.Lib.Items` | `ItemDefinition`, `ItemInstance`, `InventoryItemPlacement` |
| `Kodoku.Lib.Loadout` | `LoadoutComponent`, `LoadoutSlotRegistry`, `InventoryEquipmentSlot` |
| `Kodoku.Lib.Interaction` | `WorldInteractionComponent`, interaction actions |
| `Kodoku.Lib.Vitals` | `PlayerVitalsComponent`, `VitalStat`, `VitalStatKind` |
| `Kodoku.Lib.UI` | All Razor UI components (`GameHud/`, `GameMenu/`, `Debug/`) |
| `Kodoku.Lib.GameMenu` | `GameMenuComponent`, `GameMenuState`, `GameMenuTab` |
| `Sandbox` (root) | Game-specific glue in `Code/` |
