using System;
using Kodoku.Lib.Items;

namespace Kodoku.Lib.Loadout;

public sealed class LoadoutSlotConfig
{
	public InventoryEquipmentSlot Slot { get; init; }
	public string DisplayName { get; init; } = "";
	public InventoryItemKind[] AcceptedKinds { get; init; } = Array.Empty<InventoryItemKind>();
	public string DefaultEmptyIconPath { get; init; } = ItemDefinition.DefaultIconPath;

	/// True = show a placeholder silhouette icon when empty. False = plain grid-cell style (no icon).
	public bool HasPlaceholderIcon { get; init; } = true;

	/// Icon size variant: "sm" (20px), "md" (32px), "lg" (46px), "wide" (50x22px).
	public string IconVariant { get; init; } = "md";

	public bool CanAccept( ItemInstance item )
	{
		if ( item is null || !item.IsValid )
			return false;

		foreach ( var kind in AcceptedKinds )
			if ( item.Definition.ItemKind == kind )
				return true;

		return false;
	}
}
