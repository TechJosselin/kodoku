using Sandbox;

namespace Kodoku.Lib.Inventory;

[Title( "Inventory Bootstrapper" )]
[Category( "Inventory" )]
[Icon( "inventory_2" )]
public class InventoryBootstrapper : Component
{
	[Property] public InventoryComponent Inventory { get; set; }

	protected override void OnStart()
	{
		ResolveInventory();
	}

	protected InventoryComponent ResolveInventory()
	{
		if ( Inventory is null || !Inventory.IsValid() )
			Inventory = Components.Get<InventoryComponent>();

		Inventory?.EnsureInitialized();
		return Inventory;
	}
}
