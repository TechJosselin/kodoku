using Kodoku.Lib.Items;
using Kodoku.Lib.Loadout;

[TestClass]
public sealed class LoadoutSlotRegistryTests
{
	[TestMethod]
	public void Get_ReturnsConfigForKnownSlot()
	{
		var config = LoadoutSlotRegistry.Get( InventoryEquipmentSlot.Backpack );

		Assert.IsNotNull( config );
		Assert.AreEqual( "Backpack", config.DisplayName );
	}

	[TestMethod]
	public void CanAccept_ValidatesItemKind()
	{
		var definition = new ItemDefinition
		{
			DisplayName = "Test Backpack",
			ItemKind    = InventoryItemKind.Backpack,
		};
		var item = new ItemInstance( definition );

		Assert.IsTrue( LoadoutSlotRegistry.CanAccept( InventoryEquipmentSlot.Backpack, item ) );
		Assert.IsFalse( LoadoutSlotRegistry.CanAccept( InventoryEquipmentSlot.Headwear, item ) );
	}

	[TestMethod]
	public void Get_ReturnsNullForUnknownSlot()
	{
		var config = LoadoutSlotRegistry.Get( (InventoryEquipmentSlot)999 );

		Assert.IsNull( config );
	}
}
