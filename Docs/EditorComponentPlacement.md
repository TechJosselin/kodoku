# Kodoku — Editor Component Placement

## Principe général

La scène Kodoku est organisée pour séparer les responsabilités par GameObject :

- **Player Controller** porte les composants globaux du joueur (bridge d'interaction, caméra).
- Les enfants **Body**, **CameraRoot**, **InteractionOrigin**, **Inventory** et **Audio** isolent chaque préoccupation sur son propre nœud.
- **Inventory** est le GameObject qui porte les composants d'inventaire du joueur (InventoryComponent, LoadoutComponent, PlayerInteractor).
- **UI/GameMenuUI** porte le menu principal (PanelComponent Razor).
- Les objets du monde portent **World Item** ou **Loot Container** selon leur type.
- Les composants de **Kodoku.Glue** (`Code/Glue/`) sont spécifiques au projet. Les systèmes réutilisables restent dans **Kodoku.Lib** (`Libraries/KodokuLib/`).

---

## Placement recommandé

### Player Controller

| Component | Obligatoire ? | Rôle | Notes |
|-----------|:---:|-------|-------|
| **Player Inventory Interaction Bridge** | Oui | Connecte raycast, items monde, loot containers, inventaire et UI en un seul composant. | Remplir `ViewCamera` avec le `CameraComponent` de `CameraRoot` si `Scene.Camera` ne suffit pas. Activer `LogRaycastDebug` / `LogResults` pour diagnostiquer. |

---

### Player Controller / Inventory

> Ce GameObject **doit s'appeler `Inventory`** : `GameMenuUI` cherche en priorité un `InventoryComponent` sur un objet portant ce nom.

| Component | Obligatoire ? | Rôle | Notes |
|-----------|:---:|-------|-------|
| **Player Inventory Component** | Oui | Stockage principal (pockets, conteneurs ouverts). | Source de vérité de l'inventaire joueur. |
| **Player Loadout Component** | Oui | Gestion des slots d'équipement (headwear, armor, armes…). | Dépend de `Player Inventory Component`. |
| **Player Inventory Interactor** | Oui | Actions joueur : ramassage, drop, équipement, ouverture loot. | Rester sur ce GameObject, pas sur Player Controller. |
| **Debug Inventory Bootstrapper** | Non | Ajoute des items de test et expose les contrôles debug dans l'UI. | Assigner `Inventory` à `Player Inventory Component`. Retirer en production. |

---

### Player Controller / CameraRoot

| Component | Obligatoire ? | Rôle | Notes |
|-----------|:---:|-------|-------|
| **CameraComponent** | Oui | Caméra utilisée par le bridge pour le raycast et la position écran des prompts. | Remplir `ViewCamera` sur le bridge explicitement pour le premier test. |

---

### Player Controller / InteractionOrigin

| Component | Obligatoire ? | Rôle | Notes |
|-----------|:---:|-------|-------|
| *(aucun pour l'instant)* | — | Point de référence futur pour les interactions ou raycasts alternatifs. | Peut rester vide si le bridge utilise directement la caméra. |

---

### UI / GameMenuUI

| Component | Obligatoire ? | Rôle | Notes |
|-----------|:---:|-------|-------|
| **Game Menu UI** | Oui | Affiche le menu principal : Inventory, Stats, Map, Quests, Options. | `ToggleKey` : `TAB` par défaut. Auto-résout `InventoryComponent`, `InventoryPlayerInteractionComponent` et `IInventoryDebugActions` depuis la scène — aucun câblage manuel nécessaire. |

---

### UI / InteractionPrompt

| Component | Obligatoire ? | Rôle | Notes |
|-----------|:---:|-------|-------|
| *(aucun dans le test minimal)* | — | GameObject réservé pour un prompt d'interaction manuel si besoin. | **World Interaction Prompt UI** est créé **automatiquement** par le bridge. Ne pas le placer manuellement pour éviter deux prompts superposés. |

---

### _LoadedWorld / WorldItem_*

| Component | Obligatoire ? | Rôle | Notes |
|-----------|:---:|-------|-------|
| **World Item** | Oui | Item ramassable dans le monde. | Assigner une `ItemDefinition`. |
| **Collider** | Oui | Permet au raycast de détecter l'objet. | Sans Collider, le HUD n'apparaît jamais. |

---

### _LoadedWorld / LootContainer_*

| Component | Obligatoire ? | Rôle | Notes |
|-----------|:---:|-------|-------|
| **Loot Container** | Oui | Conteneur ouvrable dans le monde. | Remplir `ContainerName`, `StorageWidth` et `StorageHeight`. |
| **Collider** | Oui | Permet au raycast de détecter l'objet. | Sans Collider, le prompt Open n'apparaît jamais. |

---

## Catégories de composants dans l'éditeur s&box

### Kodoku/Inventory
- Player Inventory Component
- Player Loadout Component
- Player Inventory Interactor
- Inventory Bootstrapper Base
- World Item
- Loot Container

### Kodoku/Interaction
- Generic World Interaction Scanner
- Player Inventory Interaction Bridge

### Kodoku/UI
- Game Menu State Component
- Game Menu UI
- World Interaction Prompt UI

### Kodoku/Core
- Scene Loader
- World Root

### Kodoku/Debug
- Debug Inventory Bootstrapper

---

## Composants à ne pas confondre

| Composant | Ce que c'est | Ce que ce n'est pas |
|-----------|-------------|---------------------|
| **Generic World Interaction Scanner** | Détecteur raycast générique bas niveau, réutilisable hors inventaire. | Le composant principal pour connecter l'inventaire joueur — utiliser le bridge à la place. |
| **Player Inventory Interaction Bridge** | Le composant central à placer sur **Player Controller**. Connecte tout. | Un composant à placer sur Inventory ou sur les objets du monde. |
| **Player Inventory Interactor** | Gère les actions runtime (ramassage, drop, équipement). À placer sur **Player Controller/Inventory**. | Un composant à placer directement sur Player Controller. |
| **World Interaction Prompt UI** | Prompt UI de l'interaction monde. Créé **automatiquement** par le bridge. | Un composant à placer manuellement dans le test minimal. |
| **Inventory Bootstrapper Base** | Classe de base technique pour les bootstrappers. | Un composant à utiliser directement en test — utiliser **Debug Inventory Bootstrapper** à la place. |

---

## Checklist de test runtime

1. Lancer la scène.
2. Appuyer sur **TAB**.
3. Vérifier que **Game Menu UI** s'ouvre.
4. Vérifier que la page **Inventory** affiche une grille.
5. Vérifier que les boutons/items debug apparaissent si **Debug Inventory Bootstrapper** est présent.
6. Placer un **World Item** avec Collider et ItemDefinition dans la scène.
7. Viser l'objet.
8. Vérifier que le prompt **Pick up** apparaît.
9. Appuyer sur **E**.
10. Vérifier que l'item arrive dans l'inventaire (ouvrir avec TAB).
11. Placer un **Loot Container** avec Collider dans la scène.
12. Viser le container.
13. Vérifier que le prompt **Open** apparaît.
14. Appuyer sur **E**.
15. Vérifier que le menu s'ouvre sur la page Inventory avec le contenu du container.

---

## Diagnostic rapide

| Symptôme | Cause probable | Vérification |
|----------|---------------|--------------|
| Menu ne s'ouvre pas | `Game Menu UI` absent ou `ToggleKey` incorrect | Vérifier `UI/GameMenuUI` et la valeur de `ToggleKey`. |
| Page Inventory vide | `InventoryComponent` non trouvé par `GameMenuUI` | Vérifier que `Player Controller/Inventory` contient **Player Inventory Component** et que le GameObject s'appelle exactement `Inventory`. |
| Items debug absents | `Debug Inventory Bootstrapper` absent ou propriété `Inventory` non assignée | Vérifier `Debug Inventory Bootstrapper.Inventory` → doit pointer vers **Player Inventory Component**. |
| Aucun prompt sur un item | Collider absent, `ViewCamera` non assigné ou `TraceDistance` trop court | Activer `LogRaycastDebug` et `LogResults` sur **Player Inventory Interaction Bridge**. |
| Deux prompts superposés | `World Interaction Prompt UI` placé manuellement alors que le bridge en crée un automatiquement | Supprimer le prompt manuel de la scène pour le test minimal. |
| Item disparaît mais n'arrive pas dans l'inventaire | `Player Inventory Interactor` mal câblé ou `Inventory` non assigné | Vérifier le composant **Player Inventory Interactor** sur `Player Controller/Inventory`. |

---

## Règle finale

> Pour le test minimal, le **seul composant central** à placer sur **Player Controller** est **Player Inventory Interaction Bridge**.
> Les composants d'inventaire (**Player Inventory Component**, **Player Loadout Component**, **Player Inventory Interactor**) doivent rester sur **Player Controller/Inventory**.
