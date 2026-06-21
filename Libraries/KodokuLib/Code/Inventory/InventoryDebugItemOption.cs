namespace Kodoku.Lib.Inventory;

public sealed class InventoryDebugItemOption
{
	public string DisplayName { get; }
	public string ResourcePath { get; }

	public InventoryDebugItemOption( string displayName, string resourcePath )
	{
		DisplayName = displayName;
		ResourcePath = resourcePath;
	}
}
