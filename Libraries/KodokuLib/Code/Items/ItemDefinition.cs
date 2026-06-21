using System;
using Sandbox;

namespace Kodoku.Lib.Items;

[AssetType( Name = "Inventory Item", Extension = "item", Category = "Inventory" )]
public sealed class ItemDefinition : GameResource
{
	public const string DefaultIconPath = "ui/game/icons/system/default/icon_default.png";
	public const int MaxStorageWidth = 8;

	int _storageWidth;

	[Property] public string ItemId { get; set; } = "";
	[Property] public string DisplayName { get; set; } = "Item";
	[Property] public string Description { get; set; } = "";
	[Property] public string IconPath { get; set; } = "";
	[Property] public string ModelPath { get; set; } = "";
	[Property] public string PrefabPath { get; set; } = "";
	[Property] public InventoryItemKind ItemKind { get; set; } = InventoryItemKind.Simple;

	[Property] public int Width { get; set; } = 1;
	[Property] public int Height { get; set; } = 1;
	[Property] public bool CanRotate { get; set; }

	[Property] public bool IsStackable { get; set; }
	[Property] public int MaxStack { get; set; } = 1;
	[Property] public float Weight { get; set; } = 0.1f;

	[Property]
	public int StorageWidth
	{
		get => Math.Clamp( _storageWidth, 0, MaxStorageWidth );
		set => _storageWidth = Math.Clamp( value, 0, MaxStorageWidth );
	}

	[Property] public int StorageHeight { get; set; } = 0;

	public bool CreatesContainer => StorageWidth > 0 && StorageHeight > 0;

	public bool CanEquipTo( InventoryEquipmentSlot slot )
	{
		return slot switch
		{
			InventoryEquipmentSlot.Backpack    => ItemKind == InventoryItemKind.Backpack,
			InventoryEquipmentSlot.TacticalRig => ItemKind == InventoryItemKind.TacticalRig,
			InventoryEquipmentSlot.Pants       => ItemKind == InventoryItemKind.Pants,
			InventoryEquipmentSlot.Headwear    => ItemKind == InventoryItemKind.Headwear,
			InventoryEquipmentSlot.GasMask     => ItemKind == InventoryItemKind.GasMask,
			InventoryEquipmentSlot.BodyArmor   => ItemKind == InventoryItemKind.BodyArmor,
			InventoryEquipmentSlot.OnSling     => ItemKind == InventoryItemKind.Weapon,
			InventoryEquipmentSlot.OnBack      => ItemKind == InventoryItemKind.Weapon,
			InventoryEquipmentSlot.Footwear    => ItemKind == InventoryItemKind.Footwear,
			_                                  => false,
		};
	}

	public int GetWidth( bool rotated )
	{
		return rotated && CanRotate ? Math.Max( 1, Height ) : Math.Max( 1, Width );
	}

	public int GetHeight( bool rotated )
	{
		return rotated && CanRotate ? Math.Max( 1, Width ) : Math.Max( 1, Height );
	}

	public int GetMaxStack()
	{
		return IsStackable ? Math.Max( 1, MaxStack ) : 1;
	}

	public string GetStableId()
	{
		return string.IsNullOrWhiteSpace( ItemId ) ? DisplayName : ItemId;
	}

	public string GetIconPath()
	{
		return string.IsNullOrWhiteSpace( IconPath ) ? DefaultIconPath : IconPath;
	}
}
