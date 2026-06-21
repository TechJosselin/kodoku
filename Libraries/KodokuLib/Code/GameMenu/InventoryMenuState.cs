namespace Kodoku.Lib.GameMenu;

/// Tracks lightweight hover/selection bookkeeping for the Inventory tab UI.
/// Runtime component references (InventoryComponent, LoadoutComponent) and
/// active container tracking are deferred to the runtime wiring step.
public sealed class InventoryMenuState
{
	public string HoveredContainerId { get; private set; }
	public int HoveredX { get; private set; }
	public int HoveredY { get; private set; }
	public bool HasHover { get; private set; }

	public string SelectedItemId { get; private set; }
	public bool HasSelection => !string.IsNullOrWhiteSpace( SelectedItemId );

	public void SetHover( string containerId, int x, int y )
	{
		HoveredContainerId = containerId;
		HoveredX = x;
		HoveredY = y;
		HasHover = true;
	}

	public void ClearHover()
	{
		HoveredContainerId = null;
		HoveredX = 0;
		HoveredY = 0;
		HasHover = false;
	}

	public void SetSelection( string itemId )
	{
		SelectedItemId = itemId;
	}

	public void ClearSelection()
	{
		SelectedItemId = null;
	}
}
