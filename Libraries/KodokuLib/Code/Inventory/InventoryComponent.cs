using Sandbox;
using Kodoku.Lib.Items;
using Kodoku.Lib.Loadout;

namespace Kodoku.Lib.Inventory;

[Title( "Player Inventory Component" )]
[Category( "Kodoku/Inventory" )]
[Icon( "inventory_2" )]
public sealed class InventoryComponent : Component
{
	public const string PocketsContainerId = "pockets";
	static readonly InventoryEquipmentSlot[] WeaponEquipmentSlots =
	{
		InventoryEquipmentSlot.OnSling,
		InventoryEquipmentSlot.OnBack
	};

	readonly List<InventoryContainer> _activeContainers = new();
	readonly Dictionary<InventoryEquipmentSlot, InventoryContainer> _storageContainers = new();

	[Property] public bool LogActionResults { get; set; } = true;
	[Property] public float WorldItemPickupRadius { get; set; } = 160f;

	public InventoryContainer Pockets { get; private set; }
	public LoadoutComponent Loadout { get; private set; }

	public InventoryContainer BackpackContainer => GetStorageContainer( InventoryEquipmentSlot.Backpack );

	public InventoryContainer GetStorageContainer( InventoryEquipmentSlot slot )
		=> _storageContainers.TryGetValue( slot, out var c ) ? c : null;
	public IReadOnlyList<InventoryContainer> ActiveContainers => _activeContainers;

	protected override void OnStart()
	{
		EnsureInitialized();
	}

	public void EnsureInitialized()
	{
		Pockets ??= new InventoryContainer( PocketsContainerId, "Pockets", InventoryContainerKind.Pockets, 3, 2 );

		Loadout = Components.Get<LoadoutComponent>();
		if ( Loadout is null || !Loadout.IsValid() )
			Loadout = Components.Create<LoadoutComponent>();

		RebuildActiveContainers();
	}

	public InventoryActionResult TryAddItem( ItemInstance item )
	{
		EnsureInitialized();

		if ( item is null || !item.IsValid )
			return InventoryActionResult.Fail( "Item is missing or invalid." );

		var autoEquipSlot = FindFirstAvailableAutoEquipSlot( item );
		if ( autoEquipSlot.HasValue )
		{
			var result = Loadout.TryEquip( autoEquipSlot.Value, item );
			if ( result.Success )
				RebuildActiveContainers();
			LogResult( result );
			return result;
		}

		return TryStoreItemInActiveContainers( item );
	}

	public InventoryActionResult TryAddItemToContainer( string containerId, ItemInstance item, int x, int y, bool rotated )
	{
		EnsureInitialized();

		var container = FindContainer( containerId );
		if ( container is null )
			return InventoryActionResult.Fail( $"Container {containerId} does not exist." );

		var result = container.TryAddItemAt( item, x, y, rotated, out _ );
		LogResult( result );
		return result;
	}

	public InventoryActionResult TryMoveItem( string itemId, string targetContainerId, int x, int y, bool rotated )
	{
		EnsureInitialized();

		var targetContainer = FindContainer( targetContainerId );
		if ( targetContainer is null )
			return InventoryActionResult.Fail( $"Container {targetContainerId} does not exist." );

		if ( TryFindContainerLocation( itemId, out var sourceContainer, out var sourcePlacement ) )
		{
			if ( sourceContainer == targetContainer )
			{
				var sameContainerResult = sourceContainer.TryMoveItem( itemId, x, y, rotated );
				LogResult( sameContainerResult );
				return sameContainerResult;
			}

			var canAdd = targetContainer.CanAddItemAt( sourcePlacement.Item, x, y, rotated );
			if ( !canAdd.Success )
			{
				LogResult( canAdd );
				return canAdd;
			}

			var removeResult = sourceContainer.TryRemoveItem( itemId, out var movedItem );
			if ( !removeResult.Success )
			{
				LogResult( removeResult );
				return removeResult;
			}

			var addResult = targetContainer.TryAddItemAt( movedItem, x, y, rotated, out _ );
			LogResult( addResult );
			return addResult;
		}

		var equippedSlot = Loadout.FindItemSlot( itemId );
		if ( equippedSlot.HasValue )
		{
			var equippedItem = Loadout.GetEquipped( equippedSlot.Value );
			if ( IsStorageContainerForSlot( equippedSlot.Value, targetContainer ) )
				return InventoryActionResult.Fail( $"{equippedItem.DisplayName} cannot be stored inside itself." );

			var canAdd = targetContainer.CanAddItemAt( equippedItem, x, y, rotated );
			if ( !canAdd.Success )
			{
				LogResult( canAdd );
				return canAdd;
			}

			SaveStorageContainerForSlot( equippedSlot.Value );
			var unequip = Loadout.TryUnequip( equippedSlot.Value, out var movedItem );
			if ( !unequip.Success )
			{
				LogResult( unequip );
				return unequip;
			}

			var addResult = targetContainer.TryAddItemAt( movedItem, x, y, rotated, out _ );
			if ( addResult.Success )
				RebuildActiveContainers();
			LogResult( addResult );
			return addResult;
		}

		var failed = InventoryActionResult.Fail( $"Item {itemId} was not found in a movable location." );
		LogResult( failed );
		return failed;
	}

	public InventoryActionResult TryEquipWeaponItem( string itemId )
	{
		EnsureInitialized();

		var item = FindItemForEquip( itemId, out var sourceContainer, out var sourceSlot );
		if ( item is null )
			return InventoryActionResult.Fail( $"Item {itemId} was not found." );

		var targetSlot = FindFirstAvailableWeaponSlot( item );
		if ( !targetSlot.HasValue )
			return InventoryActionResult.Fail( $"{item.DisplayName} cannot be equipped to On Sling or On Back." );

		var removeResult = RemoveFromSource( itemId, sourceContainer, sourceSlot, out var removedItem );
		if ( !removeResult.Success )
			return removeResult;

		var equipResult = Loadout.TryEquip( targetSlot.Value, removedItem );
		LogResult( equipResult );
		return equipResult;
	}

	public InventoryActionResult TryEquipItem( string itemId, InventoryEquipmentSlot slot )
	{
		EnsureInitialized();

		if ( Loadout.IsOccupied( slot ) )
			return InventoryActionResult.Fail( $"{slot} is already occupied." );

		var item = FindItemForEquip( itemId, out var sourceContainer, out var sourceSlot );
		if ( item is null )
			return InventoryActionResult.Fail( $"Item {itemId} was not found." );

		if ( !item.Definition.CanEquipTo( slot ) )
			return InventoryActionResult.Fail( $"{item.DisplayName} cannot be equipped in {slot}." );

		var removeResult = RemoveFromSource( itemId, sourceContainer, sourceSlot, out var removedItem );
		if ( !removeResult.Success )
			return removeResult;

		var equipResult = Loadout.TryEquip( slot, removedItem );
		if ( !equipResult.Success )
			return equipResult;

		RebuildActiveContainers();
		LogResult( equipResult );
		return equipResult;
	}

	public InventoryActionResult TryUnequipItem( InventoryEquipmentSlot slot )
	{
		EnsureInitialized();

		if ( slot == InventoryEquipmentSlot.Backpack )
		{
			SaveStorageContainerForSlot( InventoryEquipmentSlot.Backpack );
			var unequip = Loadout.TryUnequip( InventoryEquipmentSlot.Backpack, out var backpack );
			if ( !unequip.Success )
				return unequip;

			RebuildActiveContainers();

			var storeResult = TryStoreItemInActiveContainers( backpack );
			if ( !storeResult.Success )
			{
				Loadout.TryEquip( InventoryEquipmentSlot.Backpack, backpack );
				RebuildActiveContainers();
			}

			LogResult( storeResult );
			return storeResult;
		}

		SaveStorageContainerForSlot( slot );
		var slotUnequip = Loadout.TryUnequip( slot, out var unequippedItem );
		if ( !slotUnequip.Success )
			return slotUnequip;

		var gridResult = TryStoreItemInActiveContainers( unequippedItem );
		if ( !gridResult.Success )
			Loadout.TryEquip( slot, unequippedItem );

		LogResult( gridResult );
		return gridResult;
	}

	public InventoryActionResult TryDropItem( string itemId, out ItemInstance droppedItem )
	{
		EnsureInitialized();
		droppedItem = null;

		if ( TryFindContainerLocation( itemId, out var container, out _ ) )
		{
			var result = container.TryRemoveItem( itemId, out droppedItem );
			LogResult( result );
			return result;
		}

		var equippedSlot = Loadout.FindItemSlot( itemId );
		if ( equippedSlot.HasValue )
		{
			SaveStorageContainerForSlot( equippedSlot.Value );
			var result = Loadout.TryUnequip( equippedSlot.Value, out droppedItem );
			if ( result.Success && droppedItem?.CreatesContainer == true )
				RebuildActiveContainers();
			LogResult( result );
			return result;
		}

		var failed = InventoryActionResult.Fail( $"Item {itemId} was not found." );
		LogResult( failed );
		return failed;
	}

	public InventoryActionResult TryDropItemToWorld( string itemId, Transform transform, out WorldItemComponent worldItem )
	{
		worldItem = null;

		var result = TryDropItem( itemId, out var droppedItem );
		if ( !result.Success )
			return result;

		worldItem = WorldItemComponent.SpawnDropped( Scene, transform, droppedItem );
		return InventoryActionResult.Ok( $"{droppedItem.DisplayName} dropped to world." );
	}

	public InventoryActionResult TryDropItemToWorld( string itemId, out WorldItemComponent worldItem )
	{
		return TryDropItemToWorld( itemId, GetDefaultDropTransform(), out worldItem );
	}

	public InventoryActionResult TryPickupWorldItem( WorldItemComponent worldItem )
	{
		EnsureInitialized();

		if ( worldItem is null || !worldItem.IsValid() )
			return InventoryActionResult.Fail( "World item is invalid." );

		var item = worldItem.PeekItem();
		if ( item is null || !item.IsValid )
			return InventoryActionResult.Fail( "World item has no valid inventory item." );

		var result = TryAddItem( item );
		if ( !result.Success )
			return result;

		worldItem.ConsumeWorldItem();
		return result;
	}

	public InventoryActionResult TryStoreWorldItem( WorldItemComponent worldItem )
	{
		EnsureInitialized();

		if ( worldItem is null || !worldItem.IsValid() )
			return InventoryActionResult.Fail( "World item is invalid." );

		var item = worldItem.PeekItem();
		if ( item is null || !item.IsValid )
			return InventoryActionResult.Fail( "World item has no valid inventory item." );

		var result = TryStoreItemInActiveContainers( item );
		if ( !result.Success )
			return result;

		worldItem.ConsumeWorldItem();
		return result;
	}

	public InventoryActionResult TryEquipWorldItem( WorldItemComponent worldItem )
	{
		EnsureInitialized();

		if ( worldItem is null || !worldItem.IsValid() )
			return InventoryActionResult.Fail( "World item is invalid." );

		var item = worldItem.PeekItem();
		if ( item is null || !item.IsValid )
			return InventoryActionResult.Fail( "World item has no valid inventory item." );

		var targetSlot = FindFirstAvailableEquipmentSlot( item );
		if ( !targetSlot.HasValue )
			return InventoryActionResult.Fail( $"{item.DisplayName} cannot be equipped." );

		var result = Loadout.TryEquip( targetSlot.Value, item );
		if ( !result.Success )
			return result;

		worldItem.ConsumeWorldItem();
		RebuildActiveContainers();
		LogResult( result );
		return result;
	}

	public InventoryActionResult TrySplitItem( string itemId, int splitQuantity, out ItemInstance splitItem )
	{
		EnsureInitialized();
		splitItem = null;

		if ( !TryFindContainerLocation( itemId, out var container, out _ ) )
			return InventoryActionResult.Fail( $"Item {itemId} is not in a container." );

		var result = container.TrySplitItem( itemId, splitQuantity, out splitItem );
		LogResult( result );
		return result;
	}

	public void ReturnSplitQuantity( string sourceItemId, int quantity )
	{
		EnsureInitialized();

		if ( !TryFindContainerLocation( sourceItemId, out var container, out _ ) )
			return;

		container.ReturnSplitQuantity( sourceItemId, quantity );
	}

	public InventoryActionResult TryAutoMoveItem( string itemId )
	{
		EnsureInitialized();

		if ( string.IsNullOrWhiteSpace( itemId ) )
			return InventoryActionResult.Fail( "No item specified." );

		var equippedSlot = Loadout.FindItemSlot( itemId );
		if ( equippedSlot.HasValue )
			return TryUnequipItem( equippedSlot.Value );

		if ( !TryFindContainerLocation( itemId, out _, out _ ) )
			return InventoryActionResult.Fail( $"Item {itemId} was not found." );

		var result = TryEquipWeaponItem( itemId );
		LogResult( result );
		return result;
	}

	public InventoryActionResult TryPickupNearestWorldItem()
	{
		EnsureInitialized();

		var nearest = FindNearestWorldItem( WorldItemPickupRadius );
		if ( nearest is null )
			return InventoryActionResult.Fail( $"No WorldItem within {WorldItemPickupRadius:0} units." );

		var result = TryPickupWorldItem( nearest );
		LogResult( result );
		return result;
	}

	InventoryContainer FindContainer( string containerId )
	{
		return ActiveContainers.FirstOrDefault( x => x.ContainerId == containerId );
	}

	public InventoryEquipmentSlot? FindFirstAvailableWeaponSlot( ItemInstance item )
		=> FindFirstAvailableEquipmentSlot( item, WeaponEquipmentSlots );

	public InventoryEquipmentSlot? FindFirstAvailableEquipmentSlot( ItemInstance item )
		=> FindFirstAvailableEquipmentSlot( item, System.Enum.GetValues<InventoryEquipmentSlot>() );

	InventoryEquipmentSlot? FindFirstAvailableAutoEquipSlot( ItemInstance item )
	{
		if ( item is null || !item.IsValid )
			return null;

		return FindFirstAvailableEquipmentSlot( item );
	}

	InventoryEquipmentSlot? FindFirstAvailableEquipmentSlot( ItemInstance item, IEnumerable<InventoryEquipmentSlot> slots )
	{
		if ( item is null || !item.IsValid )
			return null;

		foreach ( var slot in slots )
		{
			if ( Loadout.IsOccupied( slot ) )
				continue;

			if ( item.Definition.CanEquipTo( slot ) )
				return slot;
		}

		return null;
	}

	InventoryActionResult TryStoreItemInActiveContainers( ItemInstance item )
	{
		foreach ( var container in ActiveContainers )
		{
			var result = container.TryAddItem( item, false, out _ );
			if ( result.Success )
			{
				LogResult( result );
				return result;
			}
		}

		var failed = InventoryActionResult.Fail( $"No active container can accept {item.DisplayName}." );
		LogResult( failed );
		return failed;
	}

	WorldItemComponent FindNearestWorldItem( float maxDistance )
	{
		var maxDistanceSquared = maxDistance * maxDistance;
		WorldItemComponent nearest = null;
		var nearestDistanceSquared = float.MaxValue;

		foreach ( var worldItem in Scene.GetAllComponents<WorldItemComponent>() )
		{
			if ( worldItem is null || !worldItem.IsValid() )
				continue;

			var item = worldItem.PeekItem();
			if ( item is null || !item.IsValid )
				continue;

			var distanceSquared = (worldItem.WorldPosition - WorldPosition).LengthSquared;
			if ( distanceSquared > maxDistanceSquared || distanceSquared >= nearestDistanceSquared )
				continue;

			nearest = worldItem;
			nearestDistanceSquared = distanceSquared;
		}

		return nearest;
	}

	Transform GetDefaultDropTransform()
	{
		return WorldTransform.WithPosition( WorldPosition + WorldRotation.Forward * 72f + Vector3.Up * 24f );
	}

	bool TryFindContainerLocation( string itemId, out InventoryContainer container, out InventoryItemPlacement placement )
	{
		container = null;
		placement = null;

		foreach ( var activeContainer in ActiveContainers )
		{
			var candidate = activeContainer.FindPlacement( itemId );
			if ( candidate is null )
				continue;

			container = activeContainer;
			placement = candidate;
			return true;
		}

		return false;
	}

	ItemInstance FindItemForEquip( string itemId, out InventoryContainer sourceContainer, out InventoryEquipmentSlot? sourceSlot )
	{
		sourceContainer = null;
		sourceSlot = null;

		if ( TryFindContainerLocation( itemId, out sourceContainer, out var placement ) )
			return placement.Item;

		var foundSlot = Loadout.FindItemSlot( itemId );
		if ( foundSlot.HasValue )
		{
			sourceSlot = foundSlot.Value;
			return Loadout.GetEquipped( foundSlot.Value );
		}

		return null;
	}

	InventoryActionResult RemoveFromSource( string itemId, InventoryContainer sourceContainer, InventoryEquipmentSlot? sourceSlot, out ItemInstance item )
	{
		item = null;

		if ( sourceContainer is not null )
			return sourceContainer.TryRemoveItem( itemId, out item );

		if ( sourceSlot.HasValue )
		{
			SaveStorageContainerForSlot( sourceSlot.Value );
			return Loadout.TryUnequip( sourceSlot.Value, out item );
		}

		return InventoryActionResult.Fail( $"Item {itemId} has no removable source." );
	}

	public bool IsStorageContainerForSlot( InventoryEquipmentSlot slot, InventoryContainer container )
	{
		return container is not null
			&& _storageContainers.TryGetValue( slot, out var storageContainer )
			&& ReferenceEquals( storageContainer, container );
	}

	public void SaveStorageContainerForSlot( InventoryEquipmentSlot slot )
	{
		var item = Loadout?.GetEquipped( slot );
		if ( item?.CreatesContainer != true )
			return;

		if ( _storageContainers.TryGetValue( slot, out var container ) )
			item.StoreContainer( container );
	}

	public InventoryContainer EnsureStorageContainerForItem( ItemInstance item )
	{
		return item?.EnsureStoredContainer();
	}

	void RebuildActiveContainers()
	{
		foreach ( var pair in _storageContainers )
		{
			var item = Loadout?.GetEquipped( pair.Key );
			if ( item?.CreatesContainer == true )
				item.StoreContainer( pair.Value );
		}

		var toRemove = new List<InventoryEquipmentSlot>();
		foreach ( var slot in _storageContainers.Keys )
			if ( Loadout?.GetEquipped( slot )?.CreatesContainer != true )
				toRemove.Add( slot );
		foreach ( var slot in toRemove )
			_storageContainers.Remove( slot );

		foreach ( InventoryEquipmentSlot slot in System.Enum.GetValues<InventoryEquipmentSlot>() )
		{
			var item = Loadout?.GetEquipped( slot );
			if ( item?.CreatesContainer == true && !_storageContainers.ContainsKey( slot ) )
				_storageContainers[slot] = item.EnsureStoredContainer();
		}

		_activeContainers.Clear();
		_activeContainers.Add( Pockets );
		foreach ( InventoryEquipmentSlot slot in System.Enum.GetValues<InventoryEquipmentSlot>() )
			if ( _storageContainers.TryGetValue( slot, out var container ) )
				_activeContainers.Add( container );
	}

	void LogResult( InventoryActionResult result )
	{
		if ( LogActionResults )
			Log.Info( result.ToString() );
	}
}
