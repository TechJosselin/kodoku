using Kodoku.Lib.Items;
using Kodoku.Lib.Inventory;

[TestClass]
public sealed class InventoryContainerTests
{
	static ItemDefinition MakeDef( string name, int w = 1, int h = 1, bool stackable = false, int maxStack = 1 )
	{
		return new ItemDefinition
		{
			DisplayName = name,
			Width = w,
			Height = h,
			IsStackable = stackable,
			MaxStack = maxStack
		};
	}

	static InventoryContainer MakeContainer( int w = 4, int h = 4 )
	{
		return new InventoryContainer( "test", "Test", InventoryContainerKind.Pockets, w, h );
	}

	[TestMethod]
	public void TryAddItem_PlacesItemInGrid()
	{
		var container = MakeContainer();
		var item = new ItemInstance( MakeDef( "Widget" ) );

		var result = container.TryAddItem( item, false, out var placement );

		Assert.IsTrue( result.Success );
		Assert.IsNotNull( placement );
		Assert.AreEqual( 1, container.Placements.Count );
		Assert.AreSame( item, placement.Item );
	}

	[TestMethod]
	public void TryAddItem_FailsWhenNoSpace()
	{
		var container = MakeContainer( 2, 2 );
		var def = MakeDef( "Block", w: 2, h: 2 );

		container.TryAddItem( new ItemInstance( def ), false, out _ );
		var result = container.TryAddItem( new ItemInstance( def ), false, out var placement );

		Assert.IsFalse( result.Success );
		Assert.IsNull( placement );
	}

	[TestMethod]
	public void TryRemoveItem_RemovesPlacement()
	{
		var container = MakeContainer();
		var item = new ItemInstance( MakeDef( "Widget" ) );
		container.TryAddItem( item, false, out _ );

		var result = container.TryRemoveItem( item.InstanceId, out var removed );

		Assert.IsTrue( result.Success );
		Assert.AreSame( item, removed );
		Assert.AreEqual( 0, container.Placements.Count );
	}

	[TestMethod]
	public void TryRemoveItem_FailsWhenItemAbsent()
	{
		var container = MakeContainer();

		var result = container.TryRemoveItem( "nonexistent", out var removed );

		Assert.IsFalse( result.Success );
		Assert.IsNull( removed );
	}

	[TestMethod]
	public void CanAddItemAt_FailsOnOverlap()
	{
		var container = MakeContainer( 4, 1 );
		var def = MakeDef( "Bar", w: 2, h: 1 );

		container.TryAddItemAt( new ItemInstance( def ), 0, 0, false, out _ );
		var result = container.CanAddItemAt( new ItemInstance( def ), 1, 0, false );

		Assert.IsFalse( result.Success );
	}

	[TestMethod]
	public void TryAddItem_StacksIdenticalItems()
	{
		var container = MakeContainer();
		var def = MakeDef( "Bandage", stackable: true, maxStack: 5 );

		var first = new ItemInstance( def, 2 );
		container.TryAddItem( first, false, out _ );

		var second = new ItemInstance( def, 2 );
		var result = container.TryAddItem( second, false, out var placement );

		Assert.IsTrue( result.Success );
		Assert.IsNull( placement );
		Assert.AreEqual( 1, container.Placements.Count );
		Assert.AreEqual( 4, container.Placements[0].Item.Quantity );
	}

	[TestMethod]
	public void TrySplitItem_ProducesCorrectQuantities()
	{
		var container = MakeContainer();
		var def = MakeDef( "Bandage", stackable: true, maxStack: 10 );
		var item = new ItemInstance( def, 4 );
		container.TryAddItem( item, false, out _ );

		var result = container.TrySplitItem( item.InstanceId, 2, out var splitItem );

		Assert.IsTrue( result.Success );
		Assert.IsNotNull( splitItem );
		Assert.AreEqual( 2, item.Quantity );
		Assert.AreEqual( 2, splitItem.Quantity );
	}
}
