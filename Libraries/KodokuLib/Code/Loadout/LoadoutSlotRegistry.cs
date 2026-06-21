using System.Collections.Generic;
using Kodoku.Lib.AssetPaths;
using Kodoku.Lib.Items;

namespace Kodoku.Lib.Loadout;

/// Immutable registry of all loadout slot definitions.
/// Groups define rendering order on the paperdoll.
public static class LoadoutSlotRegistry
{
	static readonly Dictionary<InventoryEquipmentSlot, LoadoutSlotConfig> _all = new()
	{
		[InventoryEquipmentSlot.Headwear] = new()
		{
			Slot                 = InventoryEquipmentSlot.Headwear,
			DisplayName          = "Headwear",
			AcceptedKinds        = new[] { InventoryItemKind.Headwear },
			DefaultEmptyIconPath = KodokuUiAssetPaths.SlotHeadwear,
			IconVariant          = "md",
		},
		[InventoryEquipmentSlot.GasMask] = new()
		{
			Slot                 = InventoryEquipmentSlot.GasMask,
			DisplayName          = "Gas Mask",
			AcceptedKinds        = new[] { InventoryItemKind.GasMask },
			DefaultEmptyIconPath = KodokuUiAssetPaths.SlotFaceCover,
			IconVariant          = "md",
		},
		[InventoryEquipmentSlot.BodyArmor] = new()
		{
			Slot                 = InventoryEquipmentSlot.BodyArmor,
			DisplayName          = "Body Armor",
			AcceptedKinds        = new[] { InventoryItemKind.BodyArmor },
			DefaultEmptyIconPath = KodokuUiAssetPaths.SlotBodyArmor,
			IconVariant          = "lg",
		},
		[InventoryEquipmentSlot.TacticalRig] = new()
		{
			Slot                 = InventoryEquipmentSlot.TacticalRig,
			DisplayName          = "Tactical Rig",
			AcceptedKinds        = new[] { InventoryItemKind.TacticalRig },
			DefaultEmptyIconPath = KodokuUiAssetPaths.SlotTacticalRig,
			IconVariant          = "lg",
		},
		[InventoryEquipmentSlot.Backpack] = new()
		{
			Slot                 = InventoryEquipmentSlot.Backpack,
			DisplayName          = "Backpack",
			AcceptedKinds        = new[] { InventoryItemKind.Backpack },
			DefaultEmptyIconPath = KodokuUiAssetPaths.SlotBackpack,
			IconVariant          = "lg",
		},
		[InventoryEquipmentSlot.Pants] = new()
		{
			Slot                 = InventoryEquipmentSlot.Pants,
			DisplayName          = "Pants",
			AcceptedKinds        = new[] { InventoryItemKind.Pants },
			DefaultEmptyIconPath = KodokuUiAssetPaths.SlotPants,
			IconVariant          = "md",
		},
		[InventoryEquipmentSlot.Footwear] = new()
		{
			Slot                 = InventoryEquipmentSlot.Footwear,
			DisplayName          = "Footwear",
			AcceptedKinds        = new[] { InventoryItemKind.Footwear },
			DefaultEmptyIconPath = KodokuUiAssetPaths.SlotFootwear,
			IconVariant          = "md",
		},
		[InventoryEquipmentSlot.OnSling] = new()
		{
			Slot                 = InventoryEquipmentSlot.OnSling,
			DisplayName          = "On Sling",
			AcceptedKinds        = new[] { InventoryItemKind.Weapon },
			DefaultEmptyIconPath = KodokuUiAssetPaths.SlotWeapon,
			IconVariant          = "wide",
		},
		[InventoryEquipmentSlot.OnBack] = new()
		{
			Slot                 = InventoryEquipmentSlot.OnBack,
			DisplayName          = "On Back",
			AcceptedKinds        = new[] { InventoryItemKind.Weapon },
			DefaultEmptyIconPath = KodokuUiAssetPaths.SlotWeapon,
			IconVariant          = "wide",
		},
	};

	public static readonly InventoryEquipmentSlot[] HeadSlots =
	{
		InventoryEquipmentSlot.Headwear,
		InventoryEquipmentSlot.GasMask,
	};

	public static readonly InventoryEquipmentSlot[] BodySlots =
	{
		InventoryEquipmentSlot.BodyArmor,
	};

	public static readonly InventoryEquipmentSlot[] LowerBodySlots =
	{
		InventoryEquipmentSlot.Pants,
		InventoryEquipmentSlot.Footwear,
	};

	public static readonly InventoryEquipmentSlot[] WeaponSlots =
	{
		InventoryEquipmentSlot.OnSling,
		InventoryEquipmentSlot.OnBack,
	};

	public static readonly InventoryEquipmentSlot[] LargeRightSlots =
	{
		InventoryEquipmentSlot.TacticalRig,
		InventoryEquipmentSlot.Backpack,
	};

	public static LoadoutSlotConfig Get( InventoryEquipmentSlot slot )
		=> _all.TryGetValue( slot, out var config ) ? config : null;

	public static bool CanAccept( InventoryEquipmentSlot slot, ItemInstance item )
		=> Get( slot )?.CanAccept( item ) ?? false;
}
