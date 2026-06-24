namespace Kodoku.Lib.Inventory;

public interface IInventoryDebugActions
{
	IReadOnlyList<InventoryDebugItemOption> GetDebugItems();
	InventoryActionResult AddItem( InventoryDebugItemOption option, int quantity );
	InventoryActionResult SpawnItem( InventoryDebugItemOption option, int quantity );
	InventoryActionResult AddSmall();
	InventoryActionResult AddLong();
	InventoryActionResult AddBackpack();
	InventoryActionResult SpawnBandage();
	InventoryActionResult SpawnWater();
	InventoryActionResult SpawnSmallBackpack();
}
