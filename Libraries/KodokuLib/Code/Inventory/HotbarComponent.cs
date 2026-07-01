using Sandbox;
using Kodoku.Lib.Items;

namespace Kodoku.Lib.Inventory;

[Title( "Hotbar" )]
[Category( "Kodoku/Inventory" )]
[Icon( "keyboard" )]
public sealed class HotbarComponent : Component
{
	public const int SlotCount = 8;

	[Property] public bool HandleNumberInput { get; set; } = true;
	[Property] public bool LogActionResults { get; set; } = true;

	public int ActiveSlotIndex { get; private set; } = -1;
	public string ActiveItemId { get; private set; }

	InventoryComponent _inventory;
	readonly string[] _slots = new string[SlotCount];

	public void BindInventory( InventoryComponent inventory )
	{
		_inventory = inventory;
	}

	public string GetItemId( int index )
	{
		if ( index < 0 || index >= SlotCount )
			return null;

		return _slots[index];
	}

	public ItemInstance GetItem( int index )
	{
		var id = GetItemId( index );
		if ( string.IsNullOrWhiteSpace( id ) )
			return null;

		var item = _inventory?.FindOwnedItem( id );
		if ( item is null )
		{
			_slots[index] = null;
			ClearActiveIfSlot( index );
			return null;
		}

		return item;
	}

	public InventoryActionResult Assign( int index, string itemId )
	{
		if ( index < 0 || index >= SlotCount )
			return InventoryActionResult.Fail( $"Invalid hotbar slot {index}." );

		if ( string.IsNullOrWhiteSpace( itemId ) )
			return InventoryActionResult.Fail( "No item specified." );

		if ( _inventory is null )
			return InventoryActionResult.Fail( "No inventory bound." );

		var item = _inventory.FindOwnedItem( itemId );
		if ( item is null )
			return InventoryActionResult.Fail( "Item is not owned by the player." );

		_slots[index] = itemId;
		return InventoryActionResult.Ok( $"{item.DisplayName} assigned to slot {index + 1}." );
	}

	public InventoryActionResult Clear( int index )
	{
		if ( index < 0 || index >= SlotCount )
			return InventoryActionResult.Fail( $"Invalid hotbar slot {index}." );

		_slots[index] = null;
		ClearActiveIfSlot( index );
		return InventoryActionResult.Ok( $"Slot {index + 1} cleared." );
	}

	public InventoryActionResult UseSlot( int index )
	{
		if ( index < 0 || index >= SlotCount )
			return InventoryActionResult.Fail( $"Invalid hotbar slot {index}." );

		var item = GetItem( index );
		if ( item is null )
			return InventoryActionResult.Fail( "Hotbar slot is empty." );

		if ( item.Definition.IsUsable )
			return _inventory?.TryUseItem( item.InstanceId ) ?? InventoryActionResult.Fail( "No inventory." );

		if ( item.Definition.ItemKind == InventoryItemKind.Weapon )
		{
			if ( _inventory?.Loadout?.FindItemSlot( item.InstanceId ).HasValue == true )
			{
				ActiveSlotIndex = index;
				ActiveItemId = item.InstanceId;
				return InventoryActionResult.Ok( $"{item.DisplayName} selected." );
			}

			return InventoryActionResult.Fail( $"{item.DisplayName} must be equipped before use." );
		}

		return InventoryActionResult.Fail( $"{item.DisplayName} has no use action yet." );
	}

	void ClearActiveIfSlot( int index )
	{
		if ( index == ActiveSlotIndex )
		{
			ActiveSlotIndex = -1;
			ActiveItemId = null;
		}
	}

	protected override void OnUpdate()
	{
		if ( !HandleNumberInput )
			return;

		for ( var i = 0; i < SlotCount; i++ )
		{
			if ( Input.Keyboard.Pressed( $"{i + 1}" ) )
			{
				var result = UseSlot( i );
				if ( LogActionResults )
					Log.Info( result.ToString() );
				return;
			}
		}
	}
}
