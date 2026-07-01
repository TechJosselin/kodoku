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

	[TestMethod]
	public void GetMaxStack_ReturnsOne_WhenNotStackable()
	{
		var definition = new ItemDefinition { IsStackable = false, MaxStack = 99 };

		Assert.AreEqual( 1, definition.GetMaxStack() );
	}

	[TestMethod]
	public void GetMaxStack_ReturnsMaxStack_WhenStackable()
	{
		var definition = new ItemDefinition { IsStackable = true, MaxStack = 4 };

		Assert.AreEqual( 4, definition.GetMaxStack() );
	}

	[TestMethod]
	public void CreatesContainer_TrueWhenBothDimensionsPositive()
	{
		var definition = new ItemDefinition { StorageWidth = 4, StorageHeight = 8 };

		Assert.IsTrue( definition.CreatesContainer );
	}

	[TestMethod]
	public void CreatesContainer_FalseWhenEitherDimensionIsZero()
	{
		Assert.IsFalse( new ItemDefinition { StorageWidth = 4, StorageHeight = 0 }.CreatesContainer );
		Assert.IsFalse( new ItemDefinition { StorageWidth = 0, StorageHeight = 8 }.CreatesContainer );
	}

	[TestMethod]
	public void GetWidth_ReturnsHeight_WhenRotatedAndCanRotate()
	{
		var definition = new ItemDefinition { Width = 5, Height = 2, CanRotate = true };

		Assert.AreEqual( 2, definition.GetWidth( rotated: true ) );
		Assert.AreEqual( 5, definition.GetHeight( rotated: true ) );
	}

	[TestMethod]
	public void GetWidth_IgnoresRotation_WhenCanRotateFalse()
	{
		var definition = new ItemDefinition { Width = 5, Height = 2, CanRotate = false };

		Assert.AreEqual( 5, definition.GetWidth( rotated: true ) );
		Assert.AreEqual( 2, definition.GetHeight( rotated: true ) );
	}

	[TestMethod]
	public void GetIconPath_ReturnsDefaultIconPath_WhenEmpty()
	{
		var definition = new ItemDefinition { IconPath = "" };

		Assert.AreEqual( ItemDefinition.DefaultIconPath, definition.GetIconPath() );
	}

	[TestMethod]
	public void GetIconPath_ReturnsSetPath_WhenNotEmpty()
	{
		const string path = "ui/game/icons/items/consumables/medical/icon_bandage.png";
		var definition = new ItemDefinition { IconPath = path };

		Assert.AreEqual( path, definition.GetIconPath() );
	}

	[TestMethod]
	public void HasUseEffects_FalseWhenAllDeltasAreZero()
	{
		var definition = new ItemDefinition { IsUsable = true };

		Assert.IsFalse( definition.HasUseEffects() );
	}

	[TestMethod]
	public void HasUseEffects_TrueWhenHealthDeltaNonZero()
	{
		var definition = new ItemDefinition { IsUsable = true, UseHealthDelta = 20f };

		Assert.IsTrue( definition.HasUseEffects() );
	}

	[TestMethod]
	public void HasUseEffects_TrueWhenThirstDeltaNonZero()
	{
		var definition = new ItemDefinition { IsUsable = true, UseThirstDelta = 35f };

		Assert.IsTrue( definition.HasUseEffects() );
	}

	[TestMethod]
	public void IsUsable_DefaultsToFalse()
	{
		var definition = new ItemDefinition();

		Assert.IsFalse( definition.IsUsable );
	}

	[TestMethod]
	public void ConsumeOnUse_DefaultsToTrue()
	{
		var definition = new ItemDefinition();

		Assert.IsTrue( definition.ConsumeOnUse );
	}

	[TestMethod]
	public void UseQuantity_DefaultsToOne()
	{
		var definition = new ItemDefinition();

		Assert.AreEqual( 1, definition.UseQuantity );
	}
}
