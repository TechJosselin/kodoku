using System;
using Sandbox;
using Kodoku.Lib.Items;
using Kodoku.Lib.Inventory;

namespace Kodoku.Lib.Loadout;

[Title( "Player Loadout Component" )]
[Category( "Kodoku/Inventory" )]
[Icon( "checkroom" )]
public sealed class LoadoutComponent : Component
{
	readonly Dictionary<InventoryEquipmentSlot, ItemInstance> _equipped = new();

	[Property] public string HeadwearIconPath { get; set; } = "";
	[Property] public string GasMaskIconPath { get; set; } = "";
	[Property] public string BodyArmorIconPath { get; set; } = "";
	[Property] public string TacticalRigIconPath { get; set; } = "";
	[Property] public string BackpackIconPath { get; set; } = "";
	[Property] public string PantsIconPath { get; set; } = "";
	[Property] public string FootwearIconPath { get; set; } = "";
	[Property] public string OnSlingIconPath { get; set; } = "";
	[Property] public string OnBackIconPath { get; set; } = "";

	[Property] public bool LogResults { get; set; } = true;

	public Action<InventoryEquipmentSlot, ItemInstance> OnItemEquipped;
	public Action<InventoryEquipmentSlot, ItemInstance> OnItemUnequipped;

	public ItemInstance GetEquipped( InventoryEquipmentSlot slot )
		=> _equipped.TryGetValue( slot, out var item ) ? item : null;

	public bool IsOccupied( InventoryEquipmentSlot slot )
		=> GetEquipped( slot ) is not null;

	public InventoryEquipmentSlot? FindItemSlot( string itemId )
	{
		if ( string.IsNullOrWhiteSpace( itemId ) )
			return null;

		foreach ( var (slot, item) in _equipped )
			if ( item?.InstanceId == itemId )
				return slot;

		return null;
	}

	public string GetSlotEmptyIcon( InventoryEquipmentSlot slot )
	{
		var overridePath = GetSlotIconOverride( slot );
		if ( !string.IsNullOrWhiteSpace( overridePath ) )
			return overridePath;

		return LoadoutSlotRegistry.Get( slot )?.DefaultEmptyIconPath ?? ItemDefinition.DefaultIconPath;
	}

	string GetSlotIconOverride( InventoryEquipmentSlot slot )
	{
		return slot switch
		{
			InventoryEquipmentSlot.Headwear    => HeadwearIconPath,
			InventoryEquipmentSlot.GasMask     => GasMaskIconPath,
			InventoryEquipmentSlot.BodyArmor   => BodyArmorIconPath,
			InventoryEquipmentSlot.TacticalRig => TacticalRigIconPath,
			InventoryEquipmentSlot.Backpack    => BackpackIconPath,
			InventoryEquipmentSlot.Pants       => PantsIconPath,
			InventoryEquipmentSlot.Footwear    => FootwearIconPath,
			InventoryEquipmentSlot.OnSling     => OnSlingIconPath,
			InventoryEquipmentSlot.OnBack      => OnBackIconPath,
			_                                  => "",
		};
	}

	public InventoryActionResult TryEquip( InventoryEquipmentSlot slot, ItemInstance item )
	{
		if ( item is null || !item.IsValid )
			return LogResult( InventoryActionResult.Fail( "Item is invalid." ) );

		if ( IsOccupied( slot ) )
			return LogResult( InventoryActionResult.Fail( $"{SlotName( slot )} is already occupied." ) );

		if ( !LoadoutSlotRegistry.CanAccept( slot, item ) )
			return LogResult( InventoryActionResult.Fail( $"{item.DisplayName} cannot be equipped in {SlotName( slot )}." ) );

		_equipped[slot] = item;
		OnItemEquipped?.Invoke( slot, item );
		return LogResult( InventoryActionResult.Ok( $"{item.DisplayName} equipped to {SlotName( slot )}." ) );
	}

	public InventoryActionResult TryUnequip( InventoryEquipmentSlot slot, out ItemInstance item )
	{
		item = GetEquipped( slot );
		if ( item is null )
			return LogResult( InventoryActionResult.Fail( $"{SlotName( slot )} is empty." ) );

		_equipped.Remove( slot );
		OnItemUnequipped?.Invoke( slot, item );
		return LogResult( InventoryActionResult.Ok( $"{item.DisplayName} unequipped from {SlotName( slot )}." ) );
	}

	public InventoryActionResult TrySwap( InventoryEquipmentSlot from, InventoryEquipmentSlot to )
	{
		var fromItem = GetEquipped( from );
		if ( fromItem is null )
			return LogResult( InventoryActionResult.Fail( $"{SlotName( from )} is empty." ) );

		var toItem = GetEquipped( to );

		if ( !LoadoutSlotRegistry.CanAccept( to, fromItem ) )
			return LogResult( InventoryActionResult.Fail( $"{fromItem.DisplayName} cannot go in {SlotName( to )}." ) );

		if ( toItem is not null && !LoadoutSlotRegistry.CanAccept( from, toItem ) )
			return LogResult( InventoryActionResult.Fail( $"{toItem.DisplayName} cannot go back in {SlotName( from )}." ) );

		_equipped.Remove( from );
		_equipped.Remove( to );
		_equipped[to] = fromItem;
		if ( toItem is not null )
			_equipped[from] = toItem;

		return LogResult( InventoryActionResult.Ok( $"Moved {fromItem.DisplayName} to {SlotName( to )}." ) );
	}

	static string SlotName( InventoryEquipmentSlot slot )
		=> LoadoutSlotRegistry.Get( slot )?.DisplayName ?? "Unknown Slot";

	InventoryActionResult LogResult( InventoryActionResult result )
	{
		if ( LogResults )
			Log.Info( result.ToString() );
		return result;
	}
}
