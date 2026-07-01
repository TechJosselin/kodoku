# Assets/Data/Items

Item definition assets (`.item` files) for the Kodoku project.

## Rôle de chaque dossier racine

| Dossier | Contenu |
|---------|---------|
| `Assets/Data/Items/` | Fichiers `.item` — définitions statiques des items (stats, taille grille, slot d'équip) |
| `Assets/Models/Items/` | Modèles 3D des items (`.vmdl`) |
| `Assets/Prefabs/Items/` | Prefabs s&box des items (`.prefab`) |
| `Assets/UI/Game/Icons/Items/` | Icônes d'inventaire des items (`.png`) |
| `Assets/UI/Game/Icons/System/` | Icônes système (fallback default, placeholders) |

La structure de sous-dossiers est **identique dans les quatre arbres** — chaque item partage le même chemin relatif dans Data, Models, Prefabs et UI/Game/Icons/Items.

## Structure de catégories

```
Items/
├── Consumables/
│   ├── Drinks/
│   ├── Food/
│   └── Medical/
├── Equipment/
│   ├── Armor/
│   │   ├── Chest/
│   │   ├── Head/
│   │   └── Legs/
│   ├── Backpacks/
│   ├── Clothing/
│   │   ├── Chest/
│   │   ├── Feet/
│   │   ├── Head/
│   │   ├── Legs/
│   │   └── Mask/
│   └── Weapons/
│       ├── Melee/
│       ├── Ranged/
│       └── Sidearms/
├── Keys/
├── Quest/
├── Resources/
├── Tools/
└── Debug/
```

## Règle fondamentale : le dossier décrit la nature de l'objet

Le dossier dans lequel un item est classé décrit **ce qu'est** l'objet, pas ce qu'il peut faire ou quel slot il occupe.

- Un sac à dos → `Equipment/Backpacks/` même s'il occupe le slot `Backpack`
- Une veste → `Equipment/Clothing/Chest/` même si elle offre une protection
- Une armure corporelle → `Equipment/Armor/Chest/` parce que c'est de l'armure protectrice
- Un pistolet → `Equipment/Weapons/Sidearms/`
- Un pied-de-biche → `Tools/` même s'il peut avoir un profil d'arme plus tard

Le fichier `.item` décrit **ce que l'objet fait** : `ItemKind`, slot d'équipement, dimensions grille, stackability.

## Chemins canoniques

Les chemins sont définis dans `KodokuItemAssetPaths` (pour les items) et `KodokuUiAssetPaths` (pour les icônes de slots).

Les chemins s&box sont **insensibles à la casse** et relatifs à `Assets/` :
- `data/items/consumables/medical/bandage.item`
- `ui/game/icons/items/consumables/medical/icon_bandage.png`

Ne jamais hardcoder ces chemins en dehors des fichiers `AssetPaths/`.

## Exemples de chemins corrects

| Item | Fichier `.item` | Icône |
|------|-----------------|-------|
| Bandage | `consumables/medical/bandage.item` | `ui/game/icons/items/consumables/medical/icon_bandage.png` |
| Small Backpack | `equipment/backpacks/small_backpack.item` | `ui/game/icons/items/equipment/backpacks/icon_small_backpack.png` |
| Pistol | `equipment/weapons/sidearms/pistol.item` | _(utilise le fallback system/default)_ |
| Machete | `equipment/weapons/melee/machete.item` | _(utilise le fallback system/default)_ |

## Canonical .item format

An `.item` file is a JSON-backed `ItemDefinition` resource. Paths are relative to
`Assets/`, use forward slashes (`/`), and should use canonical lower-case asset
paths even though s&box resolves them case-insensitively.

### Common fields

- `ItemId`: stable identifier; do not change it when moving an asset.
- `DisplayName`: player-facing name.
- `Description`: optional player-facing details; use an empty string when absent.
- `IconPath`: inventory icon; an empty string uses
  `ItemDefinition.DefaultIconPath`.
- `ModelPath`: world model; use an empty string when no model exists.
- `PrefabPath`: world-item prefab; use an empty string when no prefab exists.
- `ItemKind`: one of the values in `InventoryItemKind`.
- `Width`, `Height`, `CanRotate`: inventory-grid footprint.
- `IsStackable`, `MaxStack`: stack rules. Non-stackable items use `MaxStack: 1`;
  stackable items use a value greater than 1.
- `Weight`: item weight. The code default is `0.1` when omitted.

Every current item explicitly includes these common fields. Their code defaults
remain safe so older resources with missing metadata still deserialize.

### Storage fields

- `StorageWidth`, `StorageHeight`: include both with positive values when the
  item creates storage. In the current model this includes backpacks and also
  equipment such as tactical rigs or cargo pants. Omit both for
  items that do not create storage. `StorageWidth` is capped at 6
  (`ItemDefinition.MaxStorageWidth`).

### Kind, slot, and folder

- The folder describes what the object is, such as `Weapons/Ranged`.
- `ItemKind` controls behavior and compatible loadout slots.
- There is no serialized `EquipSlot`: `LoadoutSlotRegistry` maps an `ItemKind`
  to accepted slots. For example, `Weapon` fits `OnSling` and `OnBack`.
- There is no serialized `WeaponKind` yet. Ranged, melee, and sidearm are folder
  categories only.
- The current enum uses `Simple`, not `Misc`, and has no `Container` value.

Safe code defaults are empty metadata paths and description,
`DisplayName: "Item"`, `ItemKind: "Simple"`, a `1x1` footprint, no rotation,
no stacking, `MaxStack: 1`, `Weight: 0.1`, and no storage.

### Simple item example

```json
{
  "ItemId": "bandage",
  "DisplayName": "Bandage",
  "Description": "",
  "IconPath": "ui/game/icons/items/consumables/medical/icon_bandage.png",
  "ModelPath": "",
  "PrefabPath": "",
  "ItemKind": "Simple",
  "Width": 1,
  "Height": 1,
  "CanRotate": false,
  "IsStackable": true,
  "MaxStack": 4,
  "Weight": 0.1
}
```

### Backpack example

```json
{
  "ItemId": "raider_backpack",
  "DisplayName": "Raider Backpack",
  "Description": "",
  "IconPath": "ui/game/icons/items/equipment/backpacks/raider_backpack.png",
  "ModelPath": "models/items/equipment/backpacks/raider_backpack.vmdl",
  "PrefabPath": "prefabs/items/equipment/backpacks/raider_backpack.prefab",
  "ItemKind": "Backpack",
  "Width": 2,
  "Height": 2,
  "CanRotate": false,
  "IsStackable": false,
  "MaxStack": 1,
  "Weight": 1.0,
  "StorageWidth": 6,
  "StorageHeight": 16
}
```

### Weapon example

```json
{
  "ItemId": "shotgun",
  "DisplayName": "Shotgun",
  "Description": "",
  "IconPath": "ui/game/icons/items/equipment/weapons/ranged/shotgun.png",
  "ModelPath": "models/items/equipment/weapons/ranged/shotgun.vmdl",
  "PrefabPath": "",
  "ItemKind": "Weapon",
  "Width": 5,
  "Height": 2,
  "CanRotate": true,
  "IsStackable": false,
  "MaxStack": 1,
  "Weight": 1.0
}
```

### New item checklist

- [ ] Create `Assets/Data/Items/<category>/<item_id>.item` with all common fields
- [ ] Set `ItemId` to a stable snake_case identifier — never change it after release
- [ ] Set `ItemKind` to the correct enum value (`Simple`, `Backpack`, `Headwear`, `GasMask`, `BodyArmor`, `TacticalRig`, `Weapon`, `Special`, `Pants`, `Footwear`)
- [ ] If the item has an icon, place it at `Assets/UI/Game/Icons/Items/<category>/<name>.png` and set `IconPath`; otherwise leave `IconPath: ""`
- [ ] If the item has a 3D model, place it at `Assets/Models/Items/<category>/<name>.vmdl` and set `ModelPath`; otherwise leave `ModelPath: ""`
- [ ] If the item has a world prefab, place it at `Assets/Prefabs/Items/<category>/<name>.prefab` and set `PrefabPath`; otherwise leave `PrefabPath: ""`
- [ ] All paths use `/` (never `\`) and are relative to `Assets/`
- [ ] If `IsStackable: true`, set `MaxStack` to a value > 1
- [ ] If `IsStackable: false`, set `MaxStack: 1`
- [ ] For backpacks and containers, set both `StorageWidth` and `StorageHeight` to positive values (`StorageWidth` ≤ 6)
- [ ] For non-container items, omit `StorageWidth`/`StorageHeight` or leave them at 0
- [ ] Register the path constant in `KodokuItemAssetPaths.cs`
- [ ] If the item should appear in the debug menu, register it in `DebugInventoryBootstrapper`
