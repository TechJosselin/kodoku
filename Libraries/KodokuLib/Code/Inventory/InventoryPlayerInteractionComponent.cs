using Sandbox;
using Kodoku.Lib.Items;

namespace Kodoku.Lib.Inventory;

[Title( "Player Inventory Interactor" )]
[Category( "Kodoku/Inventory" )]
[Icon( "touch_app" )]
public sealed class InventoryPlayerInteractionComponent : Component
{
	[Property] public InventoryComponent Inventory { get; set; }
	[Property] public float DropDistance { get; set; } = 72f;
	[Property] public float DropUpOffset { get; set; } = 12f;
	[Property] public string ToggleInventoryAction { get; set; } = "Score";
	[Property] public string RotateAction { get; set; } = "Reload";
	[Property] public string ToggleInventoryKey { get; set; } = "TAB";
	[Property] public string RotateKey { get; set; } = "R";
	[Property] public bool HandleInventoryToggleInput { get; set; }
	[Property] public bool HandleInventoryRotateInput { get; set; }
	[Property] public bool LogInputResults { get; set; } = true;

	WorldItemComponent LookedAtWorldItem { get; set; }
	LootContainerComponent LookedAtLootContainer { get; set; }
	public LootContainerComponent OpenedLootContainer { get; private set; }
	public ItemInstance OpenedItemStorageItem { get; private set; }
	public InventoryContainer OpenedItemStorageContainer { get; private set; }

	Vector3 InteractionRangeOrigin { get; set; }
	float InteractionRange { get; set; } = 180f;
	Transform InteractionDropTransform { get; set; }
	bool HasInteractionRangeContext { get; set; }
	bool HasInteractionDropTransform { get; set; }

	protected override void OnStart()
	{
		ResolveReferences();
		Log.Info( $"InventoryInteractor enabled. Inventory: {IsComponentValid( Inventory )}" );
	}

	protected override void OnUpdate()
	{
		ResolveReferences();
		ValidateOpenedLootContainer();
		ValidateOpenedItemStorageContainer();

		if ( HandleInventoryToggleInput && WasPressed( ToggleInventoryAction, ToggleInventoryKey ) )
			RequestCloseLootContainer();

		if ( InteractionRangeOrigin == default )
			InteractionRangeOrigin = WorldPosition;
	}

	void ResolveReferences()
	{
		if ( Inventory is null || !Inventory.IsValid() )
		{
			Inventory = Components.Get<InventoryComponent>();

			if ( Inventory is null || !Inventory.IsValid() )
				Inventory = Scene.GetAllComponents<InventoryComponent>().FirstOrDefault( candidate =>
					candidate.GameObject == GameObject || candidate.GameObject == GameObject.Parent );

			if ( Inventory is null || !Inventory.IsValid() )
				Inventory = Scene.GetAllComponents<InventoryComponent>().FirstOrDefault();
		}
	}

	public void SetWorldInteractionContext( Vector3 rangeOrigin, float range )
	{
		InteractionRangeOrigin = rangeOrigin;
		InteractionRange = range > 0f ? range : 180f;
		HasInteractionRangeContext = true;
		HasInteractionDropTransform = false;
	}

	public void SetWorldInteractionContext( Vector3 rangeOrigin, float range, Transform dropTransform )
	{
		InteractionRangeOrigin = rangeOrigin;
		InteractionRange = range > 0f ? range : 180f;
		InteractionDropTransform = dropTransform;
		HasInteractionRangeContext = true;
		HasInteractionDropTransform = true;
	}

	public void SetLookedAtWorldObjects( WorldItemComponent worldItem, LootContainerComponent lootContainer )
	{
		LookedAtWorldItem = worldItem;
		LookedAtLootContainer = lootContainer;
	}

	public InventoryActionResult RequestPickupLookedAtWorldItem()
	{
		var worldItem = LookedAtWorldItem ?? FindLookedAtWorldItem();
		if ( worldItem is null || !worldItem.IsValid() )
			return InventoryActionResult.Fail( "No WorldItem in view." );

		var result = RequestPickupWorldItem( worldItem );

		if ( result.Success )
			LookedAtWorldItem = null;

		return result;
	}

	public InventoryActionResult RequestUseLookedAtObject()
	{
		var worldItem = LookedAtWorldItem ?? FindLookedAtWorldItem();
		if ( worldItem is not null && worldItem.IsValid() )
			return RequestPickupWorldItem( worldItem );

		var lootContainer = LookedAtLootContainer ?? FindLookedAtLootContainer();
		if ( lootContainer is not null && lootContainer.IsValid() )
			return RequestOpenLootContainer( lootContainer );

		return InventoryActionResult.Fail( "No usable inventory object in view." );
	}

	public InventoryActionResult RequestOpenLootContainer( LootContainerComponent lootContainer )
	{
		if ( !TryGetInventory( out _, out var failed ) )
			return failed;

		if ( lootContainer is null || !lootContainer.IsValid() )
			return InventoryActionResult.Fail( "Loot container is invalid." );

		if ( !IsLootContainerInUseRange( lootContainer ) )
			return InventoryActionResult.Fail( $"{lootContainer.ContainerName} is too far away." );

		lootContainer.EnsureInitialized();
		OpenedLootContainer = lootContainer;
		return InventoryActionResult.Ok( $"{lootContainer.ContainerName} opened." );
	}

	public InventoryActionResult RequestCloseLootContainer()
	{
		if ( OpenedLootContainer is null )
			return InventoryActionResult.Ok( "No loot container open." );

		var name = OpenedLootContainer.ContainerName;
		OpenedLootContainer = null;
		ValidateOpenedItemStorageContainer();
		return InventoryActionResult.Ok( $"{name} closed." );
	}

	public InventoryActionResult RequestOpenItemStorage( string itemId )
	{
		if ( !TryGetInventory( out var inventory, out var failed ) )
			return failed;

		if ( string.IsNullOrWhiteSpace( itemId ) )
			return InventoryActionResult.Fail( "No selected item." );

		if ( inventory.Loadout?.FindItemSlot( itemId ) is not null )
			return InventoryActionResult.Fail( "Equipped containers cannot be opened from here." );

		if ( TryFindOpenedItemStorageLocation( itemId, out _, out _ ) )
			return InventoryActionResult.Fail( "Nested storage popups are not supported." );

		if ( !TryFindBaseVisibleContainerLocation( itemId, out _, out var placement ) )
			return InventoryActionResult.Fail( $"Item {itemId} was not found in a visible container." );

		var item = placement.Item;
		if ( item?.CreatesContainer != true )
			return InventoryActionResult.Fail( "This item cannot be opened." );

		OpenedItemStorageItem = item;
		OpenedItemStorageContainer = inventory.EnsureStorageContainerForItem( item );

		return OpenedItemStorageContainer is not null
			? InventoryActionResult.Ok( $"{item.DisplayName} opened." )
			: InventoryActionResult.Fail( $"{item.DisplayName} has no storage." );
	}

	public InventoryActionResult RequestCloseItemStorage()
	{
		if ( OpenedItemStorageItem is null && OpenedItemStorageContainer is null )
			return InventoryActionResult.Ok( "No item storage open." );

		var name = OpenedItemStorageItem?.DisplayName ?? OpenedItemStorageContainer?.DisplayName ?? "Item storage";
		ClearOpenedItemStorage();
		return InventoryActionResult.Ok( $"{name} closed." );
	}

	public void RequestCloseItemStorageIfOwnerMoves( string itemId )
	{
		if ( IsOpenedItemStorageOwner( itemId ) )
			ClearOpenedItemStorage();
	}

	public InventoryActionResult RequestPickupNearestWorldItem()
	{
		if ( !TryGetInventory( out var inventory, out var failed ) )
			return failed;

		var nearest = inventory.FindNearestWorldItem( GetRangeCheckOrigin(), GetPickupRange() );
		if ( nearest is null )
			return InventoryActionResult.Fail( $"No WorldItem within {GetPickupRange():0} units." );

		return RequestPickupWorldItem( nearest );
	}

	public InventoryActionResult RequestPickupWorldItem( WorldItemComponent worldItem )
	{
		if ( !TryGetInventory( out var inventory, out var failed ) )
			return failed;

		if ( worldItem is null || !worldItem.IsValid() )
			return InventoryActionResult.Fail( "World item is invalid." );

		var item = worldItem.PeekItem();
		if ( item is null || !item.IsValid )
			return InventoryActionResult.Fail( "World item has no valid inventory item." );

		if ( !IsWorldItemInPickupRange( worldItem ) )
			return InventoryActionResult.Fail( $"{item.DisplayName} is too far away." );

		return inventory.TryPickupWorldItem( worldItem );
	}

	public InventoryActionResult RequestStoreWorldItem( WorldItemComponent worldItem )
	{
		if ( !TryGetInventory( out var inventory, out var failed ) )
			return failed;

		if ( worldItem is null || !worldItem.IsValid() )
			return InventoryActionResult.Fail( "World item is invalid." );

		var item = worldItem.PeekItem();
		if ( item is null || !item.IsValid )
			return InventoryActionResult.Fail( "World item has no valid inventory item." );

		if ( !IsWorldItemInPickupRange( worldItem ) )
			return InventoryActionResult.Fail( $"{item.DisplayName} is too far away." );

		return inventory.TryStoreWorldItem( worldItem );
	}

	public InventoryActionResult RequestEquipWorldItem( WorldItemComponent worldItem )
	{
		if ( !TryGetInventory( out var inventory, out var failed ) )
			return failed;

		if ( worldItem is null || !worldItem.IsValid() )
			return InventoryActionResult.Fail( "World item is invalid." );

		var item = worldItem.PeekItem();
		if ( item is null || !item.IsValid )
			return InventoryActionResult.Fail( "World item has no valid inventory item." );

		if ( !IsWorldItemInPickupRange( worldItem ) )
			return InventoryActionResult.Fail( $"{item.DisplayName} is too far away." );

		return inventory.TryEquipWorldItem( worldItem );
	}

	public InventoryActionResult RequestDropItem( string itemId, out WorldItemComponent worldItem )
	{
		worldItem = null;

		if ( !TryGetInventory( out var inventory, out var failed ) )
			return failed;

		if ( string.IsNullOrWhiteSpace( itemId ) )
			return InventoryActionResult.Fail( "No selected item." );

		if ( TryFindOpenedItemStorageLocation( itemId, out var storageSource, out var storagePlacement ) )
			return TryDropFromContainerSource( storageSource, storagePlacement, GetDropTransform(), out worldItem );

		if ( TryFindOpenLootLocation( itemId, out var lootSource, out var lootPlacement ) )
			return TryDropFromContainerSource( lootSource, lootPlacement, GetDropTransform(), out worldItem );

		return inventory.TryDropItemToWorld( itemId, GetDropTransform(), out worldItem );
	}

	public InventoryActionResult RequestMoveItem( string itemId, string targetContainerId, int x, int y, bool rotated )
	{
		if ( !TryGetInventory( out var inventory, out var failed ) )
			return failed;

		if ( string.IsNullOrWhiteSpace( itemId ) )
			return InventoryActionResult.Fail( "No selected item." );

		var targetContainer = FindVisibleContainer( targetContainerId );
		if ( targetContainer is null )
			return InventoryActionResult.Fail( $"Container {targetContainerId} is not available." );

		return TryMoveItemToContainer( itemId, targetContainer, x, y, rotated );
	}

	public InventoryActionResult RequestEquipWeaponItem( string itemId )
	{
		if ( !TryGetInventory( out var inventory, out var failed ) )
			return failed;

		if ( string.IsNullOrWhiteSpace( itemId ) )
			return InventoryActionResult.Fail( "No selected item." );

		if ( TryFindOpenedItemStorageLocation( itemId, out var storageSource, out var storagePlacement ) )
			return TryMoveFromContainerSourceToWeaponSlot( storageSource, storagePlacement );

		if ( TryFindOpenLootLocation( itemId, out var lootSource, out var lootPlacement ) )
			return TryMoveFromContainerSourceToWeaponSlot( lootSource, lootPlacement );

		return inventory.TryEquipWeaponItem( itemId );
	}

	public InventoryActionResult RequestEquipItem( string itemId, InventoryEquipmentSlot slot )
	{
		if ( !TryGetInventory( out var inventory, out var failed ) )
			return failed;

		if ( string.IsNullOrWhiteSpace( itemId ) )
			return InventoryActionResult.Fail( "No selected item." );

		if ( TryFindOpenedItemStorageLocation( itemId, out var storageSource, out var storagePlacement ) )
			return TryMoveFromContainerSourceToEquipmentSlot( storageSource, storagePlacement, slot );

		if ( TryFindOpenLootLocation( itemId, out var lootSource, out var lootPlacement ) )
			return TryMoveFromContainerSourceToEquipmentSlot( lootSource, lootPlacement, slot );

		return inventory.TryEquipItem( itemId, slot );
	}

	public InventoryActionResult RequestAutoMoveItem( string itemId )
	{
		if ( !TryGetInventory( out var inventory, out var failed ) )
			return failed;

		if ( string.IsNullOrWhiteSpace( itemId ) )
			return InventoryActionResult.Fail( "No selected item." );

		if ( TryFindOpenedItemStorageLocation( itemId, out var storageSource, out var storagePlacement ) )
			return TryAutoMoveFromContainerSourceToContainers( storageSource, storagePlacement, GetBaseVisibleContainers() );

		if ( TryFindOpenLootLocation( itemId, out var lootSource, out var lootPlacement ) )
		{
			RequestCloseItemStorageIfOwnerMoves( itemId );

			var removeResult = lootSource.TryRemoveItem( itemId, out var lootItem );
			if ( !removeResult.Success )
				return removeResult;

			var addResult = inventory.TryAddItem( lootItem );
			if ( addResult.Success )
				return addResult;

			lootSource.TryAddItemAt( lootItem, lootPlacement.X, lootPlacement.Y, lootPlacement.Rotated, out _ );
			return addResult;
		}

		if ( OpenedLootContainer is not null && OpenedLootContainer.IsValid() && IsLootContainerInUseRange( OpenedLootContainer ) )
		{
			OpenedLootContainer.EnsureInitialized();
			var lootContainer = OpenedLootContainer.Container;

			var equippedSlot = inventory.Loadout?.FindItemSlot( itemId );
			if ( equippedSlot.HasValue )
				return TryMoveFromEquipmentSlotToContainer( equippedSlot.Value, lootContainer );

			if ( TryFindPlayerContainerLocation( itemId, out var playerSource, out var playerPlacement ) )
				return TryAutoMoveFromContainerSourceToContainers( playerSource, playerPlacement, new[] { lootContainer } );
		}

		return inventory.TryAutoMoveItem( itemId );
	}

	public InventoryActionResult RequestSplitItem( string itemId, int splitQuantity, out ItemInstance splitItem )
	{
		splitItem = null;

		if ( !TryGetInventory( out var inventory, out var failed ) )
			return failed;

		if ( TryFindOpenedItemStorageLocation( itemId, out var storageContainer, out _ ) )
		{
			var storageResult = storageContainer.TrySplitItem( itemId, splitQuantity, out splitItem );
			RunRequest( storageResult );
			return storageResult;
		}

		if ( TryFindOpenLootLocation( itemId, out var lootContainer, out _ ) )
		{
			var lootResult = lootContainer.TrySplitItem( itemId, splitQuantity, out splitItem );
			RunRequest( lootResult );
			return lootResult;
		}

		return inventory.TrySplitItem( itemId, splitQuantity, out splitItem );
	}

	public InventoryActionResult RequestPlaceSplitItem( ItemInstance splitItem, string targetContainerId, int x, int y, bool rotated )
	{
		if ( !TryGetInventory( out _, out var failed ) )
			return failed;

		if ( splitItem is null || !splitItem.IsValid )
			return InventoryActionResult.Fail( "Split item is invalid." );

		var targetContainer = FindVisibleContainer( targetContainerId );
		if ( targetContainer is null )
			return InventoryActionResult.Fail( $"Container {targetContainerId} is not available." );

		var result = targetContainer.TryAddItemAt( splitItem, x, y, rotated, out _ );
		RunRequest( result );
		return result;
	}

	public void RequestCancelSplit( string sourceItemId, int quantity )
	{
		if ( !TryGetInventory( out var inventory, out _ ) )
			return;

		if ( TryFindOpenedItemStorageLocation( sourceItemId, out var storageContainer, out _ ) )
		{
			storageContainer.ReturnSplitQuantity( sourceItemId, quantity );
			return;
		}

		if ( TryFindOpenLootLocation( sourceItemId, out var lootContainer, out _ ) )
		{
			lootContainer.ReturnSplitQuantity( sourceItemId, quantity );
			return;
		}

		inventory.ReturnSplitQuantity( sourceItemId, quantity );
	}

	public InventoryActionResult RequestUseItem( string itemId )
	{
		if ( !TryGetInventory( out var inventory, out var failed ) )
			return failed;

		if ( string.IsNullOrWhiteSpace( itemId ) )
			return InventoryActionResult.Fail( "No selected item." );

		return inventory.TryUseItem( itemId );
	}

	public InventoryActionResult RequestUnequipItem( InventoryEquipmentSlot slot )
	{
		if ( !TryGetInventory( out var inventory, out var failed ) )
			return failed;

		return inventory.TryUnequipItem( slot );
	}

	public InventoryActionResult RequestSwapEquipmentSlots( InventoryEquipmentSlot from, InventoryEquipmentSlot to )
	{
		if ( !TryGetInventory( out var inventory, out var failed ) )
			return failed;

		if ( inventory.Loadout is null || !inventory.Loadout.IsValid() )
			return InventoryActionResult.Fail( "No loadout component." );

		return inventory.Loadout.TrySwap( from, to );
	}

	bool WasPressed( string actionName, string keyName )
	{
		if ( !string.IsNullOrWhiteSpace( actionName ) && Input.Pressed( actionName ) )
			return true;

		if ( !string.IsNullOrWhiteSpace( keyName ) && Input.Keyboard.Pressed( keyName ) )
			return true;

		return false;
	}

	void RunRequest( InventoryActionResult result )
	{
		if ( LogInputResults )
			Log.Info( result.ToString() );
	}

	void UpdateLookedAtWorldItem()
	{
		LookedAtWorldItem = FindLookedAtWorldItem();
		LookedAtLootContainer = null;

		var item = LookedAtWorldItem?.PeekItem();
		if ( item is not null && item.IsValid && IsWorldItemInPickupRange( LookedAtWorldItem ) )
			return;

		LookedAtLootContainer = FindLookedAtLootContainer();

		if ( LookedAtLootContainer is not null && IsLootContainerInUseRange( LookedAtLootContainer ) )
			return;
	}

	bool TryGetInventory( out InventoryComponent inventory, out InventoryActionResult failed )
	{
		ResolveReferences();

		if ( Inventory is null || !Inventory.IsValid() )
		{
			inventory = null;
			failed = InventoryActionResult.Fail( "No inventory." );
			return false;
		}

		Inventory.EnsureInitialized();
		inventory = Inventory;
		failed = InventoryActionResult.Ok();
		return true;
	}

	void ValidateOpenedLootContainer()
	{
		if ( OpenedLootContainer is null )
			return;

		if ( !OpenedLootContainer.IsValid() || !IsLootContainerInUseRange( OpenedLootContainer ) )
			OpenedLootContainer = null;
	}

	void ValidateOpenedItemStorageContainer()
	{
		if ( OpenedItemStorageItem is null && OpenedItemStorageContainer is null )
			return;

		if ( Inventory is null || !Inventory.IsValid()
			|| OpenedItemStorageItem is null
			|| !OpenedItemStorageItem.IsValid
			|| !OpenedItemStorageItem.CreatesContainer
			|| !TryFindBaseVisibleContainerLocation( OpenedItemStorageItem.InstanceId, out _, out _ ) )
		{
			ClearOpenedItemStorage();
			return;
		}

		OpenedItemStorageContainer = Inventory.EnsureStorageContainerForItem( OpenedItemStorageItem );
		if ( OpenedItemStorageContainer is null )
			ClearOpenedItemStorage();
	}

	bool IsWorldItemInPickupRange( WorldItemComponent worldItem )
	{
		if ( worldItem is null || !worldItem.IsValid() )
			return false;

		var maxDistance = GetPickupRange();
		var distanceSquared = (worldItem.WorldPosition - GetRangeCheckOrigin()).LengthSquared;
		return distanceSquared <= maxDistance * maxDistance;
	}

	bool IsLootContainerInUseRange( LootContainerComponent lootContainer )
	{
		if ( lootContainer is null || !lootContainer.IsValid() )
			return false;

		var maxDistance = GetPickupRange();
		var distanceSquared = (lootContainer.WorldPosition - GetRangeCheckOrigin()).LengthSquared;
		return distanceSquared <= maxDistance * maxDistance;
	}

	Vector3 GetRangeCheckOrigin()
	{
		return InteractionRangeOrigin == default ? WorldPosition : InteractionRangeOrigin;
	}

	float GetPickupRange()
	{
		var inventoryRange = 0f;
		if ( Inventory is not null && Inventory.IsValid() && Inventory.WorldItemPickupRadius > 0f )
			inventoryRange = Inventory.WorldItemPickupRadius;

		if ( HasInteractionRangeContext && InteractionRange > 0f )
			return System.Math.Max( inventoryRange, InteractionRange );

		return inventoryRange > 0f ? inventoryRange : 180f;
	}

	Transform GetDropTransform()
	{
		if ( HasInteractionDropTransform )
			return InteractionDropTransform;

		return WorldTransform.WithPosition( WorldPosition + WorldRotation.Forward * DropDistance + Vector3.Up * DropUpOffset );
	}

	InventoryContainer FindVisibleContainer( string containerId )
	{
		if ( string.IsNullOrWhiteSpace( containerId ) )
			return null;

		var playerContainer = Inventory?.ActiveContainers.FirstOrDefault( container => container.ContainerId == containerId );
		if ( playerContainer is not null )
			return playerContainer;

		if ( OpenedLootContainer is not null && OpenedLootContainer.IsValid() && IsLootContainerInUseRange( OpenedLootContainer ) )
		{
			OpenedLootContainer.EnsureInitialized();
			if ( OpenedLootContainer.Container.ContainerId == containerId )
				return OpenedLootContainer.Container;
		}

		if ( OpenedItemStorageContainer is not null && OpenedItemStorageContainer.ContainerId == containerId )
			return OpenedItemStorageContainer;

		return null;
	}

	InventoryActionResult TryMoveItemToContainer( string itemId, InventoryContainer targetContainer, int x, int y, bool rotated )
	{
		if ( IsOpenedItemStorageOwner( itemId ) && ReferenceEquals( targetContainer, OpenedItemStorageContainer ) )
			return InventoryActionResult.Fail( "A container cannot be stored inside itself." );

		RequestCloseItemStorageIfOwnerMoves( itemId );

		if ( TryFindPlayerContainerLocation( itemId, out var playerSource, out var playerPlacement ) )
			return TryMoveFromContainerSource( playerSource, playerPlacement, targetContainer, x, y, rotated );

		if ( TryFindOpenLootLocation( itemId, out var lootSource, out var lootPlacement ) )
			return TryMoveFromContainerSource( lootSource, lootPlacement, targetContainer, x, y, rotated );

		if ( TryFindOpenedItemStorageLocation( itemId, out var storageSource, out var storagePlacement ) )
			return TryMoveFromContainerSource( storageSource, storagePlacement, targetContainer, x, y, rotated );

		var equippedSlot = Inventory?.Loadout?.FindItemSlot( itemId );
		if ( equippedSlot.HasValue )
			return TryMoveFromEquipmentSlot( equippedSlot.Value, targetContainer, x, y, rotated );

		return InventoryActionResult.Fail( $"Item {itemId} was not found in a movable location." );
	}

	InventoryActionResult TryMoveFromContainerSource( InventoryContainer sourceContainer, InventoryItemPlacement sourcePlacement, InventoryContainer targetContainer, int x, int y, bool rotated )
	{
		RequestCloseItemStorageIfOwnerMoves( sourcePlacement.Item.InstanceId );

		if ( sourceContainer == targetContainer )
			return sourceContainer.TryMoveItem( sourcePlacement.Item.InstanceId, x, y, rotated );

		var canAdd = targetContainer.CanAddItemAt( sourcePlacement.Item, x, y, rotated );
		if ( !canAdd.Success )
			return canAdd;

		var oldX = sourcePlacement.X;
		var oldY = sourcePlacement.Y;
		var oldRotated = sourcePlacement.Rotated;
		RequestCloseItemStorageIfOwnerMoves( sourcePlacement.Item.InstanceId );

		var remove = sourceContainer.TryRemoveItem( sourcePlacement.Item.InstanceId, out var movedItem );
		if ( !remove.Success )
			return remove;

		var add = targetContainer.TryAddItemAt( movedItem, x, y, rotated, out _ );
		if ( add.Success )
			return add;

		var rollback = sourceContainer.TryAddItemAt( movedItem, oldX, oldY, oldRotated, out _ );
		return rollback.Success
			? add
			: InventoryActionResult.Fail( $"{add.Reason} Rollback failed: {rollback.Reason}" );
	}

	InventoryActionResult TryMoveFromEquipmentSlot( InventoryEquipmentSlot slot, InventoryContainer targetContainer, int x, int y, bool rotated )
	{
		var item = Inventory?.Loadout?.GetEquipped( slot );

		if ( item is null )
			return InventoryActionResult.Fail( $"{slot} is empty." );

		if ( Inventory.IsStorageContainerForSlot( slot, targetContainer ) )
			return InventoryActionResult.Fail( $"{item.DisplayName} cannot be stored inside itself." );

		var canAdd = targetContainer.CanAddItemAt( item, x, y, rotated );
		if ( !canAdd.Success )
			return canAdd;

		Inventory.SaveStorageContainerForSlot( slot );
		var remove = Inventory.Loadout.TryUnequip( slot, out var movedItem );
		if ( !remove.Success )
			return remove;

		var add = targetContainer.TryAddItemAt( movedItem, x, y, rotated, out _ );
		if ( add.Success )
		{
			Inventory.EnsureInitialized();
			return add;
		}

		var rollback = Inventory.Loadout.TryEquip( slot, movedItem );
		return rollback.Success
			? add
			: InventoryActionResult.Fail( $"{add.Reason} Rollback failed: {rollback.Reason}" );
	}

	InventoryActionResult TryMoveFromEquipmentSlotToContainer( InventoryEquipmentSlot slot, InventoryContainer targetContainer )
	{
		var item = Inventory?.Loadout?.GetEquipped( slot );
		if ( item is null )
			return InventoryActionResult.Fail( $"{slot} is empty." );

		if ( Inventory.IsStorageContainerForSlot( slot, targetContainer ) )
			return InventoryActionResult.Fail( $"{item.DisplayName} cannot be stored inside itself." );

		Inventory.SaveStorageContainerForSlot( slot );
		var remove = Inventory.Loadout.TryUnequip( slot, out var movedItem );
		if ( !remove.Success )
			return remove;

		var add = targetContainer.TryAddItem( movedItem, false, out _ );
		if ( add.Success )
		{
			Inventory.EnsureInitialized();
			return add;
		}

		var rollback = Inventory.Loadout.TryEquip( slot, movedItem );
		return rollback.Success
			? add
			: InventoryActionResult.Fail( $"{add.Reason} Rollback failed: {rollback.Reason}" );
	}

	InventoryActionResult TryMoveFromContainerSourceToWeaponSlot( InventoryContainer sourceContainer, InventoryItemPlacement sourcePlacement )
	{
		if ( Inventory?.Loadout is null || !Inventory.Loadout.IsValid() )
			return InventoryActionResult.Fail( "No loadout component." );

		var targetSlot = Inventory.FindFirstAvailableWeaponSlot( sourcePlacement.Item );
		if ( !targetSlot.HasValue )
			return InventoryActionResult.Fail( $"{sourcePlacement.Item.DisplayName} cannot be equipped to On Sling or On Back." );

		var oldX = sourcePlacement.X;
		var oldY = sourcePlacement.Y;
		var oldRotated = sourcePlacement.Rotated;
		RequestCloseItemStorageIfOwnerMoves( sourcePlacement.Item.InstanceId );

		var remove = sourceContainer.TryRemoveItem( sourcePlacement.Item.InstanceId, out var movedItem );
		if ( !remove.Success )
			return remove;

		var equip = Inventory.Loadout.TryEquip( targetSlot.Value, movedItem );
		if ( equip.Success )
			return equip;

		var rollback = sourceContainer.TryAddItemAt( movedItem, oldX, oldY, oldRotated, out _ );
		return rollback.Success
			? equip
			: InventoryActionResult.Fail( $"{equip.Reason} Rollback failed: {rollback.Reason}" );
	}

	InventoryActionResult TryMoveFromContainerSourceToEquipmentSlot( InventoryContainer sourceContainer, InventoryItemPlacement sourcePlacement, InventoryEquipmentSlot slot )
	{
		if ( Inventory?.Loadout is null || !Inventory.Loadout.IsValid() )
			return InventoryActionResult.Fail( "No loadout component." );

		var item = sourcePlacement?.Item;
		if ( item is null || !item.IsValid )
			return InventoryActionResult.Fail( "Item is invalid." );

		if ( Inventory.Loadout.IsOccupied( slot ) )
			return InventoryActionResult.Fail( $"{slot} is already occupied." );

		if ( !item.Definition.CanEquipTo( slot ) )
			return InventoryActionResult.Fail( $"{item.DisplayName} cannot be equipped in {slot}." );

		var oldX = sourcePlacement.X;
		var oldY = sourcePlacement.Y;
		var oldRotated = sourcePlacement.Rotated;
		RequestCloseItemStorageIfOwnerMoves( item.InstanceId );

		var remove = sourceContainer.TryRemoveItem( item.InstanceId, out var movedItem );
		if ( !remove.Success )
			return remove;

		var equip = Inventory.Loadout.TryEquip( slot, movedItem );
		if ( equip.Success )
		{
			Inventory.EnsureInitialized();
			return equip;
		}

		var rollback = sourceContainer.TryAddItemAt( movedItem, oldX, oldY, oldRotated, out _ );
		return rollback.Success
			? equip
			: InventoryActionResult.Fail( $"{equip.Reason} Rollback failed: {rollback.Reason}" );
	}

	InventoryActionResult TryDropFromContainerSource( InventoryContainer sourceContainer, InventoryItemPlacement sourcePlacement, Transform transform, out WorldItemComponent worldItem )
	{
		worldItem = null;

		if ( sourceContainer is null || sourcePlacement is null )
			return InventoryActionResult.Fail( "No container item selected." );

		var oldX = sourcePlacement.X;
		var oldY = sourcePlacement.Y;
		var oldRotated = sourcePlacement.Rotated;
		RequestCloseItemStorageIfOwnerMoves( sourcePlacement.Item.InstanceId );

		var remove = sourceContainer.TryRemoveItem( sourcePlacement.Item.InstanceId, out var droppedItem );
		if ( !remove.Success )
			return remove;

		worldItem = WorldItemComponent.SpawnDropped( Scene, transform, droppedItem );
		if ( worldItem is not null && worldItem.IsValid() )
			return InventoryActionResult.Ok( $"{droppedItem.DisplayName} dropped to world." );

		var rollback = sourceContainer.TryAddItemAt( droppedItem, oldX, oldY, oldRotated, out _ );
		return rollback.Success
			? InventoryActionResult.Fail( "Could not create dropped WorldItem." )
			: InventoryActionResult.Fail( $"Could not create dropped WorldItem. Rollback failed: {rollback.Reason}" );
	}

	InventoryActionResult TryAutoMoveFromContainerSourceToContainers( InventoryContainer sourceContainer, InventoryItemPlacement sourcePlacement, IEnumerable<InventoryContainer> targetContainers )
	{
		if ( sourceContainer is null || sourcePlacement is null )
			return InventoryActionResult.Fail( "No container item selected." );

		foreach ( var targetContainer in targetContainers )
		{
			if ( targetContainer is null || targetContainer == sourceContainer )
				continue;

			RequestCloseItemStorageIfOwnerMoves( sourcePlacement.Item.InstanceId );

			var oldX = sourcePlacement.X;
			var oldY = sourcePlacement.Y;
			var oldRotated = sourcePlacement.Rotated;

			var remove = sourceContainer.TryRemoveItem( sourcePlacement.Item.InstanceId, out var movedItem );
			if ( !remove.Success )
				return remove;

			var add = targetContainer.TryAddItem( movedItem, false, out _ );
			if ( add.Success )
				return add;

			sourceContainer.TryAddItemAt( movedItem, oldX, oldY, oldRotated, out _ );
		}

		return InventoryActionResult.Fail( $"{sourcePlacement.Item.DisplayName} could not be moved automatically." );
	}

	IEnumerable<InventoryContainer> GetBaseVisibleContainers()
	{
		if ( Inventory is not null && Inventory.IsValid() )
		{
			foreach ( var activeContainer in Inventory.ActiveContainers )
				yield return activeContainer;
		}

		if ( OpenedLootContainer is not null && OpenedLootContainer.IsValid() && IsLootContainerInUseRange( OpenedLootContainer ) )
		{
			OpenedLootContainer.EnsureInitialized();
			yield return OpenedLootContainer.Container;
		}
	}

	bool TryFindBaseVisibleContainerLocation( string itemId, out InventoryContainer container, out InventoryItemPlacement placement )
	{
		if ( TryFindPlayerContainerLocation( itemId, out container, out placement ) )
			return true;

		return TryFindOpenLootLocation( itemId, out container, out placement );
	}

	bool TryFindPlayerContainerLocation( string itemId, out InventoryContainer container, out InventoryItemPlacement placement )
	{
		container = null;
		placement = null;

		if ( Inventory is null || !Inventory.IsValid() )
			return false;

		foreach ( var activeContainer in Inventory.ActiveContainers )
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

	bool TryFindOpenLootLocation( string itemId, out InventoryContainer container, out InventoryItemPlacement placement )
	{
		container = null;
		placement = null;

		if ( OpenedLootContainer is null || !OpenedLootContainer.IsValid() || !IsLootContainerInUseRange( OpenedLootContainer ) )
			return false;

		OpenedLootContainer.EnsureInitialized();
		var candidate = OpenedLootContainer.Container.FindPlacement( itemId );
		if ( candidate is null )
			return false;

		container = OpenedLootContainer.Container;
		placement = candidate;
		return true;
	}

	bool TryFindOpenedItemStorageLocation( string itemId, out InventoryContainer container, out InventoryItemPlacement placement )
	{
		container = null;
		placement = null;

		if ( OpenedItemStorageContainer is null )
			return false;

		var candidate = OpenedItemStorageContainer.FindPlacement( itemId );
		if ( candidate is null )
			return false;

		container = OpenedItemStorageContainer;
		placement = candidate;
		return true;
	}

	bool IsOpenedItemStorageOwner( string itemId )
	{
		return !string.IsNullOrWhiteSpace( itemId )
			&& OpenedItemStorageItem is not null
			&& OpenedItemStorageItem.InstanceId == itemId;
	}

	void ClearOpenedItemStorage()
	{
		OpenedItemStorageItem = null;
		OpenedItemStorageContainer = null;
	}

	static bool IsComponentValid( Component component )
	{
		return component is not null && component.IsValid();
	}

	WorldItemComponent FindLookedAtWorldItem()
	{
		return LookedAtWorldItem;
	}

	LootContainerComponent FindLookedAtLootContainer()
	{
		return LookedAtLootContainer;
	}
}
