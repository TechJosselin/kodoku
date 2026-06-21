namespace Kodoku.Lib.Inventory;

public interface IInventoryDebugActions
{
	IReadOnlyList<InventoryDebugItemOption> GetDebugItems();
	InventoryActionResult AddItem( InventoryDebugItemOption option );
	InventoryActionResult AddSmall();
	InventoryActionResult AddLong();
	InventoryActionResult AddBackpack();
	InventoryActionResult SpawnBandage();
	InventoryActionResult SpawnWater();
	InventoryActionResult SpawnSmallBackpack();
}
