using System;
using Kodoku.Lib.Inventory;

namespace Kodoku.Lib.Items;

public sealed class ItemInstance
{
	public string InstanceId { get; private set; }
	public ItemDefinition Definition { get; private set; }
	public int Quantity { get; private set; }

	public ItemInstance( ItemDefinition definition, int quantity = 1, string instanceId = null )
	{
		Definition = definition;
		InstanceId = string.IsNullOrWhiteSpace( instanceId ) ? Guid.NewGuid().ToString( "N" ) : instanceId;
		Quantity = Math.Max( 1, quantity );
		ClampQuantityToDefinition();
	}

	public bool IsValid => Definition is not null && Quantity > 0;
	public string DisplayName => Definition?.DisplayName ?? "Missing Item";
	public bool CreatesContainer => Definition?.CreatesContainer ?? false;

	public bool HasSameDefinition( ItemInstance other )
	{
		if ( other?.Definition is null || Definition is null )
			return false;

		if ( ReferenceEquals( Definition, other.Definition ) )
			return true;

		return Definition.GetStableId() == other.Definition.GetStableId();
	}

	public bool CanStackWith( ItemInstance other )
	{
		return IsValid
			&& other is not null
			&& other.IsValid
			&& Definition.IsStackable
			&& other.Definition.IsStackable
			&& HasSameDefinition( other );
	}

	internal void SetQuantity( int quantity )
	{
		Quantity = Math.Max( 0, quantity );
	}

	internal void AddQuantity( int amount )
	{
		Quantity += Math.Max( 0, amount );
		ClampQuantityToDefinition();
	}

	internal void RemoveQuantity( int amount )
	{
		Quantity = Math.Max( 0, Quantity - Math.Max( 0, amount ) );
	}

	void ClampQuantityToDefinition()
	{
		if ( Definition is null )
			return;

		Quantity = Math.Clamp( Quantity, 1, Definition.GetMaxStack() );
	}

	InventoryContainer _storedContainer;

	internal InventoryContainer GetStoredContainer() => _storedContainer;

	internal InventoryContainer EnsureStoredContainer()
	{
		if ( Definition is null || !Definition.CreatesContainer )
			return null;

		_storedContainer ??= new InventoryContainer(
			$"item_{InstanceId}",
			Definition.DisplayName,
			InventoryContainerKind.Backpack,
			Definition.StorageWidth,
			Definition.StorageHeight );

		return _storedContainer;
	}

	internal void StoreContainer( InventoryContainer container )
	{
		_storedContainer = container;
	}
}
