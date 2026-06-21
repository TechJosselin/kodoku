using System;
using System.Collections.Generic;
using System.Linq;
using Kodoku.Lib.Items;

namespace Kodoku.Lib.Inventory;

public sealed class InventoryContainer
{
	readonly List<InventoryItemPlacement> _placements = new();

	public string ContainerId { get; }
	public string DisplayName { get; }
	public InventoryContainerKind Kind { get; }
	public int Width { get; }
	public int Height { get; }

	public IReadOnlyList<InventoryItemPlacement> Placements => _placements;
	public bool IsEmpty => _placements.Count == 0;

	public InventoryContainer( string containerId, string displayName, InventoryContainerKind kind, int width, int height )
	{
		ContainerId = containerId;
		DisplayName = displayName;
		Kind = kind;
		Width = Math.Max( 1, width );
		Height = Math.Max( 1, height );
	}

	public InventoryItemPlacement FindPlacement( string itemId )
	{
		if ( string.IsNullOrWhiteSpace( itemId ) )
			return null;

		return _placements.FirstOrDefault( x => x.Item.InstanceId == itemId );
	}

	public InventoryItemPlacement GetPlacementAt( int x, int y, string ignoredItemId = null )
	{
		return _placements.FirstOrDefault( placement =>
			placement.Item.InstanceId != ignoredItemId && placement.ContainsCell( x, y ) );
	}

	public bool ContainsItem( string itemId )
	{
		return FindPlacement( itemId ) is not null;
	}

	public InventoryActionResult CanAddItemAt( ItemInstance item, int x, int y, bool rotated )
	{
		var validation = ValidateIncomingItem( item );
		if ( !validation.Success )
			return validation;

		var target = GetPlacementAt( x, y );
		if ( target is not null )
		{
			return CanStackAll( target.Item, item )
				? InventoryActionResult.Ok()
				: InventoryActionResult.Fail( $"Cell {x},{y} is occupied." );
		}

		return CanPlaceItem( item, x, y, rotated );
	}

	public InventoryActionResult CanMoveItemTo( string itemId, int x, int y, bool rotated )
	{
		var placement = FindPlacement( itemId );
		if ( placement is null )
			return InventoryActionResult.Fail( $"Item {itemId} is not in {DisplayName}." );

		var target = GetPlacementAt( x, y, itemId );
		if ( target is not null )
		{
			return CanStackAll( target.Item, placement.Item )
				? InventoryActionResult.Ok()
				: InventoryActionResult.Fail( $"Cell {x},{y} is occupied." );
		}

		return CanPlaceItem( placement.Item, x, y, rotated, itemId );
	}

	public InventoryActionResult TryAddItem( ItemInstance item, bool rotated, out InventoryItemPlacement placement )
	{
		placement = null;

		var validation = ValidateIncomingItem( item );
		if ( !validation.Success )
			return validation;

		var stackPlan = BuildStackPlan( item );
		var remaining = item.Quantity - stackPlan.Sum( x => x.Amount );

		var x = 0;
		var y = 0;
		var actualRotated = rotated;

		if ( remaining > 0 )
		{
			if ( !TryFindFirstFreePosition( item, rotated, out x, out y ) )
			{
				if ( !item.Definition.CanRotate || !TryFindFirstFreePosition( item, !rotated, out x, out y ) )
					return InventoryActionResult.Fail( $"{DisplayName} has no free space for {item.DisplayName}." );

				actualRotated = !rotated;
			}
		}

		ApplyStackPlan( stackPlan );

		if ( remaining <= 0 )
		{
			item.SetQuantity( 0 );
			return InventoryActionResult.Ok( $"{item.DisplayName} stacked into {DisplayName}." );
		}

		item.SetQuantity( remaining );
		placement = new InventoryItemPlacement( item, x, y, actualRotated );
		_placements.Add( placement );

		return InventoryActionResult.Ok( $"{item.DisplayName} added to {DisplayName}." );
	}

	public InventoryActionResult TryAddItemAt( ItemInstance item, int x, int y, bool rotated, out InventoryItemPlacement placement )
	{
		placement = null;

		var validation = CanAddItemAt( item, x, y, rotated );
		if ( !validation.Success )
			return validation;

		var target = GetPlacementAt( x, y );
		if ( target is not null )
		{
			target.Item.AddQuantity( item.Quantity );
			item.SetQuantity( 0 );
			return InventoryActionResult.Ok( $"{item.DisplayName} stacked into {target.Item.DisplayName}." );
		}

		placement = new InventoryItemPlacement( item, x, y, rotated );
		_placements.Add( placement );

		return InventoryActionResult.Ok( $"{item.DisplayName} moved to {DisplayName}." );
	}

	public InventoryActionResult TryMoveItem( string itemId, int x, int y, bool rotated )
	{
		var placement = FindPlacement( itemId );
		if ( placement is null )
			return InventoryActionResult.Fail( $"Item {itemId} is not in {DisplayName}." );

		var target = GetPlacementAt( x, y, itemId );
		if ( target is not null )
		{
			if ( !CanStackAll( target.Item, placement.Item ) )
				return InventoryActionResult.Fail( $"Cell {x},{y} is occupied." );

			target.Item.AddQuantity( placement.Item.Quantity );
			_placements.Remove( placement );
			placement.Item.SetQuantity( 0 );
			return InventoryActionResult.Ok( $"{target.Item.DisplayName} stack updated." );
		}

		var canPlace = CanPlaceItem( placement.Item, x, y, rotated, itemId );
		if ( !canPlace.Success )
			return canPlace;

		placement.MoveTo( x, y, rotated );
		return InventoryActionResult.Ok( $"{placement.Item.DisplayName} moved inside {DisplayName}." );
	}

	public InventoryActionResult TryRemoveItem( string itemId, out ItemInstance item )
	{
		item = null;

		var placement = FindPlacement( itemId );
		if ( placement is null )
			return InventoryActionResult.Fail( $"Item {itemId} is not in {DisplayName}." );

		_placements.Remove( placement );
		item = placement.Item;
		return InventoryActionResult.Ok( $"{item.DisplayName} removed from {DisplayName}." );
	}

	public InventoryActionResult TrySplitItem( string itemId, int splitQuantity, out ItemInstance splitItem )
	{
		splitItem = null;

		var placement = FindPlacement( itemId );
		if ( placement is null )
			return InventoryActionResult.Fail( $"Item {itemId} not found in {DisplayName}." );

		var item = placement.Item;

		if ( !item.Definition.IsStackable )
			return InventoryActionResult.Fail( $"{item.DisplayName} is not stackable." );

		if ( item.Quantity <= 1 )
			return InventoryActionResult.Fail( $"Cannot split {item.DisplayName}: only {item.Quantity} remaining." );

		if ( splitQuantity < 1 || splitQuantity >= item.Quantity )
			return InventoryActionResult.Fail( $"Split quantity {splitQuantity} is invalid for stack of {item.Quantity}." );

		item.RemoveQuantity( splitQuantity );
		splitItem = new ItemInstance( item.Definition, splitQuantity );

		return InventoryActionResult.Ok( $"{item.DisplayName} split: {item.Quantity} kept, {splitQuantity} held." );
	}

	public void ReturnSplitQuantity( string sourceItemId, int quantity )
	{
		var placement = FindPlacement( sourceItemId );
		placement?.Item.AddQuantity( quantity );
	}

	InventoryActionResult CanPlaceItem( ItemInstance item, int x, int y, bool rotated, string ignoredItemId = null )
	{
		if ( rotated && !item.Definition.CanRotate )
			return InventoryActionResult.Fail( $"{item.DisplayName} cannot be rotated." );

		var itemWidth = item.Definition.GetWidth( rotated );
		var itemHeight = item.Definition.GetHeight( rotated );

		if ( x < 0 || y < 0 || x + itemWidth > Width || y + itemHeight > Height )
			return InventoryActionResult.Fail( $"{item.DisplayName} does not fit inside {DisplayName}." );

		foreach ( var placement in _placements )
		{
			if ( placement.Item.InstanceId == ignoredItemId )
				continue;

			if ( RectsOverlap( x, y, itemWidth, itemHeight, placement.X, placement.Y, placement.Width, placement.Height ) )
				return InventoryActionResult.Fail( $"{item.DisplayName} overlaps {placement.Item.DisplayName}." );
		}

		return InventoryActionResult.Ok();
	}

	InventoryActionResult ValidateIncomingItem( ItemInstance item )
	{
		if ( item is null || !item.IsValid )
			return InventoryActionResult.Fail( "Item is missing or invalid." );

		return InventoryActionResult.Ok();
	}

	bool TryFindFirstFreePosition( ItemInstance item, bool rotated, out int x, out int y )
	{
		x = 0;
		y = 0;

		for ( var cy = 0; cy < Height; cy++ )
		{
			for ( var cx = 0; cx < Width; cx++ )
			{
				if ( CanPlaceItem( item, cx, cy, rotated ).Success )
				{
					x = cx;
					y = cy;
					return true;
				}
			}
		}

		return false;
	}

	List<(InventoryItemPlacement Placement, int Amount)> BuildStackPlan( ItemInstance item )
	{
		var plan = new List<(InventoryItemPlacement Placement, int Amount)>();

		if ( item.Definition is null || !item.Definition.IsStackable )
			return plan;

		var remaining = item.Quantity;
		foreach ( var placement in _placements )
		{
			if ( !placement.Item.CanStackWith( item ) )
				continue;

			var free = placement.Item.Definition.GetMaxStack() - placement.Item.Quantity;
			if ( free <= 0 )
				continue;

			var amount = Math.Min( remaining, free );
			plan.Add( (placement, amount) );
			remaining -= amount;

			if ( remaining <= 0 )
				break;
		}

		return plan;
	}

	void ApplyStackPlan( List<(InventoryItemPlacement Placement, int Amount)> plan )
	{
		foreach ( var entry in plan )
		{
			entry.Placement.Item.AddQuantity( entry.Amount );
		}
	}

	static bool CanStackAll( ItemInstance target, ItemInstance incoming )
	{
		if ( !target.CanStackWith( incoming ) )
			return false;

		return target.Quantity + incoming.Quantity <= target.Definition.GetMaxStack();
	}

	static bool RectsOverlap( int ax, int ay, int aw, int ah, int bx, int by, int bw, int bh )
	{
		return ax < bx + bw
			&& ax + aw > bx
			&& ay < by + bh
			&& ay + ah > by;
	}
}
