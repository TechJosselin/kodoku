using Kodoku.Lib.Items;

[TestClass]
public sealed class ItemDefinitionTests
{
	[TestMethod]
	public void NewDefinitionUsesSafeMetadataDefaults()
	{
		var definition = new ItemDefinition();

		Assert.AreEqual( "", definition.Description );
		Assert.AreEqual( "", definition.IconPath );
		Assert.AreEqual( "", definition.ModelPath );
		Assert.AreEqual( "", definition.PrefabPath );
	}

	[TestMethod]
	public void StorageWidthIsClampedToMaximum()
	{
		var definition = new ItemDefinition
		{
			StorageWidth = ItemDefinition.MaxStorageWidth + 2,
			StorageHeight = 4
		};

		Assert.AreEqual( ItemDefinition.MaxStorageWidth, definition.StorageWidth );
	}
}
