using Kodoku.Lib.Items;

[TestClass]
public sealed class ItemDefinitionTests
{
	[TestMethod]
	public void StorageWidthIsClampedToSixSlots()
	{
		var definition = new ItemDefinition
		{
			StorageWidth = ItemDefinition.MaxStorageWidth + 2,
			StorageHeight = 4
		};

		Assert.AreEqual( ItemDefinition.MaxStorageWidth, definition.StorageWidth );
	}
}
