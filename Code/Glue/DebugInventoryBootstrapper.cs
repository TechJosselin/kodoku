using Sandbox;
using Kodoku.Lib.Inventory;
using Kodoku.Lib.Items;

namespace Kodoku.Glue;

[Title( "Debug Inventory Bootstrapper" )]
[Category( "Inventory/Debug" )]
[Icon( "bug_report" )]
public sealed class DebugInventoryBootstrapper : InventoryBootstrapper, IInventoryDebugActions
{
	const string DefaultBandageDefinitionPath = "demo/items/bandage.item";
	const string DefaultWaterDefinitionPath = "demo/items/water.item";
	const string DefaultSmallBackpackDefinitionPath = "demo/items/small_backpack.item";

	static readonly InventoryDebugItemOption[] DebugGiveItems =
	{
		new( "Assault Rifle",  "demo/items/assault_rifle.item" ),
		new( "Balaclava",      "demo/items/balaclava.item" ),
		new( "Bandage",        DefaultBandageDefinitionPath ),
		new( "Body Armor",     "demo/items/body_armor.item" ),
		new( "Boots",          "demo/items/boots.item" ),
		new( "Cargo Pants",    "demo/items/cargo_pants.item" ),
		new( "Helmet",         "demo/items/helmet.item" ),
		new( "Pistol",         "demo/items/pistol.item" ),
		new( "Small Backpack", DefaultSmallBackpackDefinitionPath ),
		new( "Tactical Rig",   "demo/items/tactical_rig.item" ),
		new( "Water Bottle",   DefaultWaterDefinitionPath ),
	};

	public static IReadOnlyList<InventoryDebugItemOption> DefaultDebugGiveItemOptions => DebugGiveItems;

	[Property] public ItemDefinition DebugSmallItemDefinition { get; set; }
	[Property] public ItemDefinition DebugLongItemDefinition { get; set; }
	[Property] public ItemDefinition DebugBackpackDefinition { get; set; }
	[Property] public ItemDefinition DebugBandageWorldDefinition { get; set; }
	[Property] public ItemDefinition DebugWaterWorldDefinition { get; set; }
	[Property] public ItemDefinition DebugSmallBackpackWorldDefinition { get; set; }

	[Property] public bool SeedDebugItemsOnStart { get; set; }
	[Property] public float DebugWorldSpawnDistance { get; set; } = 96f;
	[Property] public float DebugWorldSpawnUpOffset { get; set; } = 24f;

	protected override void OnStart()
	{
		ResolveInventory();

		if ( SeedDebugItemsOnStart )
		{
			LogResult( DebugAddSmallItem() );
			LogResult( DebugAddLongItem() );
			LogResult( DebugAddBackpack() );
		}
	}

	public InventoryActionResult DebugAddSmallItem()
	{
		return TryAddDebugItem( DebugSmallItemDefinition, DefaultBandageDefinitionPath );
	}

	public InventoryActionResult DebugAddLongItem()
	{
		return TryAddDebugItem( DebugLongItemDefinition, DefaultWaterDefinitionPath );
	}

	public InventoryActionResult DebugAddBackpack()
	{
		return TryAddDebugItem( DebugBackpackDefinition, DefaultSmallBackpackDefinitionPath );
	}

	public InventoryActionResult DebugAddItem( InventoryDebugItemOption option )
	{
		if ( option is null )
			return InventoryActionResult.Fail( "No debug item selected." );

		if ( option.ResourcePath == DefaultBandageDefinitionPath )
			return DebugAddSmallItem();

		if ( option.ResourcePath == DefaultWaterDefinitionPath )
			return DebugAddLongItem();

		if ( option.ResourcePath == DefaultSmallBackpackDefinitionPath )
			return DebugAddBackpack();

		return DebugAddItem( option.ResourcePath );
	}

	public InventoryActionResult DebugAddItem( string resourcePath )
	{
		if ( string.IsNullOrWhiteSpace( resourcePath ) )
			return InventoryActionResult.Fail( "No debug item resource path selected." );

		return TryAddDebugItem( null, resourcePath );
	}

	public InventoryActionResult DebugSpawnBandageWorldItem( out WorldItemComponent worldItem )
	{
		return DebugSpawnNamedWorldItem( DebugBandageWorldDefinition, DefaultBandageDefinitionPath, out worldItem );
	}

	public InventoryActionResult DebugSpawnWaterWorldItem( out WorldItemComponent worldItem )
	{
		return DebugSpawnNamedWorldItem( DebugWaterWorldDefinition, DefaultWaterDefinitionPath, out worldItem );
	}

	public InventoryActionResult DebugSpawnSmallBackpackWorldItem( out WorldItemComponent worldItem )
	{
		return DebugSpawnNamedWorldItem( DebugSmallBackpackWorldDefinition, DefaultSmallBackpackDefinitionPath, out worldItem );
	}

	InventoryActionResult DebugSpawnNamedWorldItem( ItemDefinition configuredDefinition, string defaultPath, out WorldItemComponent worldItem )
	{
		var definition = ResolveItemDefinition( configuredDefinition, defaultPath );
		if ( !TryValidateDebugDefinition( definition, defaultPath, out var missing ) )
		{
			worldItem = null;
			LogResult( missing );
			return missing;
		}

		return DebugSpawnWorldItem( new ItemInstance( definition, 1 ), out worldItem );
	}

	InventoryActionResult DebugSpawnWorldItem( ItemInstance item, out WorldItemComponent worldItem )
	{
		worldItem = null;

		if ( item is null || !item.IsValid )
			return InventoryActionResult.Fail( "Cannot spawn an invalid WorldItem." );

		worldItem = WorldItemComponent.SpawnDropped( Scene, GetDebugWorldSpawnTransform(), item );
		var result = InventoryActionResult.Ok( $"{item.DisplayName} spawned as WorldItem." );
		LogResult( result );
		return result;
	}

	InventoryActionResult TryAddDebugItem( ItemDefinition configuredDefinition, string defaultResourcePath )
	{
		if ( Inventory is null || !Inventory.IsValid() )
			return InventoryActionResult.Fail( "No InventoryComponent found." );

		var definition = ResolveItemDefinition( configuredDefinition, defaultResourcePath );
		if ( !TryValidateDebugDefinition( definition, defaultResourcePath, out var missing ) )
		{
			LogResult( missing );
			return missing;
		}

		return Inventory.TryAddItem( new ItemInstance( definition, 1 ) );
	}

	static ItemDefinition ResolveItemDefinition( ItemDefinition configuredDefinition, string defaultResourcePath )
	{
		return configuredDefinition ?? ResourceLibrary.Get<ItemDefinition>( defaultResourcePath );
	}

	static bool TryValidateDebugDefinition( ItemDefinition definition, string defaultResourcePath, out InventoryActionResult result )
	{
		if ( definition is not null )
		{
			result = InventoryActionResult.Ok();
			return true;
		}

		result = InventoryActionResult.Fail( $"Missing ItemDefinition asset: {defaultResourcePath}" );
		return false;
	}

	Transform GetDebugWorldSpawnTransform()
	{
		if ( Inventory is not null && Inventory.IsValid() )
		{
			var position = Inventory.WorldPosition
				+ Inventory.WorldRotation.Forward * DebugWorldSpawnDistance
				+ Vector3.Up * DebugWorldSpawnUpOffset;

			return Inventory.WorldTransform.WithPosition( position );
		}

		var fallbackPosition = WorldPosition
			+ WorldRotation.Forward * DebugWorldSpawnDistance
			+ Vector3.Up * DebugWorldSpawnUpOffset;

		return WorldTransform.WithPosition( fallbackPosition );
	}

	void LogResult( InventoryActionResult result )
	{
		Log.Info( result.ToString() );
	}

	// IInventoryDebugActions — wrappers for the library UI
	IReadOnlyList<InventoryDebugItemOption> IInventoryDebugActions.GetDebugItems() => DebugGiveItems;
	InventoryActionResult IInventoryDebugActions.AddItem( InventoryDebugItemOption option ) => DebugAddItem( option );
	InventoryActionResult IInventoryDebugActions.AddSmall() => DebugAddSmallItem();
	InventoryActionResult IInventoryDebugActions.AddLong() => DebugAddLongItem();
	InventoryActionResult IInventoryDebugActions.AddBackpack() => DebugAddBackpack();
	InventoryActionResult IInventoryDebugActions.SpawnBandage() => DebugSpawnBandageWorldItem( out _ );
	InventoryActionResult IInventoryDebugActions.SpawnWater() => DebugSpawnWaterWorldItem( out _ );
	InventoryActionResult IInventoryDebugActions.SpawnSmallBackpack() => DebugSpawnSmallBackpackWorldItem( out _ );
}
