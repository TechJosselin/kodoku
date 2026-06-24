# Graph Report - e:/S_Box_Game/MainProject/kodoku  (2026-06-24)

## Corpus Check
- Large corpus: 83 files · ~667,239 words. Semantic extraction will be expensive (many Claude tokens). Consider running on a subfolder.

## Summary
- 626 nodes · 1244 edges · 55 communities (40 shown, 15 thin omitted)
- Extraction: 96% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 35 edges (avg confidence: 0.91)
- Token cost: 3,885 input · 1,192 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Player Inventory Interaction|Player Inventory Interaction]]
- [[_COMMUNITY_Asset Paths & Resource Loading|Asset Paths & Resource Loading]]
- [[_COMMUNITY_Core Inventory Component|Core Inventory Component]]
- [[_COMMUNITY_Scene Loading & Engine Core|Scene Loading & Engine Core]]
- [[_COMMUNITY_World-Inventory Bridge|World-Inventory Bridge]]
- [[_COMMUNITY_Project Docs & Item Assets|Project Docs & Item Assets]]
- [[_COMMUNITY_Inventory Container & Stacking|Inventory Container & Stacking]]
- [[_COMMUNITY_Game Menu Tests|Game Menu Tests]]
- [[_COMMUNITY_World Item Drop System|World Item Drop System]]
- [[_COMMUNITY_Inventory Page UI|Inventory Page UI]]
- [[_COMMUNITY_Game Menu Navigation|Game Menu Navigation]]
- [[_COMMUNITY_Tab Navigation & Menu State|Tab Navigation & Menu State]]
- [[_COMMUNITY_Debug Menu & Tools|Debug Menu & Tools]]
- [[_COMMUNITY_Equipment Slot Icons|Equipment Slot Icons]]
- [[_COMMUNITY_UI Background Panels|UI Background Panels]]
- [[_COMMUNITY_Test Infrastructure|Test Infrastructure]]
- [[_COMMUNITY_Broken Weather UI Asset|Broken Weather UI Asset]]
- [[_COMMUNITY_World Interaction HUD|World Interaction HUD]]
- [[_COMMUNITY_Razor UI Imports|Razor UI Imports]]
- [[_COMMUNITY_Inventory Menu State|Inventory Menu State]]
- [[_COMMUNITY_Nock Gun Weapon Texture|Nock Gun Weapon Texture]]
- [[_COMMUNITY_Repair Tab Background|Repair Tab Background]]
- [[_COMMUNITY_Shotgun Weapon Icon|Shotgun Weapon Icon]]
- [[_COMMUNITY_Raider Backpack Albedo|Raider Backpack Albedo]]
- [[_COMMUNITY_Small Backpack Item|Small Backpack Item]]
- [[_COMMUNITY_Hotbar UI Strip|Hotbar UI Strip]]
- [[_COMMUNITY_Bandage Medical Icon|Bandage Medical Icon]]
- [[_COMMUNITY_Item Placement Grid|Item Placement Grid]]
- [[_COMMUNITY_Container UI Background|Container UI Background]]
- [[_COMMUNITY_Water Drink Icon|Water Drink Icon]]
- [[_COMMUNITY_Raider Backpack Icon|Raider Backpack Icon]]
- [[_COMMUNITY_Game Menu Header|Game Menu Header]]
- [[_COMMUNITY_Default System Icon|Default System Icon]]
- [[_COMMUNITY_World Interaction Prompt|World Interaction Prompt]]
- [[_COMMUNITY_Debug Item Option|Debug Item Option]]
- [[_COMMUNITY_Charcoal Noise Texture|Charcoal Noise Texture]]
- [[_COMMUNITY_Inventory Action Result|Inventory Action Result]]
- [[_COMMUNITY_Backpack Slot Icon|Backpack Slot Icon]]
- [[_COMMUNITY_Body Armor Slot|Body Armor Slot]]
- [[_COMMUNITY_Pants Slot|Pants Slot]]
- [[_COMMUNITY_Pouch Equipment Slot|Pouch Equipment Slot]]
- [[_COMMUNITY_Body Armor Icon|Body Armor Icon]]
- [[_COMMUNITY_Pants Slot Icon|Pants Slot Icon]]
- [[_COMMUNITY_Pouch Slot Icon|Pouch Slot Icon]]

## God Nodes (most connected - your core abstractions)
1. `InventoryPlayerInteractionComponent` - 59 edges
2. `WorldItemComponent` - 50 edges
3. `ItemInstance` - 46 edges
4. `InventoryComponent` - 39 edges
5. `InventoryEquipmentSlot` - 31 edges
6. `DebugInventoryBootstrapper` - 28 edges
7. `WorldInventoryInteractionBridge` - 24 edges
8. `LootContainerComponent` - 23 edges
9. `InventoryContainer` - 20 edges
10. `WorldInteractionComponent` - 17 edges

## Surprising Connections (you probably didn't know these)
- `IA.md (CLAUDE.md equivalent guidance)` --semantically_similar_to--> `Kodoku Project Overview`  [INFERRED] [semantically similar]
  IA.md → CLAUDE.md
- `Item Definition (.item) Format` --semantically_similar_to--> `ItemDefinition (GameResource)`  [INFERRED] [semantically similar]
  Assets/Data/Items/README.md → CLAUDE.md
- `World Item Component (Editor)` --semantically_similar_to--> `WorldItemComponent`  [INFERRED] [semantically similar]
  Docs/EditorComponentPlacement.md → CLAUDE.md
- `Generic World Interaction Scanner` --semantically_similar_to--> `WorldInteractionComponent`  [INFERRED] [semantically similar]
  Docs/EditorComponentPlacement.md → CLAUDE.md
- `Player Inventory Interaction Bridge Component` --semantically_similar_to--> `WorldInventoryInteractionBridge`  [INFERRED] [semantically similar]
  Docs/EditorComponentPlacement.md → CLAUDE.md

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Core Inventory System Types** — claude_md_inventorycontainer, claude_md_itemdefinition, claude_md_iteminstance, claude_md_inventorycomponent [INFERRED 0.85]
- **Player Controller Scene Hierarchy** — editorcomponentplacement_player_controller, editorcomponentplacement_inventory_go, editorcomponentplacement_bridge, editorcomponentplacement_gamemenuui [EXTRACTED 1.00]
- **Item Asset Mirror Structure (Data/Models/Prefabs/UI)** — readme_item_category_structure, readme_kodokuitemassetpaths, readme_folder_describes_nature [EXTRACTED 1.00]

## Communities (55 total, 15 thin omitted)

### Community 0 - "Player Inventory Interaction"
Cohesion: 0.10
Nodes (4): IEnumerable, InventoryPlayerInteractionComponent, InventoryContainer, InventoryItemPlacement

### Community 1 - "Asset Paths & Resource Loading"
Cohesion: 0.07
Nodes (15): KodokuItemAssetPaths, KodokuUiAssetPaths, GameResource, DebugInventoryBootstrapper, IInventoryDebugActions, int, IInventoryDebugActions, Fail() (+7 more)

### Community 2 - "Core Inventory Component"
Cohesion: 0.12
Nodes (6): Action, Dictionary, InventoryComponent, InventoryEquipmentSlot, LoadoutComponent, LoadoutSlotRegistry

### Community 3 - "Scene Loading & Engine Core"
Cohesion: 0.07
Nodes (13): bool, CameraComponent, Component, SceneLoaderComponent, float, GameMenuComponent, GameObject, WorldInteractionComponent (+5 more)

### Community 4 - "World-Inventory Bridge"
Cohesion: 0.10
Nodes (7): ExecuteInEditor, WorldInventoryInteractionBridge, LootContainerComponent, LootContainerInitialItem, InventoryComponent, Vector2, WorldInteractionPromptAction

### Community 5 - "Project Docs & Item Assets"
Cohesion: 0.07
Nodes (41): Raider Backpack 3D Asset, Raider Backpack License (CC-BY), GameMenuComponent, Glue Layer (Code/Glue/), Graphify Knowledge Graph Workflow, InventoryComponent (s.box Component), InventoryContainer, ItemDefinition (GameResource) (+33 more)

### Community 6 - "Inventory Container & Stacking"
Cohesion: 0.11
Nodes (6): Amount, InventoryContainer, ItemInstance, List, LoadoutSlotConfig, Placement

### Community 7 - "Game Menu Tests"
Cohesion: 0.11
Nodes (6): GameMenuStateTests, InventoryActionResultTests, InventoryContainerTests, ItemDefinitionTests, LoadoutSlotRegistryTests, TestMethod

### Community 8 - "World Item Drop System"
Cohesion: 0.12
Nodes (8): BBox, BoxCollider, Color, WorldItemComponent, Model, Scene, Transform, Vector3

### Community 9 - "Inventory Page UI"
Cohesion: 0.08
Nodes (24): Kodoku.Lib.Items, Kodoku.Lib.Loadout, InventoryContainer, InventoryItemPlacement, Kodoku.Lib.GameMenu, Kodoku.Lib.Inventory, Sandbox, Sandbox.UI (+16 more)

### Community 10 - "Game Menu Navigation"
Cohesion: 0.08
Nodes (23): GameMenuHeader, GameMenuSidebar, InventoryPage, Kodoku.Lib.UI.Components, Kodoku.Lib.UI.Pages, InventoryComponent, InventoryPlayerInteractionComponent, Kodoku.Lib.GameMenu (+15 more)

### Community 11 - "Tab Navigation & Menu State"
Cohesion: 0.13
Nodes (9): GetTabClass, GetTabIndex, SelectTab, GameMenuState, NavigationMenuState, GameMenuTab, Kodoku.Lib.GameMenu, System (+1 more)

### Community 12 - "Debug Menu & Tools"
Cohesion: 0.13
Nodes (14): IInventoryDebugActions, InventoryBootstrapper, InventoryDebugItemOption, InventoryPlayerInteractionComponent, Kodoku.Lib.Inventory, PanelComponent, Sandbox, Sandbox.UI (+6 more)

### Community 13 - "Equipment Slot Icons"
Cohesion: 0.19
Nodes (14): AK-47 Style Assault Rifle, Equipment Slot: Footwear, Headwear Equipment Slot, Gas Mask Equipment, Medieval Knight Helmet Visual Design, Inventory Page Icons, Face Equipment Slot, Inventory Weapon Slot (+6 more)

### Community 14 - "UI Background Panels"
Cohesion: 0.33
Nodes (9): Equipment Panel Background, Equipment Slots Grid, Game Menu, Inventory Background Panel, Inventory Page, Inventory Panel Border Frame, Corner Rivet/Bolt Details, Dark/Worn Metal Visual Theme (+1 more)

### Community 15 - "Test Infrastructure"
Cohesion: 0.25
Nodes (5): AssemblyCleanup, AssemblyInitialize, TestAppSystem, TestContext, TestInit

### Community 16 - "Broken Weather UI Asset"
Cohesion: 0.33
Nodes (6): Cracked Glass / Broken Surface Motif, Faint Tower / Lighthouse Silhouette Motif, Wide Horizontal Banner Shape, Inventory Page Background Panel, Dark Cracked Panel Style, InventoryPage/Background Folder

### Community 17 - "World Interaction HUD"
Cohesion: 0.29
Nodes (6): Kodoku.Lib.Interaction, PanelComponent, WorldInteractionPromptAction, BuildHash, HidePrompt, ShowPrompt

### Community 18 - "Razor UI Imports"
Cohesion: 0.29
Nodes (6): Sandbox, Sandbox.UI, System, System.Collections.Generic, System.Linq, Panel

### Community 20 - "Nock Gun Weapon Texture"
Cohesion: 0.33
Nodes (6): Brass and Dark Steel Metal Fittings, Dark Walnut Wood Stock Material, Color Palette: dark brown, brass gold, charcoal grey, Historical Flintlock Style, nockgun_tex (Texture), Nock Gun (Ranged Weapon)

### Community 21 - "Repair Tab Background"
Cohesion: 0.60
Nodes (5): Dark Industrial UI Visual Style, InventoryPage Background Asset Group, Repair Tab (Inventory), Repair Weather Background Panel, Watchtower/Lighthouse Etched Motif

### Community 22 - "Shotgun Weapon Icon"
Cohesion: 0.50
Nodes (5): Double Barrel Shotgun, Ranged Weapon, Shotgun Icon, Shotgun, Vintage Firearm Style

### Community 23 - "Raider Backpack Albedo"
Cohesion: 0.50
Nodes (5): Raider Backpack (Equipment Item), Tan/Khaki and Dark Brown Color Palette, Tactical Military Style, Raider Backpack Albedo Texture, UV Unwrap Layout

### Community 24 - "Small Backpack Item"
Cohesion: 0.67
Nodes (4): Equipment Category: Backpacks, Icon: Small Backpack, Small Backpack (Equipment Item), UI Icon Style: Flat/Minimal

### Community 25 - "Hotbar UI Strip"
Cohesion: 0.50
Nodes (4): Hotbar Background, Hotbar Extra Panel (right side), Hotbar Logo/Branding Area, Hotbar Slot

### Community 26 - "Bandage Medical Icon"
Cohesion: 0.50
Nodes (4): Icon: Bandage (Medical Consumable), Medical Consumable Category, Pixel Art Icon Style, Red Cross Visual Motif

### Community 28 - "Container UI Background"
Cohesion: 1.00
Nodes (3): Inventory Page Container Background, GameMenu InventoryPage UI Section, InventoryPage Background Folder

### Community 29 - "Water Drink Icon"
Cohesion: 0.67
Nodes (3): Consumable Drink Category, Icon: Water (Drink Consumable), Pixel Art Icon Style

### Community 30 - "Raider Backpack Icon"
Cohesion: 0.67
Nodes (3): Equipment Backpacks Category, Raider Backpack, Raider Backpack Icon

## Knowledge Gaps
- **135 isolated node(s):** `WorldInteractionPromptAction`, `InventoryDebugItemOption`, `LootContainerInitialItem`, `Kodoku.Lib.GameMenu`, `System` (+130 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **15 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `WorldItemComponent` connect `World Item Drop System` to `Player Inventory Interaction`, `Asset Paths & Resource Loading`, `Core Inventory Component`, `Scene Loading & Engine Core`, `World-Inventory Bridge`?**
  _High betweenness centrality (0.071) - this node is a cross-community bridge._
- **Why does `InventoryEquipmentSlot` connect `Core Inventory Component` to `Player Inventory Interaction`, `Asset Paths & Resource Loading`, `Inventory Page UI`?**
  _High betweenness centrality (0.061) - this node is a cross-community bridge._
- **Why does `ItemInstance` connect `Inventory Container & Stacking` to `Player Inventory Interaction`, `Asset Paths & Resource Loading`, `Core Inventory Component`, `World-Inventory Bridge`, `World Item Drop System`?**
  _High betweenness centrality (0.052) - this node is a cross-community bridge._
- **What connects `WorldInteractionPromptAction`, `InventoryDebugItemOption`, `LootContainerInitialItem` to the rest of the system?**
  _136 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Player Inventory Interaction` be split into smaller, more focused modules?**
  _Cohesion score 0.1016949152542373 - nodes in this community are weakly interconnected._
- **Should `Asset Paths & Resource Loading` be split into smaller, more focused modules?**
  _Cohesion score 0.07188778492109878 - nodes in this community are weakly interconnected._
- **Should `Core Inventory Component` be split into smaller, more focused modules?**
  _Cohesion score 0.11607843137254902 - nodes in this community are weakly interconnected._