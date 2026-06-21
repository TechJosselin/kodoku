using Kodoku.Lib.GameMenu;

[TestClass]
public sealed class GameMenuStateTests
{
	// --- GameMenuState ---

	[TestMethod]
	public void StartsOnInventoryTab()
	{
		var state = new GameMenuState();

		Assert.AreEqual( GameMenuTab.Inventory, state.ActiveTab );
	}

	[TestMethod]
	public void CanChangeActiveTab()
	{
		var state = new GameMenuState();

		state.SetActiveTab( GameMenuTab.Map );

		Assert.AreEqual( GameMenuTab.Map, state.ActiveTab );
	}

	[TestMethod]
	public void InitializesNavigationAndInventorySubStates()
	{
		var state = new GameMenuState();

		Assert.IsNotNull( state.Navigation );
		Assert.IsNotNull( state.Inventory );
		Assert.AreEqual( state.Navigation.ActiveTab, state.ActiveTab );
		Assert.AreEqual( state.Navigation.IsOpen, state.IsOpen );
	}

	// --- NavigationMenuState ---

	[TestMethod]
	public void Open_SetsIsOpenTrue()
	{
		var nav = new NavigationMenuState();

		nav.Open();

		Assert.IsTrue( nav.IsOpen );
	}

	[TestMethod]
	public void Close_SetsIsOpenFalse()
	{
		var nav = new NavigationMenuState();
		nav.Open();

		nav.Close();

		Assert.IsFalse( nav.IsOpen );
	}

	[TestMethod]
	public void Toggle_ClosesWhenOpen()
	{
		var nav = new NavigationMenuState();
		nav.Open();

		nav.Toggle();

		Assert.IsFalse( nav.IsOpen );
	}

	// --- InventoryMenuState ---

	[TestMethod]
	public void TracksHoverAndSelection()
	{
		var state = new InventoryMenuState();

		state.SetHover( "pockets", 1, 2 );
		state.SetSelection( "item-1" );

		Assert.IsTrue( state.HasHover );
		Assert.AreEqual( "pockets", state.HoveredContainerId );
		Assert.AreEqual( 1, state.HoveredX );
		Assert.AreEqual( 2, state.HoveredY );
		Assert.IsTrue( state.HasSelection );
		Assert.AreEqual( "item-1", state.SelectedItemId );

		state.ClearHover();
		state.ClearSelection();

		Assert.IsFalse( state.HasHover );
		Assert.IsFalse( state.HasSelection );
	}
}
