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
