namespace Kodoku.Lib.Inventory;

public sealed class InventoryDebugItemOption
{
	public string DisplayName { get; }
	public string ResourcePath { get; }
	public string Category { get; }

	public InventoryDebugItemOption( string displayName, string resourcePath, string category = "" )
	{
		DisplayName = displayName;
		ResourcePath = resourcePath;
		Category = category;
	}
}
